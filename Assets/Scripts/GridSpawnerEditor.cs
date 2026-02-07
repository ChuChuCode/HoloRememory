using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class GridSpawnerEditor : MonoBehaviour
{
    [Header("Grid Size (Floor)")]
    public int width = 13;
    public int height = 11;
    public float cellSize = 1f;

    [Header("Prefabs")]
    public GameObject[] floorPrefabs;
    public GameObject wallPrefab;

    [Header("Floor Options")]
    public bool randomFloorPrefab = false;

    [Header("Wall / Obstacle Settings")]
    public int obstacleLayers = 2;          // 疊幾層（2 = 人物爬不上）

    [Header("Obstacle Spacing")]
    [Min(1)]
    public int wallSpacing = 2;              // ⭐ 可調整牆壁間隔

    [Header("Inner Obstacle Random")]
    public bool randomInnerWalls = false;
    [Range(0f, 1f)]
    public float randomObstacleChance = 0.4f;

    [ContextMenu("Generate Grid")]
    public void GenerateGrid()
    {
#if UNITY_EDITOR
        ClearGrid();

        if (floorPrefabs == null || floorPrefabs.Length == 0 || wallPrefab == null)
        {
            Debug.LogError("Missing prefabs");
            return;
        }

        Undo.RegisterFullObjectHierarchyUndo(gameObject, "Generate Grid");

        float offsetX = (width - 1) * cellSize / 2f;
        float offsetZ = (height - 1) * cellSize / 2f;

        // ========================
        // Generate Floor
        // ========================
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 pos = new Vector3(
                    x * cellSize - offsetX,
                    0f,
                    y * cellSize - offsetZ
                );

                GameObject prefab = randomFloorPrefab
                    ? floorPrefabs[Random.Range(0, floorPrefabs.Length)]
                    : floorPrefabs[(x + y) % floorPrefabs.Length];

                GameObject floor = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                floor.transform.SetParent(transform);
                floor.transform.localPosition = pos;
                floor.name = $"Floor_{x}_{y}";

                Undo.RegisterCreatedObjectUndo(floor, "Create Floor");
            }
        }

        // ========================
        // Obstacle Grid (Character Scale)
        // ========================
        float wallCellSize = cellSize / 2f;
        int wallGridWidth = width * 2;
        int wallGridHeight = height * 2;

        float halfWidth = width * cellSize / 2f;
        float halfHeight = height * cellSize / 2f;

        for (int x = 0; x < wallGridWidth; x++)
        {
            for (int y = 0; y < wallGridHeight; y++)
            {
                bool isBorder =
                    x == 0 || y == 0 ||
                    x == wallGridWidth - 1 || y == wallGridHeight - 1;

                bool isSpawnSafe =
                    (x <= 2 && y <= 2) ||
                    (x >= wallGridWidth - 3 && y >= wallGridHeight - 3);

                bool placeWall;

                if (isBorder)
                {
                    placeWall = true; // 邊界一定生成
                }
                else if (isSpawnSafe)
                {
                    placeWall = false;
                }
                else
                {
                    placeWall = randomInnerWalls
                        ? Random.value < randomObstacleChance
                        : (x % wallSpacing == 0 && y % wallSpacing == 0); // ⭐ 關鍵
                }

                if (!placeWall) continue;
                // ========================
                // Stack Obstacle Layers
                // ========================
                Renderer wallRenderer = wallPrefab.GetComponentInChildren<Renderer>();
                float wallUnitHeight = wallRenderer != null ? wallRenderer.bounds.size.y : 1f;

                Vector3 basePos = new Vector3(
                    x * wallCellSize - halfWidth,
                    wallUnitHeight * 0.5f,
                    y * wallCellSize - halfHeight
                );

                for (int layer = 1; layer <= obstacleLayers; layer++)
                {
                    GameObject wall = (GameObject)PrefabUtility.InstantiatePrefab(wallPrefab);
                    wall.transform.SetParent(transform);
                    wall.transform.localPosition =
                        basePos + Vector3.up * wallUnitHeight * (layer - 1);

                    wall.name = $"Wall_{x}_{y}_L{layer}";
                    Undo.RegisterCreatedObjectUndo(wall, "Create Wall");
                }
            }
        }
#endif
    }

    [ContextMenu("Clear Grid")]
    public void ClearGrid()
    {
#if UNITY_EDITOR
        while (transform.childCount > 0)
        {
            Undo.DestroyObjectImmediate(transform.GetChild(0).gameObject);
        }
#endif
    }
}
