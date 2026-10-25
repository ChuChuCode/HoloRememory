using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.Localization.Settings;
using HR.Global;

namespace HR.UI.Main{
// One row in the Key Bindings panel - label + current key + a button that
// starts an interactive rebind. Spawned by KeyBindPanel, one per
// BindableKey, onto the "Key Bind Component" prefab.
public class KeyBindComponent : MonoBehaviour
{
    const string UITableName = "UI";

    [SerializeField] TMP_Text actionLabelText;
    [SerializeField] TMP_Text currentKeyText;
    [SerializeField] Button rebindButton;
    [SerializeField] TMP_Text rebindButtonText;

    BindableKey boundKey;
    string actionLabelKey;

    public void Setup(BindableKey key, string labelKey)
    {
        boundKey = key;
        actionLabelKey = labelKey;
        if (actionLabelText != null) actionLabelText.text = LocalizationSettings.StringDatabase.GetLocalizedString(UITableName, actionLabelKey);
        ResetButtonText();
        RefreshDisplay();
    }

    // Re-reads the bound action's display string - called after Setup, and
    // again after a rebind/reset so this row reflects the latest binding.
    public void RefreshDisplay()
    {
        (var action, int bindingIndex) = KeyBindingManager.Instance.Resolve(boundKey);
        if (currentKeyText != null && action != null && bindingIndex >= 0)
        {
            currentKeyText.text = KeyBindingManager.GetKeyDisplayName(action, bindingIndex);
        }
    }

    // Hooked up to the row's rebind Button OnClick in the Inspector.
    public void OnRebindClicked()
    {
        if (rebindButton != null) rebindButton.interactable = false;
        if (rebindButtonText != null) rebindButtonText.text = LocalizationSettings.StringDatabase.GetLocalizedString(UITableName, "keybind_press_any_key");

        KeyBindingManager.Instance.StartRebind(boundKey, result =>
        {
            RefreshDisplay();
            ResetButtonText();
            if (rebindButton != null) rebindButton.interactable = true;

            if (result == RebindResult.Duplicate && rebindButtonText != null)
            {
                // Briefly flash a warning in place of the button label before
                // settling back to the normal "Rebind" text.
                StartCoroutine(FlashDuplicateWarning());
            }
        });
    }

    System.Collections.IEnumerator FlashDuplicateWarning()
    {
        rebindButtonText.text = LocalizationSettings.StringDatabase.GetLocalizedString(UITableName, "keybind_duplicate_warning");
        yield return new WaitForSecondsRealtime(1.2f);
        ResetButtonText();
    }

    void ResetButtonText()
    {
        if (rebindButtonText != null) rebindButtonText.text = LocalizationSettings.StringDatabase.GetLocalizedString(UITableName, "keybind_rebind_button");
    }
}
}
