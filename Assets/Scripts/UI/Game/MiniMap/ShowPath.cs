using UnityEngine;
using HR.Object.Player;

namespace HR.UI{
public class ShowPath : MonoBehaviour
{
    public static ShowPath Instance;
    public CharacterBase LocalPlayer;
    [SerializeField] LineRenderer lineRenderer;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
    void Start()
    {
        
        lineRenderer.startWidth = 1f;
        lineRenderer.endWidth = 1f;
        lineRenderer.positionCount = 0;
    }
    void Update()
    {
        if (LocalPlayer == null) return;
        // if (!LocalPlayer.agent.hasPath) return;
        // lineRenderer.positionCount = LocalPlayer.agent.path.corners.Length;
        lineRenderer.SetPosition(0,LocalPlayer.transform.position);

        if (lineRenderer.positionCount < 2) return;

        for (int i = 1 ; i < lineRenderer.positionCount ; i++)
        {
            // Vector3 pointPosition = LocalPlayer.agent.path.corners[i];
            // lineRenderer.SetPosition(i,pointPosition);
        }
    }
}

}