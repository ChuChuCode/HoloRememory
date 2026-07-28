using UnityEditor;
using UnityEngine;
using Mirror;
using HR.Map;

namespace HR.GUI{
public static class PlaceholderBlockCreator
{
    const string FolderPath = "Assets/Prefabs/Map/Placeholder";

    [MenuItem("Tools/HoloBomber/Create Placeholder Blocks")]
    static void CreatePlaceholderBlocks()
    {
        EnsureFolder();

        CreateFloor();
        CreateWall();
        CreateDestructible();
        CreateItem<BombCountItem>("BombCountItem_Placeholder", Color.red);
        CreateItem<BombPowerItem>("BombPowerItem_Placeholder", new Color(1f, 0.5f, 0f));
        CreateItem<SpeedItem>("SpeedItem_Placeholder", Color.cyan);

        AssetDatabase.SaveAssets();
        Debug.Log($"Placeholder prefabs created under {FolderPath}");
    }

    static void EnsureFolder()
    {
        if (!AssetDatabase.IsValidFolder(FolderPath))
        {
            AssetDatabase.CreateFolder("Assets/Prefabs/Map", "Placeholder");
        }
    }

    static void CreateFloor()
    {
        if (AlreadyExists("Floor_Placeholder")) return;

        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = "Floor_Placeholder";
        cube.transform.localScale = new Vector3(1f, 0.1f, 1f);
        SetColor(cube, new Color(0.85f, 0.85f, 0.85f));

        int landLayer = LayerMask.NameToLayer("Land");
        if (landLayer >= 0) cube.layer = landLayer;

        // Purely visual/walkable ground - no GridCell, floor cells are
        // whatever GridManager has no Wall/Destructible entry for.
        SaveAndDestroy(cube, "Floor_Placeholder");
    }

    static void CreateWall()
    {
        if (AlreadyExists("Wall_Placeholder")) return;

        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = "Wall_Placeholder";
        SetColor(cube, Color.gray);

        cube.AddComponent<GridCell>().type = CellType.Wall;

        SaveAndDestroy(cube, "Wall_Placeholder");
    }

    static void CreateDestructible()
    {
        if (AlreadyExists("Destructible_Placeholder")) return;

        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = "Destructible_Placeholder";
        SetColor(cube, new Color(0.6f, 0.4f, 0.2f));

        cube.AddComponent<GridCell>().type = CellType.Destructible;
        cube.AddComponent<NetworkIdentity>();
        cube.AddComponent<DestructibleBlock>();

        SaveAndDestroy(cube, "Destructible_Placeholder");
    }

    static void CreateItem<T>(string name, Color color) where T : Component
    {
        if (AlreadyExists(name)) return;

        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.name = name;
        sphere.transform.localScale = Vector3.one * 0.5f;
        SetColor(sphere, color);

        Collider col = sphere.GetComponent<Collider>();
        col.isTrigger = true;

        // Unity only fires trigger events between a pair of colliders if at
        // least one side has a Rigidbody - ExplosionSegment doesn't have
        // one, and neither did this sphere, so OnTriggerEnter never fired
        // at all between an explosion and an item. Kinematic so it doesn't
        // fall over or get physically shoved around.
        Rigidbody rb = sphere.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        sphere.AddComponent<NetworkIdentity>();
        sphere.AddComponent<T>();

        SaveAndDestroy(sphere, name);
    }

    static void SetColor(GameObject obj, Color color)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        renderer.sharedMaterial = new Material(DefaultShader()) { color = color };
    }

    // "Standard" only exists under the Built-in Render Pipeline; this project
    // uses URP, so that lookup returned null and every placeholder cube got
    // Unity's magenta/black "missing shader" fallback material instead.
    static Shader DefaultShader()
    {
        return Shader.Find("Universal Render Pipeline/Lit")
            ?? Shader.Find("Standard")
            ?? Shader.Find("HDRP/Lit");
    }

    static void SaveAndDestroy(GameObject obj, string name)
    {
        string path = $"{FolderPath}/{name}.prefab";
        PrefabUtility.SaveAsPrefabAsset(obj, path);
        GameObject.DestroyImmediate(obj);
    }

    // Re-running the menu (e.g. to add a newly-supported item type) would
    // otherwise blow away materials/tweaks you already made on the existing
    // prefabs, since SaveAsPrefabAsset overwrites whatever's at that path.
    static bool AlreadyExists(string name)
    {
        string path = $"{FolderPath}/{name}.prefab";
        return AssetDatabase.LoadAssetAtPath<GameObject>(path) != null;
    }
}
}
