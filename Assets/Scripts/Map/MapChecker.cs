using UnityEngine;

namespace HR.Map{
// Place one per map Scene - validates the map has the setup every map
// needs, so a broken/incomplete map fails loudly at scene start instead of
// silently misbehaving mid-match. Errors (not warnings) for anything
// critical - a soft warning is easy to miss and this only runs once.
// Right-click the component (or the gear icon) in the Inspector and pick
// "Validate Map" to re-run this on demand, in Edit mode, without pressing
// Play at all.
public class MapChecker : MonoBehaviour
{
    void Awake()
    {
        Validate();
    }

    [ContextMenu("Validate Map")]
    void Validate()
    {
        string sceneName = gameObject.scene.name;

        // Checked first so a bad text map (Game_Map_Runtime) reports its
        // actual root cause instead of just the knock-on "no spawns
        // registered" symptom below.
        if (MapLoader.LastError != null)
        {
            Debug.LogError($"[{sceneName}] Map text failed to load: {MapLoader.LastError}", this);
        }

        GridManager gridManager = FindObjectOfType<GridManager>();
        if (gridManager == null)
        {
            Debug.LogError($"[{sceneName}] No GridManager found in this scene - nothing grid-based will work.", this);
            return; // nothing else here can be checked without one
        }

        int spawnCount = 0;
        foreach (Vector2Int _ in gridManager.GetAllSpawnCoords())
        {
            spawnCount++;
        }
        if (spawnCount == 0)
        {
            Debug.LogError($"[{sceneName}] No Spawn points registered on this map - players can't be placed.", this);
        }

        if (FindObjectOfType<SupplyRun>() == null)
        {
            Debug.LogWarning($"[{sceneName}] No SupplyRun in this scene - items will only ever come from breaking blocks.", this);
        }
    }
}
}
