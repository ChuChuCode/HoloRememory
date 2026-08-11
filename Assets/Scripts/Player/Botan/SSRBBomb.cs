using System.Collections;
using UnityEngine;
using HR.Object.Player;
using HR.Map;

namespace HR.Object.Skill{
// Botan's skill bomb: chases whichever enemy was locked at spawn, one grid
// cell at a time, up to MaxChaseDistance hops or until something blocks it -
// then arms with the normal fuse/explosion pipeline. SpawnExplosion itself
// is entirely inherited from BombBase, untouched, so it reuses the exact
// same fire range / block destruction / chain reaction / item drop system
// every normal bomb already uses.
public class SSRBBomb : BombBase
{
    // A Transform rather than CharacterBase - the fallback target when no
    // enemy is around is the nearest destructible block, which has no
    // CharacterBase at all.
    Transform chaseTarget;
    int maxChaseDistance;
    float moveInterval;

    // Set right after Instantiate, before NetworkServer.Spawn - same timing
    // as the inherited SetOwner/SetPower/SetFuseTime.
    public void Configure(Transform chaseTarget, int maxChaseDistance, float moveInterval)
    {
        this.chaseTarget = chaseTarget;
        this.maxChaseDistance = maxChaseDistance;
        this.moveInterval = moveInterval;
    }

    // Deliberately doesn't call base.Start() - a normal bomb starts
    // counting its fuse down the instant it's placed, but SSRB's fuse only
    // starts once it's done chasing (see ChaseRoutine).
    protected override void Start()
    {
        if (!isServer) return;
        StartCoroutine(ChaseRoutine());
    }

    IEnumerator ChaseRoutine()
    {
        for (int i = 0; i < maxChaseDistance; i++)
        {
            Vector2Int current = gridCell.Coord;
            Vector2Int targetCoord = GridManager.Instance.WorldToGrid(chaseTarget.position);
            Vector2Int delta = targetCoord - current;
            if (delta == Vector2Int.zero) break;

            // Horizontal first, then vertical - simple axis-priority chase,
            // not real pathfinding, per spec.
            Vector2Int dir = delta.x != 0
                ? new Vector2Int((int)Mathf.Sign(delta.x), 0)
                : new Vector2Int(0, (int)Mathf.Sign(delta.y));
            Vector2Int next = current + dir;

            // Walls, destructibles and other bombs all block it the same
            // way they'd block a walking player.
            if (GridManager.Instance.IsOccupied(next)) break;

            // Slide across the hop over moveInterval instead of snapping -
            // still a discrete grid decision underneath (MoveToCellRoutine
            // re-registers immediately, not once the slide finishes), just
            // visibly a chase instead of a teleport.
            yield return MoveToCellRoutine(next, moveInterval);

            // chaseTarget == null covers a destroyed block target; a player
            // target's GameObject never gets destroyed on death, so that
            // case needs its own isDead check instead. Checked after the
            // slide so the next iteration's direction calc doesn't run
            // against a stale/invalid target.
            if (chaseTarget == null) break;
            if (chaseTarget.TryGetComponent(out CharacterBase character) && character.isDead) break;
        }

        Invoke(nameof(SpawnExplosion), BombTime);
    }
}
}
