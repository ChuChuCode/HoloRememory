using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HR.UI{
public class BillBoard : MonoBehaviour
{
    void LateUpdate() 
    {
        transform.LookAt(transform.position + Camera.main.transform.forward);   
    }
}

}