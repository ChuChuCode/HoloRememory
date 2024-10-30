using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class Selectable : MonoBehaviour
{
    public Transform highlight;
    public Transform select;
    RaycastHit raycastHit;
    [SerializeField] GameObject SelectInfo;
    [SerializeField] Bar HP;
    [SerializeField] Bar MP;

    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray,out raycastHit))
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
        if (InputSystem.instance.playerInput.Player.Left_Mouse.WasPressedThisFrame())
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
                highlight = null;
                // Set HP/SP
                HP.SetMaxValue(select.GetComponent<IHealth>().maxHealth);
                HP.SetValue(select.GetComponent<IHealth>().currentHealth);
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
}
