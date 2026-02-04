using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class GridSpawnerEditor : MonoBehaviour
{
    [Header("Grid Size")]
    public int width = 13;
    public int height = 11;
    public float cellSize = 1f;

    [Header("Prefabs")]
    public GameObject[] prefabs;

    [Header("Options")]
    public bool randomPrefab = false;

    [ContextMenu("Generate Grid")]
    public void GenerateGrid()
    {
#if UNITY_EDITOR
        ClearGrid();

        if (prefabs == null || prefabs.Length == 0)
        {
            Debug.LogError("No prefabs assigned");
            return;
        }

        Undo.RegisterFullObjectHierarchyUndo(gameObject, "Generate Grid");

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 pos = new Vector3(
                    x * cellSize - (width - 1) * cellSize / 2f,
                    0,
                    y * cellSize - (height - 1) * cellSize / 2f
                );

                GameObject prefab = randomPrefab
                    ? prefabs[Random.Range(0, prefabs.Length)]
                    : prefabs[(x + y) % prefabs.Length];

                GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                obj.transform.SetParent(transform);
                obj.transform.localPosition = pos;
                obj.name = $"Cell_{x}_{y}";

                Undo.RegisterCreatedObjectUndo(obj, "Create Cell");
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