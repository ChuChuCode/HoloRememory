using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HR.Object.Map;

public class MiddleTowerBehaviour : TowerBase
{
    public override void Death()
    {
        GetComponent<Collider>().enabled = false;
        if (Next_Tower_Collider != null)
        {
            Next_Tower_Collider.enabled = true;
        }
    }
}
