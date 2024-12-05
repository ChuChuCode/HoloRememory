using System.Collections;
using System.Collections.Generic;
using HR.Global;
using TMPro;
using UnityEngine;

namespace HR.UI{
[RequireComponent(typeof(TMP_Text))]
public class SliderText : MonoBehaviour
{
    [SerializeField] Audio_Type type;
    [SerializeField] TMP_Text ValueText;
    public void OnSliderValueChanged(float value)
    {
        ValueText.text = (value*100).ToString();
        Setting_Component.Instance.SetMixer(type,value);
    }
}

}