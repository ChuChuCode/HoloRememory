using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using HR.Object.Skill;
using HR.Map;

namespace HR.Object.Player{
// Hawk Eye: for a few seconds, highlights every bomb currently on the map's
// blast range (candidate B from the design doc - "Bomb Range", not the
// timer candidate). Purely a local visual on the caster's own client - the
// highlight markers are plain Instantiate() calls, never NetworkServer.Spawn,
// so nothing here needs to be networked beyond telling the caster's own
// client to start/stop.
public class RuiSkill : CharacterSkillBase
{
    [SerializeField] GameObject highlightPrefab;
    RuiHawkEyeData Data => data as RuiHawkEyeData;

    List<GameObject> activeHighlights = new List<GameObject>();

    protected override bool Activate()
    {
        float duration = Data != null ? Data.Duration : 10f;
        float refreshInterval = Data != null ? Data.RefreshInterval : 0.15f;
        TargetStartHawkEye(connectionToClient, duration, refreshInterval);
        return true;
    }

    // Activate() runs on the server (via CmdActivate), so the visual can
    // only actually start from here, on the caster's own client.
    [TargetRpc]
    void TargetStartHawkEye(NetworkConnection target, float duration, float refreshInterval)
    {
        StartCoroutine(HawkEyeRoutine(duration, refreshInterval));
    }

    IEnumerator HawkEyeRoutine(float duration, float refreshInterval)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            RefreshHighlights();
            yield return new WaitForSeconds(refreshInterval);
            elapsed += refreshInterval;
        }
        ClearHighlights();
    }

    // Bombs can move (pushed, or an SSRB chasing), get placed, or explode
    // during the effect's duration, so this recomputes from scratch on a
    // timer instead of just once at activation.
    void RefreshHighlights()
    {
        ClearHighlights();
        // GridToWorld always returns y=0, and bombs themselves are placed
        // using that same flat assumption - neither actually reflects the
        // floor's real height. owner (Rui herself) rests on the floor via
        // normal physics, so her own y is the one reference here that's
        // guaranteed correct.
        float y = owner.transform.position.y + 0.02f;
        Color freshColor = Data != null ? Data.FreshFuseColor : new Color(1f, 0.85f, 0.1f);
        Color aboutToExplodeColor = Data != null ? Data.AboutToExplodeColor : new Color(1f, 0.1f, 0.05f);
        foreach (BombBase bomb in FindObjectsOfType<BombBase>())
        {
            Color color = Color.Lerp(freshColor, aboutToExplodeColor, bomb.FuseProgress());
            foreach (Vector2Int cell in bomb.GetBlastCells())
            {
                Vector3 worldXZ = GridManager.Instance.GridToWorld(cell);
                Vector3 position = new Vector3(worldXZ.x, y, worldXZ.z);
                GameObject highlight = Instantiate(highlightPrefab, position, Quaternion.identity);
                Renderer rend = highlight.GetComponentInChildren<Renderer>();
                if (rend != null)
                {
                    MaterialPropertyBlock block = new MaterialPropertyBlock();
                    block.SetColor("_BaseColor", color);
                    rend.SetPropertyBlock(block);
                }
                activeHighlights.Add(highlight);
            }
        }
    }

    void ClearHighlights()
    {
        foreach (GameObject highlight in activeHighlights)
        {
            if (highlight != null) Destroy(highlight);
        }
        activeHighlights.Clear();
    }
}
}
