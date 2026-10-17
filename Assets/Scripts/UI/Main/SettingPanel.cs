using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HR.Global;

namespace HR.UI{
public class SettingPanel : MonoBehaviour
{
    [SerializeField] GameObject Main_UI;
    [Header("Setting Object")]
    public Slider Master;
    public Slider Music;
    public Slider SFX;
    public Slider Voice;
    // Options are populated from the UI table's "language_native_name" Key
    // at Awake (see below), not authored in the Inspector - index still
    // maps straight across to the Language_Type enum, same as
    // OnLanguageChanged.
    public TMP_Dropdown Language;
    [SerializeField] Setting_Component setting_Component;
    // Recorded on OnEnable - the committed value from before this panel was
    // opened, so Back() can revert a previewed-but-not-saved language pick
    // the same way the audio sliders above revert to setting_Component's
    // own stored values.
    Language_Type originalLanguage;
    void Awake()
    {
        List<string> names = new List<string>();
        foreach (Language_Type language in System.Enum.GetValues(typeof(Language_Type)))
        {
            names.Add(Language_Component.GetNativeName(language));
        }
        Language.ClearOptions();
        Language.AddOptions(names);
    }
    void OnEnable()
    {
        // Set UI with Setting_Component
        Master.value = setting_Component.Audio_Master;
        Music.value = setting_Component.Audio_Music;
        SFX.value = setting_Component.Audio_SFX;
        Voice.value = setting_Component.Audio_Voice;
        if (Language != null && Language_Component.Instance != null)
        {
            originalLanguage = Language_Component.Instance.CurrentLanguage;
            Language.SetValueWithoutNotify((int)originalLanguage);
        }
    }
    // Language Dropdown's OnValueChanged (wire up in the Inspector) - only
    // PREVIEWS (every live LocalizeStringEvent updates immediately, same as
    // the audio sliders previewing through the mixer as you drag them), but
    // doesn't save yet - Save()/Back() below decide whether that preview
    // sticks or gets reverted to originalLanguage, same lifecycle as the
    // audio settings.
    public void OnLanguageChanged(int index)
    {
        if (Language_Component.Instance == null)
        {
            Debug.LogWarning("OnLanguageChanged fired but Language_Component.Instance is null - is it in this scene?");
            return;
        }
        Language_Component.Instance.PreviewLanguage((Language_Type)index);
    }
    // Save Button
    public void Save()
    {
        // Save to Setting_Component
        setting_Component.Audio_Master = (int) Master.value;
        setting_Component.Audio_Music = (int) Music.value;
        setting_Component.Audio_SFX = (int) SFX.value;
        setting_Component.Audio_Voice = (int) Voice.value;

        setting_Component.SaveDataToPlayerPrefs();
        setting_Component.GetDataFromPlayerPrefs();
        // Commits whatever language OnLanguageChanged already previewed -
        // CurrentLanguage/SelectedLocale are already correct, this just
        // makes the PlayerPrefs write (see PreviewLanguage) stick instead
        // of only lasting until Back()/next launch reverts it.
        Language_Component.Instance?.SaveDataToPlayerPrefs();
        ClosePanel();
    }
    // Back/Cancel Button
    public void Back()
    {
        // Set Value Back
        Master.value = setting_Component.Audio_Master;
        Music.value = setting_Component.Audio_Music;
        SFX.value = setting_Component.Audio_SFX;
        Voice.value = setting_Component.Audio_Voice;
        // Revert a previewed-but-not-saved language pick back to whatever
        // was active when this panel opened - same "cancel = no lasting
        // effect" contract as the audio sliders above.
        if (Language_Component.Instance != null)
        {
            Language_Component.Instance.PreviewLanguage(originalLanguage);
            Language.SetValueWithoutNotify((int)originalLanguage);
        }

        ClosePanel();
    }
    void ClosePanel()
    {
        Main_UI.SetActive(true);
        gameObject.SetActive(false);
    }
}

}