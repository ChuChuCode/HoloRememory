using UnityEngine;

using HR.Object.Player;
using HR.Object;

namespace HR.Network.Game{
public class GameController : MonoBehaviour
{
    public static GameController Instance;
    [Header("Spawn Point")]
    public Transform Team1_transform;
    public Transform Team2_transform;
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
    public PlayerObject LocalPlayerController;
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
    [Header("網格尺寸")]
    public int gridSizeX = 20; // 橫向格數
    public int gridSizeY = 10; // 縱向格數
    public float unitLength = 1f;

    [Header("線條外觀")]
    public float lineWidth = 0.05f;
    public Material lineMaterial;
    public Color gridColor = Color.white;
    void CreateGrid()
    {
        // 計算總長度與總寬度
        float width = gridSizeX * unitLength;
        float height = gridSizeY * unitLength;
        
        // 計算中心偏移，使 (0,0) 位於網格中心
        float offsetX = width / 2f;
        float offsetY = height / 2f;

        // 繪製橫線 (平行於 X 軸，數量由 Y 決定)
        for (int i = 0; i <= gridSizeY; i++)
        {
            float yPos = (i * unitLength) - offsetY;
            CreateLine($"Row_{i}", new Vector3(-offsetX, 0, yPos), new Vector3(offsetX, 0, yPos));
        }

        // 繪製直線 (平行於 Z 軸，數量由 X 決定)
        for (int i = 0; i <= gridSizeX; i++)
        {
            float xPos = (i * unitLength) - offsetX;
            CreateLine($"Col_{i}", new Vector3(xPos, 0, -offsetY), new Vector3(xPos, 0, offsetY));
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
        if (!showInEditor) return;

        Gizmos.color = gizmoColor;
        // 將 Gizmos 矩陣設為物件的 Transform，支援旋轉與位移
        Gizmos.matrix = transform.localToWorldMatrix;

        float width = gridSizeX * unitLength;
        float height = gridSizeY * unitLength;
        float offsetX = width / 2f;
        float offsetY = height / 2f;

        // 繪製橫線
        for (int i = 0; i <= gridSizeY; i++)
        {
            float yPos = (i * unitLength) - offsetY;
            Vector3 start = new Vector3(-offsetX, 0, yPos);
            Vector3 end = new Vector3(offsetX, 0, yPos);
            Gizmos.DrawLine(start, end);
        }

        // 繪製直線
        for (int i = 0; i <= gridSizeX; i++)
        {
            float xPos = (i * unitLength) - offsetX;
            Vector3 start = new Vector3(xPos, 0, -offsetY);
            Vector3 end = new Vector3(xPos, 0, offsetY);
            Gizmos.DrawLine(start, end);
        }
    }
    public void End_Game(int win_team)
    {
        if (win_team == 1)
        {
            if (LocalPlayer.gameObject.layer == LayerMask.NameToLayer("Team1"))
            {
                // Show Victory
            }
            else
            {
                // Show Defeat

            }
        }
        else
        {
            if (LocalPlayer.gameObject.layer == LayerMask.NameToLayer("Team2"))
            {
                // Show Victory
            }
            else
            {
                // Show Defeat

            }
        }
        // PlayerInput Disable
        
    }
}

}