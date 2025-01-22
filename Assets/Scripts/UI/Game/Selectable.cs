using UnityEngine;
using UnityEngine.EventSystems;
using HR.Object;
using HR.Global;

namespace HR.UI{
public class Selectable : MonoBehaviour
{
    public static Selectable instance;
    public Transform highlight;
    public Transform select;
    RaycastHit raycastHit;
    [SerializeField] GameObject SelectInfo;
    [SerializeField] Bar HP;
    [SerializeField] Bar MP;
    [SerializeField] LayerMask layer;
    public int playerLayerID;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray,out raycastHit,Mathf.Infinity,layer))
        {
            // Get Component
            Transform temp = raycastHit.transform.root;
            if (temp.CompareTag("Selectable") && temp != select)
            {
                // still highlight but differnet object
                if (highlight != null && temp != highlight)
                {
                    highlight.gameObject.GetComponent<Outline>().enabled = false;
                }
                // New one highlight
                highlight = temp;
                highlight.gameObject.GetComponent<Outline>().enabled = true;
                highlight.gameObject.GetComponent<Outline>().OutlineWidth = 1.5f;
                // Check Outline Color
                Check_Outline_Color(highlight);
            }
            else
            {
                if (highlight != null)
                {
                    highlight.gameObject.GetComponent<Outline>().enabled = false;
                    highlight = null;
                }
            }
        }
        else
        {
            if (highlight != null)
            {
                highlight.gameObject.GetComponent<Outline>().enabled = false;
                highlight = null;
            }
        }
        // Left Click
        if ( InputComponent.instance.playerInput.Player.Left_Mouse.WasPressedThisFrame()
            && !EventSystem.current.IsPointerOverGameObject())
        {
            // if is hgihlight and click
            if (highlight != null)
            {
                // If already select
                if (select != null)
                {
                    select.gameObject.GetComponent<Outline>().enabled = false;
                }
                // Select UI show
                SelectInfo.SetActive(true);
                // Set Outline
                select = highlight;
                select.gameObject.GetComponent<Outline>().enabled = true;
                select.gameObject.GetComponent<Outline>().OutlineWidth = 1f;
                // Check Outline Color
                Check_Outline_Color(select);
                highlight = null;
                // Set HP/SP
                HP.SetMaxValue(select.GetComponent<Health>().maxHealth);
                HP.SetValue(select.GetComponent<Health>().currentHealth);
            }
            else
            {
                // If already select 
                if (select != null)
                {
                    // Select UI hide
                    SelectInfo.SetActive(false);
                    // Set Outline
                    select.gameObject.GetComponent<Outline>().enabled = false;
                    select = null;
                }
            }
        }
    }
    public void updateInfo(Health health)
    {
        // If UI is open and select is same as the update object -> then update
        if ( select == null ) return;
        if ( health != select.GetComponent<Health>() ) return;
        HP.SetMaxValue(select.GetComponent<Health>().maxHealth);
        HP.SetValue(select.GetComponent<Health>().currentHealth);
    }
    void Check_Outline_Color(Transform ob)
    {
        string check_item_LayerName = LayerMask.LayerToName(ob.gameObject.layer);
        string gameObject_LayerName = LayerMask.LayerToName(playerLayerID);
        // Same Team (Team1 or Team1Building)
        if (check_item_LayerName.IndexOf(gameObject_LayerName) > -1)
        {
            ob.gameObject.GetComponent<Outline>().OutlineColor = Color.green;
        }
        else
        {
            ob.gameObject.GetComponent<Outline>().OutlineColor = Color.red;
        }
    }
}

}