using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeCamera : MonoBehaviour
{
    [SerializeField] GameObject parent;
    [SerializeField] float camSpeed = 20;
    [SerializeField] float screenSizeThickness = 10;
    float angle = -30 ;

    void Start()
    {
        // Remove Parent relationships
        transform.parent = null;
        gameObject.SetActive(false);
    }
    void Update()
    {
        Vector3 mousePos = InputSystem.instance.playerInput.Player.MousePosition.ReadValue<Vector2>();
        Vector3 position = transform.position;
        // Up
        if (mousePos.y >= Screen.height - screenSizeThickness)
        {
            position.x += camSpeed * Time.deltaTime * Mathf.Cos(angle * Mathf.Deg2Rad);
            position.z -= camSpeed * Time.deltaTime * Mathf.Sin(angle * Mathf.Deg2Rad);
        }
        // Down
        if (mousePos.y <= screenSizeThickness)
        {
            position.x -= camSpeed * Time.deltaTime * Mathf.Cos(angle * Mathf.Deg2Rad);
            position.z += camSpeed * Time.deltaTime * Mathf.Sin(angle * Mathf.Deg2Rad);
        }
        // Right
        if (mousePos.x >= Screen.width - screenSizeThickness)
        {
            position.x -= camSpeed * Time.deltaTime * Mathf.Sin(angle * Mathf.Deg2Rad);
            position.z -= camSpeed * Time.deltaTime * Mathf.Cos(angle * Mathf.Deg2Rad);
        }
        // Left
        if (mousePos.x <= screenSizeThickness)
        {
            position.x += camSpeed * Time.deltaTime * Mathf.Sin(angle * Mathf.Deg2Rad);
            position.z += camSpeed * Time.deltaTime * Mathf.Cos(angle * Mathf.Deg2Rad);
        }
        transform.position = position;
    }
}
