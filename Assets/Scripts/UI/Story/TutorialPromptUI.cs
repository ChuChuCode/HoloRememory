using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Localization.Settings;
using HR.Global;

namespace HR.UI{
// Lightweight "press this key" hint - icon (via the existing KeyIconSet,
// same Resources.Load("KeyIconSet") pattern as KeyBindComponent) + short
// text, shown for a fixed duration (Network_Manager.FireLevelEvent's
// ShowPrompt case) then auto-hidden. Unlike DialogueUI this never locks
// input and isn't click-to-advance - it's meant to sit over live gameplay
// while the player tries the thing it's telling them to do. Lives in the
// Game_Canvas prefab, same as DialogueUI.
public class TutorialPromptUI : MonoBehaviour
{
    public static TutorialPromptUI Instance;

    [SerializeField] GameObject panel;
    [SerializeField] Image iconImage;
    [SerializeField] TMP_Text promptText;

    const string StoryTableName = "Story";

    static KeyIconSet iconSet;
    static KeyIconSet IconSet => iconSet != null ? iconSet : (iconSet = Resources.Load<KeyIconSet>("KeyIconSet"));

    Coroutine hideRoutine;

    void Awake()
    {
        if (Instance == null) Instance = this;
        if (panel != null) panel.SetActive(false);
    }

    // Called by Network_Manager.FireLevelEvent (ShowPrompt action) - fire-
    // and-forget, no completion callback (nothing waits on this the way
    // DialogueUI's Play() is awaited).
    public void Show(string controlName, string textKey, float duration)
    {
        if (hideRoutine != null)
        {
            StopCoroutine(hideRoutine);
            hideRoutine = null;
        }

        if (panel != null) panel.SetActive(true);

        Sprite icon = !string.IsNullOrEmpty(controlName) && IconSet != null ? IconSet.GetIcon(controlName) : null;
        if (iconImage != null)
        {
            iconImage.sprite = icon;
            iconImage.gameObject.SetActive(icon != null);
        }
        if (promptText != null)
        {
            // Falls back to the raw control name if no icon/translation
            // covers it, same "something rather than a blank box" spirit as
            // KeyBindComponent's own icon/text fallback.
            string text = !string.IsNullOrEmpty(textKey)
                ? LocalizationSettings.StringDatabase.GetLocalizedString(StoryTableName, textKey)
                : controlName;
            promptText.text = icon != null ? text : $"[{controlName}] {text}";
        }

        hideRoutine = StartCoroutine(HideAfterDelay(duration));
    }

    public void Hide()
    {
        if (hideRoutine != null)
        {
            StopCoroutine(hideRoutine);
            hideRoutine = null;
        }
        if (panel != null) panel.SetActive(false);
    }

    System.Collections.IEnumerator HideAfterDelay(float duration)
    {
        yield return new WaitForSeconds(duration);
        hideRoutine = null;
        if (panel != null) panel.SetActive(false);
    }
}
}
