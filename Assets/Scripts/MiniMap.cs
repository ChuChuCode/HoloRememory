using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MiniMap : MonoBehaviour, IPointerDownHandler, IPointerUpHandler,IPointerMoveHandler
{
    //Camera that renders to the texture
    public Camera gridCamera; 
    //RawImage RectTransform that shows the RenderTexture on the UI
    [SerializeField] RectTransform textureRectTransform; 
    Vector3 position;
    bool Pressed_Left = false;
    bool Pressed_Right = false;
    void Awake()
    {
        textureRectTransform = GetComponent<RectTransform>(); //Get the RectTransform
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        //I get the point of the RawImage where I click
        // eventData.position -> Get Click on Screen
        RectTransformUtility.ScreenPointToLocalPointInRectangle(textureRectTransform, eventData.position, null, out Vector2 localClick);

        //My RawImage is 700x700 and the click coordinates are in range (-350,350) so I transform it to (0,700) to then normalize
        localClick.x = textureRectTransform.rect.width - (localClick.x * -1);

        //I normalize the click coordinates so I get the viewport point to cast a Ray
        Vector2 viewportClick = new Vector2(localClick.x / textureRectTransform.rect.width, localClick.y / textureRectTransform.rect.height);

        //I have a special layer for the objects I want to detect with my ray
        LayerMask layer = LayerMask.GetMask("Land");

        //I cast the ray from the camera which rends the texture
        Ray ray = gridCamera.ViewportPointToRay(new Vector3(viewportClick.x, viewportClick.y, 0));

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layer))
        {
            position = hit.point;
            // Check Left Button or Right Button
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                // Free Camera Move
                Pressed_Left = true;
                GameController.Instance.LocalPlayer.Set_FreeCamera(position);
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                // Player Move
                Pressed_Right = true;
                GameController.Instance.LocalPlayer.Set_Destination(position,true);
            }
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            // Free Camera Move
            Pressed_Left = false;
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            // Player Move
            Pressed_Right = false;
        }
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        //I get the point of the RawImage where I click
        // eventData.position -> Get Click on Screen
        RectTransformUtility.ScreenPointToLocalPointInRectangle(textureRectTransform, eventData.position, null, out Vector2 localClick);

        // Set x value
        localClick.x = textureRectTransform.rect.width - (localClick.x * -1);

        //I normalize the click coordinates so I get the viewport point to cast a Ray
        Vector2 viewportClick = new Vector2(localClick.x / textureRectTransform.rect.width, localClick.y / textureRectTransform.rect.height);

        //I have a special layer for the objects I want to detect with my ray
        LayerMask layer = LayerMask.GetMask("Land");

        //I cast the ray from the camera which rends the texture
        Ray ray = gridCamera.ViewportPointToRay(new Vector3(viewportClick.x, viewportClick.y, 0));

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layer))
        {
            position = hit.point;
            // Check Left Button or Right Button
            if (Pressed_Left)
            {
                // Free Camera Move
                GameController.Instance.LocalPlayer.Set_FreeCamera(position);
            }
            else if (Pressed_Right)
            {
                // Player Move
                GameController.Instance.LocalPlayer.Set_Destination(position,false);
            }
        }
    }
}
