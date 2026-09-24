using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using HR.Map;

namespace HR.Object.Player{
// Time Rewind: continuously records her own position server-side (see
// FixedUpdate) at RecordIntervalSeconds resolution. Once at least
// RecordWindowSeconds of history has built up, Activate() replays that
// whole recorded path BACKWARDS (see TargetRewind/RewindRoutine) to
// teleport her to where she was - not a straight-line jump to the oldest
// point. Can't be spammed: using it clears the history and resets the
// "how long until usable again" timer, so a fresh RecordWindowSeconds has
// to build back up before it's usable again, on top of the normal SP cost.
public class KroniiSkill : CharacterSkillBase
{
    KroniiRewindData RewindData => data as KroniiRewindData;
    float RecordWindowSeconds => RewindData != null ? RewindData.RecordWindowSeconds : 3f;
    float RewindDuration => RewindData != null ? RewindData.RewindDuration : 0.3f;
    const float RecordIntervalSeconds = 0.1f;

    struct PositionSample
    {
        public float time;
        public Vector3 position;
    }
    // Oldest at index 0, newest at the end - matches the order entries are
    // trimmed/appended in FixedUpdate below.
    readonly List<PositionSample> history = new();
    float nextRecordTime;
    // Time.time has to reach this before the skill can be used at all -
    // set to "now + RecordWindowSeconds" the moment recording starts, and
    // pushed out by the same amount again every time Activate() actually
    // succeeds (see there) - a recurring gate, not a one-time warmup.
    float nextUsableTime = -1f;

    // A separate Unity message from CharacterSkillBase's own (non-virtual)
    // Update() - using FixedUpdate here instead means this never risks
    // hiding/replacing the base class's SP-regen Update().
    [ServerCallback]
    void FixedUpdate()
    {
        if (nextUsableTime < 0f) nextUsableTime = Time.time + RecordWindowSeconds;
        if (Time.time < nextRecordTime) return;
        nextRecordTime = Time.time + RecordIntervalSeconds;

        history.Add(new PositionSample { time = Time.time, position = owner.transform.position });
        while (history.Count > 0 && Time.time - history[0].time > RecordWindowSeconds)
        {
            history.RemoveAt(0);
        }
    }

    // The condition half of CanUse (see CharacterSkillBase) - separate from
    // Activate() itself so UI can poll it every frame with no side effects.
    protected override bool CanUseExtra()
    {
        return history.Count > 0 && Time.time >= nextUsableTime;
    }

    protected override bool Activate()
    {
        if (!CanUseExtra()) return false;

        Vector3 oldest = history[0].position;
        Vector2Int cell = GridManager.Instance.WorldToGrid(oldest);
        if (GridManager.Instance.IsOccupied(cell) || GridManager.Instance.IsPlayerAt(cell)) return false;

        // Newest-first, oldest-last - RewindRoutine walks this front to
        // back, which plays the recorded path backwards through time and
        // ends up exactly at the oldest point.
        Vector3[] waypoints = new Vector3[history.Count];
        for (int i = 0; i < history.Count; i++)
        {
            waypoints[i] = history[history.Count - 1 - i].position;
        }

        TargetRewind(connectionToClient, waypoints, RecordWindowSeconds);

        // Can't rewind into a rewind - clear the trail and make her build
        // up a fresh RecordWindowSeconds before this is usable again.
        history.Clear();
        nextUsableTime = Time.time + RecordWindowSeconds;
        return true;
    }

    // Same client-authoritative-NetworkTransform reasoning as Korone's Jump
    // - a server-side transform.position write here would just get
    // overwritten by the owning client's next outgoing sync.
    [TargetRpc]
    void TargetRewind(NetworkConnection target, Vector3[] waypoints, float recordWindowSeconds)
    {
        StartCoroutine(RewindRoutine(waypoints));

        // Match the server-side reset (see Activate) so the ghost preview
        // disappears immediately instead of still pointing at a path that
        // no longer exists, and only reappears once a fresh
        // RecordWindowSeconds has actually passed again.
        localHistory.Clear();
        ghostReadyTime = Time.time + recordWindowSeconds;
        if (ghost != null) ghost.SetActive(false);
    }

