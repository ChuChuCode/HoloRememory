using HR.Global;
using UnityEngine.UI;
using UnityEngine;

namespace HR.UI{
[RequireComponent(typeof(Slider))]
public class SliderChange : MonoBehaviour
{
    [SerializeField] Audio_Type type;
    public void OnSliderValueChanged(float value)
    {
        Setting_Component.Instance.SetMixer(type,value);
    }
}

}