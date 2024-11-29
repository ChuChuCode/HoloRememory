using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HR.Object.Player{
public class Ability_Indicator : MonoBehaviour
{
    Vector3 mouseProject;
    // Update is called once per frame
    void Update()
    {
        // check mouse raycast
        Vector3 mousePos = InputSystem.instance.playerInput.Player.MousePosition.ReadValue<Vector2>();
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        if (Physics.Raycast(ray.origin,ray.direction, out hit,Mathf.Infinity,~LayerMask.NameToLayer("Land")))
        {
            mouseProject = hit.point;
        }
        // mouseProject.y = transform.position.y;
        transform.LookAt(mouseProject);
    }
}

}