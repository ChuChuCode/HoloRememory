using UnityEngine;

namespace HR.Map{
// One global default palette is enough today - CellType is a small closed
// enum and there's only one visual tileset (the placeholder prefabs). A
// MapConfig can still override this per-map later (see MapConfig.PaletteOverride)
// without any changes here.
[CreateAssetMenu(fileName = "MapBlockPalette", menuName = "HoloBomber/Map Block Palette")]
public class MapBlockPalette : ScriptableObject
{
    [Header("Instantiated under every cell, regardless of symbol")]
    public GameObject FloorPrefab;

    [Header("Per-symbol blocks")]
    public GameObject WallPrefab;          // '#'
    public GameObject DestructiblePrefab;  // 'X' - must have a NetworkIdentity, see MapLoader
    public GameObject BushPrefab;          // '~'

    [Header("Markers (may be left null - MapLoader adds a bare GridCell instead)")]
    public GameObject SpawnMarkerPrefab;       // '1'-'8'
    public GameObject MinionSpawnMarkerPrefab; // 'M'
}
}
