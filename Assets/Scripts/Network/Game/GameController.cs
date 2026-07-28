using UnityEngine;

using HR.Object.Player;
using HR.Object;

namespace HR.Network.Game{
public class GameController : MonoBehaviour
{
    public static GameController Instance;
    [Header("Manager")]
    private Network_Manager manager;

    public Network_Manager Manager
    {
        get
        {
            if (manager != null)
            {
                return manager;
            }
            return manager = Network_Manager.singleton as Network_Manager;
        }
    }
    public CharacterBase LocalPlayer;
    [HideInInspector] public PlayerObject LocalPlayerController;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
    void Start()
    {
        CreateGrid();
        foreach (PlayerObject player in  Manager.PlayersInfoList)
        {
            if (player.isOwned)
            {
                LocalPlayerController = player;
            }
        }
    }
    [Header("網格尺寸 (跟 GridSpawnerEditor 共用同一份設定)")]
    [SerializeField] GridSpawnerEditor gridSpawner;

    [Header("線條外觀")]
    public float lineWidth = 0.05f;
    public Material lineMaterial;
    public Color gridColor = Color.white;
    void CreateGrid()
    {
        if (gridSpawner == null) return;

        foreach ((string name, Vector3 start, Vector3 end) in GridLines())
        {
            CreateLine(name, start, end);
        }
    }

    // Same width/height/cellSize and centering formula as GridSpawnerEditor's
    // GenerateGrid(), so the debug lines land exactly on the real tile edges.
    System.Collections.Generic.IEnumerable<(string, Vector3, Vector3)> GridLines()
    {
        int width = gridSpawner.width;
        int height = gridSpawner.height;
        float cellSize = gridSpawner.cellSize;

        float offsetX = (width / 2) * cellSize;
        float offsetZ = (height / 2) * cellSize;
        float xMin = -offsetX - cellSize / 2f;
        float zMin = -offsetZ - cellSize / 2f;
        float xMax = xMin + width * cellSize;
        float zMax = zMin + height * cellSize;

        // 橫線 (平行於 X 軸，數量由 height 決定)
        for (int i = 0; i <= height; i++)
        {
            float zPos = zMin + i * cellSize;
            yield return ($"Row_{i}", new Vector3(xMin, 0, zPos), new Vector3(xMax, 0, zPos));
        }

        // 直線 (平行於 Z 軸，數量由 width 決定)
        for (int i = 0; i <= width; i++)
        {
            float xPos = xMin + i * cellSize;
            yield return ($"Col_{i}", new Vector3(xPos, 0, zMin), new Vector3(xPos, 0, zMax));
        }
    }

    void CreateLine(string name, Vector3 start, Vector3 end)
    {
        GameObject lineObj = new GameObject(name);
        lineObj.transform.SetParent(transform);
        lineObj.transform.localPosition = Vector3.zero;

        LineRenderer lr = lineObj.AddComponent<LineRenderer>();
        lr.material = lineMaterial != null ? lineMaterial : new Material(Shader.Find("Sprites/Default"));
        
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        lr.startColor = gridColor;
        lr.endColor = gridColor;

        lr.positionCount = 2;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);

        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.receiveShadows = false;
        lr.useWorldSpace = false;
    }
    [Header("編輯器顯示 (Gizmos)")]
    public bool showInEditor = true;
    public Color gizmoColor = Color.yellow;
    private void OnDrawGizmos()
    {
        if (!showInEditor || gridSpawner == null) return;

        Gizmos.color = gizmoColor;
        // 將 Gizmos 矩陣設為物件的 Transform，支援旋轉與位移
        Gizmos.matrix = transform.localToWorldMatrix;

        foreach ((string _, Vector3 start, Vector3 end) in GridLines())
        {
            Gizmos.DrawLine(start, end);
        }
    }
}

}