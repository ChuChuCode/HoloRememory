using System.Collections.Generic;
using UnityEngine;
using HR.Object.Skill;
using HR.Map;

namespace HR.Object.Player{
// Muscle Throw: only usable when there's a bomb sitting under Noel's own
// feet (the cell she's standing on, not one in front of her - a freshly
// placed bomb's collider stays a trigger while she's still on top of it,
// see BombBase.OnTriggerEnter/Exit, so this works immediately after
// placing one without having to step off first), and only if it's HER OWN
// bomb (checked via BombBase.Owner). Tosses it in an arc ThrowDistance
// cells forward in her facing direction; if that cell is blocked, it
// visibly bounces another ThrowDistance cells further in the SAME
// direction (see BombBase.ThrowBounce - every attempted cell gets its own
// arc hop, not just the final one) until it lands somewhere clear.
// Running off one edge of the map wraps it around to the opposite edge -
// that specific hop snaps instantly instead of arcing (see
// BombBase.ThrowBounce's `wrapped` list), since animating a normal-speed
// arc across the entire map's width just reads as a glitchy teleport. If
// no clear landing spot exists within MaxBounces attempts, the throw
// doesn't happen at all (free no-op) instead of bouncing back to where it
// started - same "no valid target = nothing happens, no SP spent" pattern
// as every other skill here.
public class NoelSkill : CharacterSkillBase
{
    NoelThrowData ThrowData => data as NoelThrowData;
    int ThrowDistance => ThrowData != null ? ThrowData.ThrowDistance : 2;
    // Same stat Watame's push scales its distance by - not a separate
    // tunable, so a stronger Fire Power (from a BombPower item) naturally
    // also means more bounces/reach here too.
    int MaxBounces => owner.bombPower;
    float ThrowDuration => ThrowData != null ? ThrowData.ThrowDuration : 0.35f;
    float ArcHeight => ThrowData != null ? ThrowData.ArcHeight : 1.2f;

    // Mirrors Activate()'s own checks below, without the side effect - see
    // CharacterSkillBase.CanUseExtra.
    protected override bool CanUseExtra()
    {
        if (!TryGetOwnBomb(out Vector2Int bombCoord, out BombBase bomb)) return false;
        FindLandingPath(bombCoord, owner.FacingDir, out _, out bool found);
        return found;
    }

    protected override bool Activate()
    {
        if (!TryGetOwnBomb(out Vector2Int bombCoord, out BombBase bomb)) return false;

        List<Vector2Int> path = FindLandingPath(bombCoord, owner.FacingDir, out List<bool> wrapped, out bool found);
        if (!found) return false; // nowhere to land within MaxBounces - don't throw at all

        bomb.ThrowBounce(path, wrapped, ThrowDuration, ArcHeight);
        return true;
    }

    bool TryGetOwnBomb(out Vector2Int bombCoord, out BombBase bomb)
    {
        bombCoord = GridManager.Instance.WorldToGrid(owner.transform.position);
        bomb = null;
        if (!GridManager.Instance.TryGetCell(bombCoord, out GridCell cell) || cell.type != CellType.Bomb) return false;
        bomb = cell.GetComponent<BombBase>();
        return bomb != null && bomb.Owner == owner;
    }

    // Keeps hopping ThrowDistance cells at a time in `direction` from
    // `start`, wrapping around the map edge (see GridManager.TryGetBounds)
    // whenever a hop would otherwise run off it, recording every attempted
    // cell along the way (including blocked ones - BombBase.ThrowBounce
    // visibly bounces through each) until landing on a cell that isn't
    // blocked (`found` = true) or MaxBounces attempts run out (`found` =
    // false - caller should treat this as no valid throw at all). Capped
    // at MaxBounces attempts - without a small hard cap here, a clear run
    // could bounce (and wrap) all the way around the map, letting her
    // threaten anywhere from anywhere the instant she has a bomb down,
    // which is a lot stronger than intended for an opening move.
    List<Vector2Int> FindLandingPath(Vector2Int start, Vector2Int direction, out List<bool> wrapped, out bool found)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        wrapped = new List<bool>();
        found = false;

        if (!GridManager.Instance.TryGetBounds(out int minX, out int maxX, out int minZ, out int maxZ))
        {
            return path;
        }
        int walkMinX = minX + 1, walkMaxX = maxX - 1;
        int walkMinZ = minZ + 1, walkMaxZ = maxZ - 1;

        Vector2Int pos = start;
        for (int i = 0; i < MaxBounces; i++)
        {
            Vector2Int rawNext = pos + direction * ThrowDistance;
            Vector2Int next = new Vector2Int(
                Wrap(rawNext.x, walkMinX, walkMaxX),
                Wrap(rawNext.y, walkMinZ, walkMaxZ));
            bool didWrap = next != rawNext;

            path.Add(next);
            wrapped.Add(didWrap);

            if (!GridManager.Instance.IsOccupied(next) && !GridManager.Instance.IsPlayerAt(next))
            {
                found = true;
                return path; // this one's clear - lands here
            }
            pos = next; // still blocked - bounce through it visually, keep going
        }
        return path; // ran out of bounces - found stays false, nothing to throw
    }

    static int Wrap(int value, int min, int max)
    {
        int range = max - min + 1;
        if (range <= 0) return value;
        int offset = (value - min) % range;
        if (offset < 0) offset += range;
        return min + offset;
    }
}
}
