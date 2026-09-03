using Mirror;
using UnityEngine;
using HR.Map;

namespace HR.Object.Minions{
// Scene-placed, one per map (same pattern as SupplyRun/MapChecker) - spawns
// one minion at each level-designed spawn marker (CellType.MinionSpawn,
// placed via a GridCell in the Editor) once, at match start. Not a
// periodic loop like SupplyRun - a minion persists and wanders for the
// whole match once spawned (see Minion's instant in-place revive), so
// there's no need to keep spawning more over time.
public class MinionSpawner : NetworkBehaviour
{
    [SerializeField] GameObject[] minionPrefabs;

    void Start()
    {
        if (!isServer) return;
        if (minionPrefabs == null || minionPrefabs.Length == 0) return;

        foreach (Vector2Int coord in GridManager.Instance.GetAllMinionSpawnCoords())
        {
            GameObject prefab = minionPrefabs[Random.Range(0, minionPrefabs.Length)];
            GameObject minion = Instantiate(prefab, GridManager.Instance.GridToWorld(coord), Quaternion.identity);
            NetworkServer.Spawn(minion);
        }
    }
}
}
