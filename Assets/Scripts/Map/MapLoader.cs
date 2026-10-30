using Mirror;
using UnityEngine;
using HR.Network;

namespace HR.Map{
// Lives in Game_Map_Runtime.unity. Reads the active MapConfig's text map and
// builds the grid at runtime instead of it being baked into the scene.
//
// Plain MonoBehaviour, not NetworkBehaviour - Floor/Wall/Bush/Spawn/
// MinionSpawn need no sync at all (same as GridCell today, which has no
// SyncVars): every peer parses the identical text file and builds an
// identical local grid independently, exactly mirroring how today's
// scene-baked maps give every peer the same layout for free. Destructible
// AND Bush are the exceptions - see BuildSymbol() below (BushBlock is also a
// NetworkBehaviour, same as DestructibleBlock, despite not blocking
// movement).
//
// Must run before MapChecker (both fire in Awake) - set via Project
// Settings > Script Execution Order, otherwise MapChecker's spawn-count
// check can run against an empty GridManager and report a false error.
public class MapLoader : MonoBehaviour
{
    [SerializeField] MapBlockPalette defaultPalette;
    [SerializeField] float cellSize = 1f; // must match GridManager's own cellSize

    // Read by MapChecker to report a specific root cause instead of just the
    // knock-on "no spawns registered" symptom.
    public static string LastError;

    void Awake()
    {
        LastError = null;

        MapConfig config = ResolveConfig();
        if (config == null || config.MapFile == null)
        {
            LastError = "No text map file configured for the current MapConfig.";
            Debug.LogError(LastError);
            return;
        }

        MapParseResult map = MapTextParser.Parse(config.MapFile.text);
        if (!map.Success)
        {
            LastError = string.Join("; ", map.Errors);
            Debug.LogError($"Failed to parse map '{config.MapId}': {LastError}");
            return;
        }

        MapBlockPalette palette = config.PaletteOverride != null ? config.PaletteOverride : defaultPalette;
        if (palette == null)
        {
            LastError = "No MapBlockPalette available (neither MapConfig.PaletteOverride nor MapLoader's default is set).";
            Debug.LogError(LastError);
            return;
        }

        Build(map, palette);
    }

    MapConfig ResolveConfig()
    {
        var manager = Network_Manager.singleton as Network_Manager;
        if (manager == null || GameSettings.Instance == null) return null;

        string mapId = GameSettings.Instance.MapName;
        return manager.mapConfigList.Find(c => c.MapId == mapId);
    }

    void Build(MapParseResult map, MapBlockPalette palette)
    {
        // Same centering formula as GridSpawnerEditor's procedural layout, so
        // a text map lines up with GridManager (cellSize = 1 world unit per
        // cell) exactly the same way the old baked maps do.
        float offsetX = (map.Width / 2) * cellSize;
        float offsetZ = (map.Height / 2) * cellSize;

        for (int row = 0; row < map.Height; row++)
        {
            for (int col = 0; col < map.Width; col++)
            {
                Vector3 pos = new Vector3(
                    col * cellSize - offsetX,
                    0f,
                    row * cellSize - offsetZ);

                if (palette.FloorPrefab != null)
                {
                    Instantiate(palette.FloorPrefab, pos, Quaternion.identity, transform);
                }

                BuildSymbol(map.Grid[row][col], pos, palette);
            }
        }
    }

    void BuildSymbol(char symbol, Vector3 floorPos, MapBlockPalette palette)
    {
        switch (symbol)
        {
            case '.':
                break;

            case '#':
                Instantiate(palette.WallPrefab, BlockPosition(palette.WallPrefab, floorPos), Quaternion.identity, transform);
                break;

            case '~':
                // Bush is a NetworkBehaviour (BushBlock) despite not blocking
                // movement - same server-only spawn path as Destructible, see
                // the 'X' case below. A plain local Instantiate here would
                // create a NetworkIdentity with no valid sceneId, which Mirror
                // rejects at build time ("needs to be opened and resaved...").
                SpawnNetworked(palette.BushPrefab, BlockPosition(palette.BushPrefab, floorPos));
                break;

            case 'M':
                SpawnMarker(palette.MinionSpawnMarkerPrefab, floorPos, CellType.MinionSpawn, 0);
                break;

            case 'X':
                // Destructible can be broken, and that has to sync. Only the
                // server creates it (NetworkServer.Spawn) - clients receive
                // their copy through Mirror's normal replication instead of
                // also building one themselves from the text file, same as
                // today's scene-baked BreakableWall.prefab (already a
                // NetworkIdentity + DestructibleBlock : NetworkBehaviour).
                SpawnNetworked(palette.DestructiblePrefab, BlockPosition(palette.DestructiblePrefab, floorPos));
                break;

            default:
                if (symbol >= '1' && symbol <= '8')
                {
                    SpawnMarker(palette.SpawnMarkerPrefab, floorPos, CellType.Spawn, symbol - '0');
                }
                break;
        }
    }

    // Matches GridSpawnerEditor's obstacle stacking formula
    // (Assets/Scripts/GridSpawnerEditor.cs:137-146): floor sits at y=0, and a
    // block's pivot is centered half its own rendered height above that, so
    // its bottom rests on the floor instead of sinking into/floating level
    // with it.
    Vector3 BlockPosition(GameObject prefab, Vector3 floorPos)
    {
        Renderer r = prefab.GetComponentInChildren<Renderer>();
        float height = r != null ? r.bounds.size.y : 1f;
        return floorPos + Vector3.up * (height * 0.5f);
    }

    // Server-only Instantiate + NetworkServer.Spawn, for any prefab that's a
    // NetworkIdentity (Destructible, Bush) - clients get their copy through
    // Mirror's own replication instead of calling this themselves.
    void SpawnNetworked(GameObject prefab, Vector3 pos)
    {
        if (!NetworkServer.active) return;
        GameObject instance = Instantiate(prefab, pos, Quaternion.identity);
        NetworkServer.Spawn(instance);
    }

    // Local-only, like Floor/Wall - Spawn/MinionSpawn markers have no prefab
    // today (bare GameObject + GridCell), so this assumes no NetworkIdentity.
    // If a marker prefab is ever added, check it the same way Bush turned out
    // to need SpawnNetworked() instead of this.
    void SpawnMarker(GameObject prefab, Vector3 pos, CellType type, int index)
    {
        GameObject go = prefab != null
            ? Instantiate(prefab, pos, Quaternion.identity, transform)
            : new GameObject(type.ToString());

        if (prefab == null)
        {
            go.transform.SetParent(transform);
            go.transform.position = pos;
        }

        GridCell cell = go.GetComponent<GridCell>();
        if (cell == null) cell = go.AddComponent<GridCell>();
        cell.type = type;
        cell.spawnIndex = index;
        // Vestigial today - GridManager's spawn-point pools don't scope by
        // team (see GetRandomUnusedSpawnPosition/GetRandomSpawnPosition),
        // kept only for schema completeness/forward-compat.
        cell.teamId = index;
    }
}
}
