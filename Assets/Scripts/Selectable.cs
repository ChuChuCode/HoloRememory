using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Selectable : MonoBehaviour
{
    public Transform highlight;
    RaycastHit raycastHit;

    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray,out raycastHit))
        {
            Transform temp = raycastHit.transform.root;
            print(temp);
            if (temp.CompareTag("Selectable"))
            {
                if (highlight != null && temp != highlight)
                {
                    // still highlight but differnet object
                    highlight.gameObject.GetComponent<Outline>().enabled = false;
                }
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
    }
}