    IEnumerator RewindRoutine(Vector3[] waypoints)
    {
        if (waypoints.Length == 0) yield break;

        owner.SetSkillLock(true);
        int segments = Mathf.Max(1, waypoints.Length - 1);
        float segmentDuration = RewindDuration / segments;

        // waypoints[0] is ~her current position already - start retracing
        // from index 1 onward, ending at waypoints[last] (the oldest point).
        for (int i = 1; i < waypoints.Length; i++)
        {
            Vector3 start = owner.transform.position;
            Vector3 destination = waypoints[i];
            float elapsed = 0f;
            while (elapsed < segmentDuration)
            {
                elapsed += Time.deltaTime;
                owner.transform.position = Vector3.Lerp(start, destination, elapsed / segmentDuration);
                yield return null;
            }
            owner.transform.position = destination; // guarantee an exact stop despite frame-time drift
        }
        owner.SetSkillLock(false);
    }

    // Ghost preview of where Activate() would currently rewind to - a
    // second, client-local copy of the same rolling history (not the
    // server-only `history` above, which a non-host client never runs
    // FixedUpdate for at all). Movement is client-authoritative anyway, so
    // this local copy tracks almost identically to the server's version.
    [SerializeField] GameObject GhostPrefab;
    GameObject ghost;
    // Fetched once off the instantiated ghost - it's a separate GameObject
    // from `owner`, so its Animator's "isMove" parameter is its own copy,
    // never touched by CharacterBase's movement code. Driven manually below
    // from the ghost's own frame-to-frame position instead, so it plays
    // run/idle matching whatever she was actually doing at that point in
    // her history, not just idling in place.
    Animator ghostAnimator;
    Vector3 lastGhostPosition;
    bool hasLastGhostPosition;
    // Below this, floating point jitter in the recorded positions alone
    // would flicker isMove on/off every frame while she's actually standing
    // still.
    const float GhostMoveThreshold = 0.001f;
    readonly Queue<PositionSample> localHistory = new();
    // Set once, the first time LateUpdate runs (and again after every
    // rewind - see TargetRewind), to "now + RecordWindowSeconds" - the
    // ghost stays hidden until real time actually reaches this, so it only
    // appears once there's a genuinely RecordWindowSeconds-old position to
    // show instead of popping in right on top of her. Once shown, it's
    // always visible and keeps following (not gated on SP/IsSkillReady -
    // it always reflects where Activate() would send her).
    float ghostReadyTime = -1f;

    // LateUpdate (not Update or Start) for the same hiding-the-base-class
    // reason FixedUpdate is used above (CharacterSkillBase's own Start()
    // isn't virtual either) - and also so the ghost is positioned after
    // this frame's own movement has already been applied. The ghost is
    // lazily created here on its first run rather than in a would-be
    // Start(), since isLocalPlayer isn't guaranteed to be valid yet at
    // Awake() time for a replaced player object.
    void LateUpdate()
    {
        if (!isLocalPlayer) return;

        if (ghost == null && GhostPrefab != null)
        {
            ghost = Instantiate(GhostPrefab);
            // includeInactive: true - SetActive(false) right below would
            // otherwise make this search come up empty, since the ghost's
            // whole hierarchy (Animator included) is inactive by then.
            ghostAnimator = ghost.GetComponentInChildren<Animator>(true);
            ghost.SetActive(false);
        }
        if (ghostReadyTime < 0f) ghostReadyTime = Time.time + RecordWindowSeconds;

        localHistory.Enqueue(new PositionSample { time = Time.time, position = owner.transform.position });
        while (localHistory.Count > 0 && Time.time - localHistory.Peek().time > RecordWindowSeconds)
        {
            localHistory.Dequeue();
        }

        if (ghost == null) return;
        if (localHistory.Count == 0 || Time.time < ghostReadyTime)
        {
            ghost.SetActive(false);
            hasLastGhostPosition = false; // don't compare against a stale position once it reappears
        }
        else
        {
            ghost.SetActive(true);
            Vector3 nextPosition = localHistory.Peek().position;
            if (ghostAnimator != null)
            {
                bool moved = hasLastGhostPosition
                    && (nextPosition - lastGhostPosition).sqrMagnitude > GhostMoveThreshold * GhostMoveThreshold;
                ghostAnimator.SetBool("isMove", moved);
            }
            ghost.transform.position = nextPosition;
            lastGhostPosition = nextPosition;
            hasLastGhostPosition = true;
        }
    }
}
}
