using System.Collections;
using UnityEngine;
using Mirror;
using HR.Map;

namespace HR.Object.Player{
// Jump: hop over the SkipCells directly in front of Korone and land just
// past them. Built for "越過炸彈 / 逃離爆炸" - clearing a bomb or hazard
// tile sitting right next to her, not a long-range dash.
public class KoroneSkill : CharacterSkillBase
{
    KoroneJumpData JumpData => data as KoroneJumpData;
    int SkipCells => JumpData != null ? JumpData.SkipCells : 1;

    protected override bool Activate()
    {
        Vector2Int dir = owner.FacingDir;
        Vector2Int current = GridManager.Instance.WorldToGrid(owner.transform.position);

        // Walls, bombs, destructibles - anything sitting in the hopped
        // cells - are all fair game to jump over. Only the landing cell
        // itself has to be clear.
        Vector2Int landing = current + dir * (SkipCells + 1);
        if (GridManager.Instance.IsOccupied(landing))
        {
            return false;
        }

        Vector3 landingPos = GridManager.Instance.GridToWorld(landing);
        landingPos.y = owner.transform.position.y;
        TargetJump(connectionToClient, landingPos);
        return true;
    }

    // The player object's NetworkTransform is client-authoritative, so a
    // position change made here on the server (inside Activate(), called
    // from the [Command] CmdActivate) would just get overwritten by the
    // owning client's next outgoing transform update. Moving it on the
    // owning client itself is what actually sticks and propagates out -
    // and since it's continuously updating transform.position over time
    // (not an instant set), the same client-authoritative sync that
    // already replicates normal WASD movement carries the arc out to
    // every other client too, not just the owning client's own screen.
    [TargetRpc]
    void TargetJump(NetworkConnection target, Vector3 destination)
    {
        StartCoroutine(JumpRoutine(destination));
    }

    IEnumerator JumpRoutine(Vector3 destination)
    {
        float duration = JumpData != null ? JumpData.JumpDuration : 0.3f;
        float arcHeight = JumpData != null ? JumpData.ArcHeight : 1f;
        Vector3 start = owner.transform.position;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            Vector3 flat = Vector3.Lerp(start, destination, t);
            float arc = arcHeight * Mathf.Sin(t * Mathf.PI); // 0 at takeoff/landing, peaks at the midpoint
            owner.transform.position = new Vector3(flat.x, flat.y + arc, flat.z);
            yield return null;
        }
        owner.transform.position = destination; // guarantee an exact landing despite frame-time drift
    }
}
}
