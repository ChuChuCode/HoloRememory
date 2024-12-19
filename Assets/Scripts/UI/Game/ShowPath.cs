using UnityEngine;
using HR.Network.Game;
using Unity.VisualScripting;
using HR.Object.Player;

namespace HR.UI{
public class ShowPath : MonoBehaviour
{
    [SerializeField] LineRenderer lineRenderer;
    void Start()
    {
        lineRenderer.startWidth = 1f;
        lineRenderer.endWidth = 1f;
        lineRenderer.positionCount = 0;
    }
    void Update()
    {
        if (GameController.Instance.LocalPlayer == null) return;
        if (!GameController.Instance.LocalPlayer.agent.hasPath) return;
        lineRenderer.positionCount = GameController.Instance.LocalPlayer.agent.path.corners.Length;
        lineRenderer.SetPosition(0,GameController.Instance.LocalPlayer.transform.position);

        if (lineRenderer.positionCount < 2) return;

        for (int i = 1 ; i < lineRenderer.positionCount ; i++)
        {
            Vector3 pointPosition = GameController.Instance.LocalPlayer.agent.path.corners[i];
            lineRenderer.SetPosition(i,pointPosition);
        }
    }
}

}