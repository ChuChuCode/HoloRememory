using UnityEngine;
using HR.Global;

namespace HR.Object.Player{
public class Ability_Indicator : MonoBehaviour
{
    Vector3 mouseProject;
    [SerializeField] LayerMask layer;
    // Update is called once per frame
    void Update()
    {
        // check mouse raycast
        Vector3 mousePos = InputComponent.instance.playerInput.Player.MousePosition.ReadValue<Vector2>();
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        if (Physics.Raycast(ray.origin,ray.direction, out hit,Mathf.Infinity,layer))
        {
            mouseProject = hit.point;
        }
        // mouseProject.y = transform.position.y;
        transform.LookAt(mouseProject);
    }
}

}