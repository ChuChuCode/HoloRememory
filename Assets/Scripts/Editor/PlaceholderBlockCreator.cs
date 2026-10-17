using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Mirror;
using HR.Map;

namespace HR.GUI{
public static class PlaceholderBlockCreator
{
    const string FolderPath = "Assets/Prefabs/Map/Placeholder";

    [MenuItem("HoloBomber/Create Placeholder Blocks")]
    static void CreatePlaceholderBlocks()
    {
        EnsureFolder();

        CreateFloor();
        CreateWall();
        CreateDestructible();
        CreateBush();
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

    static void CreateBush()
    {
        if (AlreadyExists("Bush_Placeholder")) return;

        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = "Bush_Placeholder";
        cube.transform.localScale = new Vector3(1f, 0.6f, 1f);
        // Genuinely Opaque - looks, shades, and shadows like any other
        // solid block by default (see BushBlock's own comment for why this
        // can't just be one material with alpha dialed down at runtime).
        SetColor(cube, new Color(0.2f, 0.55f, 0.2f, 1f));

        // Trigger, not solid - walkable (see GridManager.IsOccupied), just
        // detectable so BushBlock knows who's standing in it. No Rigidbody
        // needed here: the character's own Rigidbody is enough for Unity to
        // fire trigger events between the two (see CreateItem's comment).
        Collider col = cube.GetComponent<Collider>();
        col.isTrigger = true;

        cube.AddComponent<GridCell>().type = CellType.Bush;
        cube.AddComponent<NetworkIdentity>();
        BushBlock bushBlock = cube.AddComponent<BushBlock>();

        // Separate, genuinely Transparent material BushBlock swaps in only
        // while an ally's hiding inside - never saved as the renderer's own
        // material, so SetColor/SaveMaterialAsset above is bypassed here.
        Material occupiedMaterial = new Material(DefaultShader()) { color = new Color(0.2f, 0.55f, 0.2f, 0.45f) };
        SetTransparent(occupiedMaterial);
        bushBlock.OccupiedMaterial = SaveMaterialAsset(occupiedMaterial, "Bush_Placeholder_Occupied");

        SaveAndDestroy(cube, "Bush_Placeholder");
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
        Material material = new Material(DefaultShader()) { color = color };
        renderer.sharedMaterial = SaveMaterialAsset(material, obj.name);
    }

    // A Material assigned straight onto a Renderer and carried through
    // PrefabUtility.SaveAsPrefabAsset without ever being saved as its own
    // real asset doesn't get embedded cleanly - in practice this ended up
    // serializing as a reference that resolved to a COMPLETELY UNRELATED
    // asset's internal sub-object instead (this is what silently pointed
    // Wall/Destructible/Floor/Bush's placeholder material at random,
    // unrelated materials from the Kenney asset packs). Saving it as its
    // own standalone .mat first avoids that entirely, and matches how
    // every other material in this project already works.
    static Material SaveMaterialAsset(Material material, string ownerName)
    {
        string path = $"{FolderPath}/{ownerName}_Mat.mat";
        AssetDatabase.CreateAsset(material, path);
        return material;
    }

    // Script-equivalent of flipping a URP Lit material's Surface Type from
    // Opaque to Transparent in the Inspector. Values confirmed against this
    // project's own existing Transparent URP Lit materials (KroniiGhost.mat,
    // Models/face 1.mat) rather than assumed from UnityEngine.Rendering.
    // BlendMode - the Inspector-set _SrcBlend/_DstBlend ints there don't
    // match that enum's ordinals for this shader.
    static void SetTransparent(Material material)
    {
        material.SetFloat("_Surface", 1f); // 1 = Transparent
        material.SetFloat("_Blend", 0f); // 0 = Alpha blend
        material.SetOverrideTag("RenderType", "Transparent");
        material.SetInt("_SrcBlend", 5); // SrcAlpha
        material.SetInt("_DstBlend", 10); // OneMinusSrcAlpha
        material.SetInt("_ZWrite", 0);
        material.DisableKeyword("_ALPHATEST_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        // DepthOnly/SHADOWCASTER deliberately left enabled (unlike a typical
        // "actually see-through" transparent material) - this bush looks
        // fully solid (alpha 1) almost all the time, and disabling them made
        // it skip this project's SSAO renderer feature (which samples the
        // DepthOnly prepass) and stop casting a shadow, reading as "flat"/
        // not-quite-solid even at alpha 1.
        material.renderQueue = (int)RenderQueue.Transparent;
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
