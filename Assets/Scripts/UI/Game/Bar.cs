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
    // Smoothly slides the fill to its new value instead of snapping -
    // 0 keeps the old instant-jump behavior, for any Bar that wants it.
    [SerializeField] float fillAnimDuration = 0.3f;

    Coroutine fillRoutine;

    // float, not int - lets a continuously-regenerating bar (Skill Energy)
    // feed a smooth fractional fill instead of snapping in whole-unit
    // jumps. Whole-number bars (Health) just pass an int, which implicitly
    // widens to float with no behavior change.
    //
    // instant: true skips the fill animation entirely - needed for the
    // very first value pushed to a fresh bar, since animating there would
    // slide FROM whatever the Slider happened to be left at in the Editor
    // (often its max) instead of snapping straight to the real starting
    // value.
    public void SetValue(float value, bool instant = false)
    {
        // Text/color react to the real target value immediately - only the
        // fill's own visual position animates toward it.
        switch(barType)
        {
            case BarType.Health:
                fill.color = gradient.Evaluate(slider.maxValue > 0 ? value / slider.maxValue : 0f);
                break;
            case BarType.Mana:
                fill.color = Color.blue;
                break;
            case BarType.Energy:
                fill.color = Color.yellow;
                break;
        }
        if (text != null)
        {
            // Floored, not rounded - only counts a unit once the fill has
            // actually fully reached it, per spec ("滿了就會更新文字"),
            // rather than rounding up early while still mid-fill.
            text.text = Mathf.FloorToInt(value) + "/" + slider.maxValue;
        }

        if (!instant && fillAnimDuration > 0f && isActiveAndEnabled)
        {
            if (fillRoutine != null) StopCoroutine(fillRoutine);
            fillRoutine = StartCoroutine(AnimateFill(value));
        }
        else
        {
            if (fillRoutine != null)
            {
                StopCoroutine(fillRoutine);
                fillRoutine = null;
            }
            slider.value = value;
        }
    }

    IEnumerator AnimateFill(float target)
    {
        float start = slider.value;
        float elapsed = 0f;
        while (elapsed < fillAnimDuration)
        {
            elapsed += Time.deltaTime;
            slider.value = Mathf.Lerp(start, target, elapsed / fillAnimDuration);
            yield return null;
        }
        slider.value = target; // guarantee an exact landing despite frame-time drift
    }
    public void SetMaxValue(int value)
    {
        float ratio = slider.normalizedValue;
        slider.maxValue = value;
        // Was "(int)ratio * value" - casting the 0-1 ratio to int truncates
        // it to 0 every time, silently resetting the fill. Harmless back
        // when SetValue snapped instantly right after (the reset was never
        // visible), but SetValue now animates FROM slider.value, so that
        // stale 0 became a visible "restarts from empty" glitch.
        slider.value = ratio * value;
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
