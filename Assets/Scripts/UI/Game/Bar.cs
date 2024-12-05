using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace HR.UI{
public class Bar : MonoBehaviour
{
    [SerializeField] Slider slider;
    public Gradient gradient;
    public Image fill;
    public TMP_Text text;

    public void SetValue(int value)
    {
        slider.value = value;
        fill.color = gradient.Evaluate(slider.normalizedValue);
        // if text has object then set
        if (text != null) SetText();
    }
    public void SetMaxValue(int value)
    {
        float ratio = slider.normalizedValue;
        slider.maxValue = value;
        slider.value = (int)ratio * value;

        fill.color = gradient.Evaluate(ratio);
        // if text has object then set
        if (text != null) SetText();
    }
    public void SetText()
    {
        text.text = slider.value.ToString() + "/" + slider.maxValue.ToString();
    }
}
}
