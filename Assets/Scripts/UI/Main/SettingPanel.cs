using UnityEngine;
using UnityEngine.UI;
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
    [SerializeField] Setting_Component setting_Component;
    void OnEnable()
    {
        // Set UI with Setting_Component
        Master.value = setting_Component.Audio_Master;
        Music.value = setting_Component.Audio_Music;
        SFX.value = setting_Component.Audio_SFX;
        Voice.value = setting_Component.Audio_Voice;

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
        Back();
    }
    // Back Button
    public void Back()
    {
        // Set Value Back 
        Master.value = setting_Component.Audio_Master;
        Music.value = setting_Component.Audio_Music;
        SFX.value = setting_Component.Audio_SFX;
        Voice.value = setting_Component.Audio_Voice;

        Main_UI.SetActive(true);
        gameObject.SetActive(false);
    }
}

}