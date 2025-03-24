using UnityEngine;
using UnityEngine.EventSystems;
using HR.Object;
using HR.Global;
using HR.Object.Player;

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
    public CharacterBase LocalPlayer;

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
            // If is Selectable
            if (temp.CompareTag("Selectable") )
            {
                // change only when hover one is not select
                if (temp != select)
                {
                    // still highlight but differnet object
                    if (highlight != null && highlight != select)
                    {
                        // hide old highlight
                        highlight.gameObject.GetComponent<Outline>().enabled = false;
                    }
                    // replace new highlight
                    highlight = temp;
                    // new highlight show
                    highlight.gameObject.GetComponent<Outline>().OutlineWidth = 1f;
                    highlight.gameObject.GetComponent<Outline>().enabled = true;
                    highlight.gameObject.GetComponent<Outline>().UpdateMaterialProperties();
                    // Check Outline Color
                    Check_Outline_Color(highlight);
                }
            }
            else
            {
                // hide highlight
                if (highlight != null && highlight != select)
                {
                    highlight.gameObject.GetComponent<Outline>().enabled = false;
                }
                highlight = null;
            }
        }
        else
        {
            // hide highlight
            if (highlight != null && highlight != select)
            {
                highlight.gameObject.GetComponent<Outline>().enabled = false;
            }
            highlight = null;
        }
        // Left Click
        if ( InputComponent.instance.playerInput.Player.Left_Mouse.WasPressedThisFrame()
            && !EventSystem.current.IsPointerOverGameObject())
        {
            // if is highlight and click
            if (highlight != null)
            {
                // hide old select
                if (select != null)
                {
                    select.gameObject.GetComponent<Outline>().enabled = false;
                }
                // Select UI show
                SelectInfo.SetActive(true);
                // Replace new select
                select = highlight;
                select.gameObject.GetComponent<Outline>().enabled = true;
                select.gameObject.GetComponent<Outline>().UpdateMaterialProperties();
                // Check Outline Color
                Check_Outline_Color(select);
                select.gameObject.GetComponent<Outline>().OutlineWidth = 2f;

                // Set HP/SP
                HP.SetMaxValue(select.GetComponent<Health>().maxHealth);
                HP.SetValue(select.GetComponent<Health>().currentHealth);
            }
            // if not highlight and click
            else
            {
                // If already select 
                if (select != null)
                {
                    // Select UI hide
                    SelectInfo.SetActive(false);
                    // hide Outline
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
        if ( health.currentHealth <= 0 ) 
        {
            select.gameObject.GetComponent<Outline>().enabled = false;
            SelectInfo.SetActive(false);
            return;
        }
        HP.SetMaxValue(select.GetComponent<Health>().maxHealth);
        HP.SetValue(select.GetComponent<Health>().currentHealth);
    }
    void Check_Outline_Color(Transform ob)
    {
        string check_item_LayerName = LayerMask.LayerToName(ob.gameObject.layer);
        string gameObject_LayerName = LayerMask.LayerToName(LocalPlayer.gameObject.layer);
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