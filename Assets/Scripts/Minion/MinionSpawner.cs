using System.Collections.Generic;
using Mirror;
using UnityEngine;
using HR.Map;

namespace HR.Object.Minions{
// Scene-placed, one per map (same pattern as SupplyRun/MapChecker) - spawns
// minions at level-designed spawn markers (CellType.MinionSpawn, placed via
// a GridCell in the Editor). The initial full spawn happens once at match
// start (Start()); SpawnWave() is also called later by a solo level's
// LevelEvent (Network_Manager.FireLevelEvent) for a mid-match extra wave -
// a minion persists and wanders once spawned (see Minion's instant
// in-place revive), so there's never a need for a periodic re-spawn loop.
public class MinionSpawner : NetworkBehaviour
{
    // Same singleton pattern as GridManager.Instance - one of these per map,
    // looked up by Network_Manager when a LevelEvent wants a mid-match wave.
    public static MinionSpawner Instance;

    [SerializeField] GameObject[] minionPrefabs;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        if (!isServer) return;
        SpawnWave(int.MaxValue); // every MinionSpawn point, same as before this refactor
    }

    // Spawns up to `count` minions across the available CellType.MinionSpawn
    // points - a random subset of those points if there are more of them
    // than count. No occupancy check against already-alive minions (the
    // original Start() loop never had one either - a second wave can land on
    // the same point as a survivor, same pre-existing gap, not new here).
    public void SpawnWave(int count)
    {
        if (!isServer) return;
        if (minionPrefabs == null || minionPrefabs.Length == 0) return;

        List<Vector2Int> coords = new List<Vector2Int>(GridManager.Instance.GetAllMinionSpawnCoords());
        Shuffle(coords);

        for (int i = 0; i < coords.Count && i < count; i++)
        {
            GameObject prefab = minionPrefabs[Random.Range(0, minionPrefabs.Length)];
            GameObject minion = Instantiate(prefab, GridManager.Instance.GridToWorld(coords[i]), Quaternion.identity);
            NetworkServer.Spawn(minion);
        }
    }

    static void Shuffle(List<Vector2Int> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
}
