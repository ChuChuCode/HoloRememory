using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace HR.UI{
public class Bar : MonoBehaviour
{
    public enum BarType
    {
        Health,
        Mana,
        Energy
    }
    public BarType barType;
    [SerializeField] Slider slider;
    public Gradient gradient;
    public Image fill;
    public TMP_Text text;

    public void SetValue(int value)
    {
        slider.value = value;
        // Set fill color
        switch(barType)
        {
            case BarType.Health:
                fill.color = gradient.Evaluate(slider.normalizedValue);
                break;
            case BarType.Mana:
                fill.color = Color.blue;
                break;
            case BarType.Energy:
                fill.color = Color.yellow;
                break;
        }
        // if text has object then set
        if (text != null) SetText();
    }
    public void SetMaxValue(int value)
    {
        float ratio = slider.normalizedValue;
        slider.maxValue = value;
        slider.value = (int)ratio * value;
        // Set fill color
        switch(barType)
        {
            case BarType.Health:
                fill.color = gradient.Evaluate(ratio);
                break;
            case BarType.Mana:
                fill.color = Color.blue;
                break;
            case BarType.Energy:
                fill.color = Color.yellow;
                break;
        }
        // if text has object then set
        if (text != null) SetText();
    }
    public void SetText()
    {
        text.text = slider.value.ToString() + "/" + slider.maxValue.ToString();
    }
}
}
