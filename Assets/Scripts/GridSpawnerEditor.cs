using UnityEngine;
using HR.Map;
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
    public GameObject destructiblePrefab;

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

        // Integer division (not (width-1)/2f) so this is always a whole
        // multiple of cellSize - otherwise an even width/height leaves every
        // tile centered half a cell off from GridManager's coord*cellSize,
        // which is exactly what made bombs land on a tile's corner.
        float offsetX = (width / 2) * cellSize;
        float offsetZ = (height / 2) * cellSize;

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
        // Obstacle Grid - same resolution as the floor grid, so GridManager
        // (cellSize = 1 floor cell) lines up 1:1 with bomb placement/explosion.
        // ========================
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                bool isBorder =
                    x == 0 || y == 0 ||
                    x == width - 1 || y == height - 1;

                // Cleared based on the real Spawn markers (GridCell type=Spawn)
                // placed in the scene, not a guessed corner - so the player's
                // own cell and its 4 cardinal neighbors are always open.
                bool isSpawnSafe = IsNearSpawn(x - width / 2, y - height / 2);

                bool isPillar = x % wallSpacing == 0 && y % wallSpacing == 0; // ⭐ 固定不可破壞柱子

                GameObject obstaclePrefab;

                if (isBorder)
                {
                    obstaclePrefab = wallPrefab; // 邊界一定生成，不可破壞
                }
                else if (isSpawnSafe)
                {
                    obstaclePrefab = null; // 出生點附近淨空
                }
                else if (isPillar)
                {
                    obstaclePrefab = wallPrefab; // 固定柱子，不可破壞
                }
                else if (randomInnerWalls && destructiblePrefab != null)
                {
                    // 隨機生成可破壞磚（不是牆）
                    obstaclePrefab = Random.value < randomObstacleChance ? destructiblePrefab : null;
                }
                else
                {
                    obstaclePrefab = null;
                }

                if (obstaclePrefab == null) continue;
                // ========================
                // One object per cell, stretched to the stacked height.
                // (Previously this instantiated `obstacleLayers` separate
                // copies at the same x/z - GridManager only tracks x/z, so
                // the copies fought over the same dictionary slot and only
                // one of them ever actually got destroyed by an explosion,
                // leaving an invisible collider from the other behind.)
                // ========================
                Renderer obstacleRenderer = obstaclePrefab.GetComponentInChildren<Renderer>();
                float wallUnitHeight = obstacleRenderer != null ? obstacleRenderer.bounds.size.y : 1f;
                float totalHeight = wallUnitHeight * obstacleLayers;

                GameObject obstacle = (GameObject)PrefabUtility.InstantiatePrefab(obstaclePrefab);
                obstacle.transform.SetParent(transform);
                obstacle.transform.localPosition = new Vector3(
                    x * cellSize - offsetX,
                    totalHeight * 0.5f,
                    y * cellSize - offsetZ
                );
                obstacle.transform.localScale = new Vector3(
                    obstacle.transform.localScale.x,
                    obstacle.transform.localScale.y * obstacleLayers,
                    obstacle.transform.localScale.z);

                string prefix = obstaclePrefab == wallPrefab ? "Wall" : "Destructible";
                obstacle.name = $"{prefix}_{x}_{y}";
                Undo.RegisterCreatedObjectUndo(obstacle, "Create Obstacle");
            }
        }
#endif
    }

    bool IsNearSpawn(int gridX, int gridY)
    {
        if (GridManager.Instance == null) return false;

        foreach (Vector2Int spawn in GridManager.Instance.GetAllSpawnCoords())
        {
            // Manhattan distance <= 1 covers the spawn cell itself plus its
            // 4 cardinal neighbors (front/back/left/right), not diagonals.
            if (Mathf.Abs(spawn.x - gridX) + Mathf.Abs(spawn.y - gridY) <= 1)
            {
                return true;
            }
        }
        return false;
    }

    [ContextMenu("Clear Grid")]
    public void ClearGrid()
    {
#if UNITY_EDITOR
        // Only remove what GenerateGrid() itself created - a blanket "destroy
        // every child" here would also wipe out manually-placed Spawn markers
        // that happen to sit under the same parent, right before the obstacle
        // loop queries GridManager for them (making IsNearSpawn always miss).
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            if (child.name.StartsWith("Floor_") || child.name.StartsWith("Wall_") || child.name.StartsWith("Destructible_"))
            {
                Undo.DestroyObjectImmediate(child.gameObject);
            }
        }
#endif
    }
}
