using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HR.UI{
public class ChangeMiniMapLayer : MonoBehaviour
{
    bool isChange = false;
    void Update()
    {
        if (gameObject.layer != LayerMask.NameToLayer("MiniMap") && !isChange)
        {
            gameObject.layer = LayerMask.NameToLayer("MiniMap");
        }
    }
}

}