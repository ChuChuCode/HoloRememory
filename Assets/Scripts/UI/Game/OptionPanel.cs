using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HR.UI{
public class OptionPanel : MonoBehaviour
{
    public static OptionPanel Instance;
    // Start is called before the first frame update
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
}

}