using System.Collections;
using UnityEngine;
using Mirror;
using HR.Object.Skill;
using HR.Map;

namespace HR.Object.Player{
// Bomb Push: only usable when there's a bomb directly in front of Watame.
// Push distance comes from her current Fire Power (bombPower) - the same
// stat that already drives her own explosion radius - not a separate
// tunable, so a stronger Fire Power naturally also means a stronger push.
public class WatameSkill : CharacterSkillBase
{
    WatameBombPushData Data => data as WatameBombPushData;

    protected override bool Activate()
    {
        Vector2Int bombCoord = GridManager.Instance.WorldToGrid(owner.transform.position) + owner.FacingDir;
        if (!GridManager.Instance.TryGetCell(bombCoord, out GridCell cell) || cell.type != CellType.Bomb)
        {
            return false; // no bomb directly in front - nothing to push
        }

        BombBase bomb = cell.GetComponent<BombBase>();
        if (bomb == null) return false;

        // Walk forward up to bombPower cells, stopping at the last free
        // cell before anything blocking (wall/destructible/another bomb, or
        // a player standing in the way - treated the same as a wall here).
        int maxDistance = owner.bombPower;
        Vector2Int destination = bombCoord;
        int distanceTraveled = 0;
        for (int i = 1; i <= maxDistance; i++)
        {
            Vector2Int next = bombCoord + owner.FacingDir * i;
            if (GridManager.Instance.IsOccupied(next) || GridManager.Instance.IsPlayerAt(next)) break;
            destination = next;
            distanceTraveled = i;
        }

        float durationPerCell = Data != null ? Data.DurationPerCell : 0.1f;
        float pushDuration = distanceTraveled * durationPerCell;
        bomb.MoveToCell(destination, pushDuration);
        // Activate() runs on the server, but movement is client-authoritative
        // - only the owning client's own FixedUpdate actually needs to stop,
        // so this has to reach her the same way the push itself is felt.
        TargetLockDuringPush(connectionToClient, pushDuration);
        return true; // bumping the bomb counts as "used" even if it didn't move (e.g. wall right behind it)
    }

    [TargetRpc]
    void TargetLockDuringPush(NetworkConnection target, float duration)
    {
        StartCoroutine(LockDuringPushRoutine(duration));
    }
    IEnumerator LockDuringPushRoutine(float duration)
    {
        owner.SetSkillLock(true);
        yield return new WaitForSeconds(duration);
        owner.SetSkillLock(false);
    }
}
}
