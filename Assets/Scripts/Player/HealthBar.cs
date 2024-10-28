using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] Slider slider;
    public Gradient gradient;
    public Image fill;

    public void SetHealth(int health)
    {
        slider.value = health;
        float ratio = slider.normalizedValue;
        fill.color = gradient.Evaluate(ratio);
    }
    public void SetMaxHealth(int health)
    {
        float ratio = slider.normalizedValue;
        slider.maxValue = health;
        slider.value = (int)ratio * health;

        fill.color = gradient.Evaluate(ratio);
    }
}
