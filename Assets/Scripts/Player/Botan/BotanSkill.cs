using UnityEngine;
using Mirror;
using HR.Object.Skill;
using HR.Map;

namespace HR.Object.Player{
// SSRB: locks onto the nearest enemy the instant the skill is used, then
// spawns an SSRBBomb that chases them a few cells before arming and
// exploding through the normal Bomb system. Doesn't touch the owner's
// normal bomb count - it's a separate skill object entirely.
public class BotanSkill : CharacterSkillBase
{
    [SerializeField] SSRBBomb SSRBPrefab;
    BotanSSRBData Data => data as BotanSSRBData;

    // UnityEngine.Object's == override treats a destroyed SSRB as null
    // automatically, so this doesn't need to be cleared by hand once it
    // explodes.
    SSRBBomb activeSSRB;

    protected override bool Activate()
    {
        if (activeSSRB != null) return false; // only one SSRB at a time, per spec

        // No living enemy -> chase the nearest destructible block instead of
        // just doing nothing. Otherwise the skill would be untestable (and
        // unusable) with nobody else left standing nearby.
        Transform target = FindNearestEnemy()?.transform ?? FindNearestDestructibleBlock();
        if (target == null) return false; // no enemy AND no block anywhere - free no-op

        Vector2Int coord = GridManager.Instance.WorldToGrid(owner.transform.position);
        Vector3 spawnPos = GridManager.Instance.GridToWorld(coord);

        SSRBBomb ssrb = Instantiate(SSRBPrefab, spawnPos, Quaternion.identity);
        ssrb.SetOwner(owner);
        ssrb.SetPower(owner.bombPower); // Fire Range = the caster's own Fire Range
        ssrb.SetCountsTowardBombLimit(false);
        ssrb.SetFuseTime(Data != null ? Data.FuseTime : 1.5f);
        ssrb.Configure(target, Data != null ? Data.MaxChaseDistance : 3, Data != null ? Data.MoveInterval : 0.3f);
        NetworkServer.Spawn(ssrb.gameObject);

        activeSSRB = ssrb;
        return true;
    }

    CharacterBase FindNearestEnemy()
    {
        CharacterBase nearest = null;
        float nearestDistance = float.MaxValue;
        foreach (CharacterBase candidate in owner.Manager.Player_List)
        {
            if (candidate == owner || candidate.isDead) continue;
            float distance = Vector3.Distance(candidate.transform.position, owner.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = candidate;
            }
        }
        return nearest;
    }

    Transform FindNearestDestructibleBlock()
    {
        GridCell nearest = null;
        float nearestDistance = float.MaxValue;
        foreach (GridCell cell in FindObjectsOfType<GridCell>())
        {
            if (cell.type != CellType.Destructible) continue;
            float distance = Vector3.Distance(cell.transform.position, owner.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = cell;
            }
        }
        return nearest != null ? nearest.transform : null;
    }
}
}
