using UnityEditor;
using UnityEngine;
using HR.Map;

namespace HR.GUI{
public static class GridSnapTools
{
    [MenuItem("HoloBomber/Snap Selected To Grid")]
    static void SnapSelectedToGrid()
    {
        if (!TryGetGridManager(out GridManager manager)) return;

        Transform[] targets = Selection.transforms;
        foreach (Transform t in targets)
        {
            SnapTransform(manager, t);
        }
        Debug.Log($"Snapped {targets.Length} selected object(s) to grid.");
    }

    [MenuItem("HoloBomber/Snap Selected To Grid", true)]
    static bool ValidateSnapSelectedToGrid() => Selection.transforms.Length > 0;

    // Scans every GridCell in the currently open scene(s) - Spawn/
    // MinionSpawn/Wall/Destructible/Bomb markers, including inactive ones -
    // and snaps each to its nearest grid cell center, so hand-placed markers
    // line up with GridSpawnerEditor's generated tiles without needing to
    // select them all by hand first.
    [MenuItem("HoloBomber/Snap All Grid Cells In Scene")]
    static void SnapAllGridCellsInScene()
    {
        if (!TryGetGridManager(out GridManager manager)) return;

        GridCell[] cells = UnityEngine.Object.FindObjectsOfType<GridCell>(true);
        foreach (GridCell cell in cells)
        {
            SnapTransform(manager, cell.transform);
        }
        Debug.Log($"Snapped {cells.Length} GridCell object(s) to grid.");
    }

    static void SnapTransform(GridManager manager, Transform t)
    {
        Vector2Int coord = manager.WorldToGrid(t.position);
        Vector3 snapped = manager.GridToWorld(coord);

        Undo.RecordObject(t, "Snap To Grid");
        // Keep each object's own Y - only X/Z are meaningful grid coords.
        t.position = new Vector3(snapped.x, t.position.y, snapped.z);
    }

    static bool TryGetGridManager(out GridManager manager)
    {
        manager = GridManager.Instance;
        if (manager == null)
        {
            Debug.LogWarning("No GridManager in the scene - can't snap to grid.");
            return false;
        }
        return true;
    }
}
}
