using HR.Global;
using UnityEngine;

namespace HR.UI{
public class OptionPanel : MonoBehaviour
{
    public static OptionPanel Instance;
    [SerializeField] Setting_Component setting_Component;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
    void Start()
    {
        gameObject.SetActive(false);
    }
}

}