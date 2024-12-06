using UnityEngine;
using UnityEngine.Audio;

namespace HR.Global{
public enum Audio_Type 
{
    MIXER_MASTER,
    MIXER_MUSIC,
    MIXER_SFX,
    MIXER_VOICE
}
public class Setting_Component : MonoBehaviour
{
    public static Setting_Component Instance;
    [SerializeField] AudioMixer mixer;
    [Header("Audio Value")]
    public int Audio_Master;
    public int Audio_Music;
    public int Audio_SFX;
    public int Audio_Voice;
    [SerializeField] float scale = 10000f;
    [Header("Constant String")]
    const string MIXER_MASTER = "Master";
    const string MIXER_MUSIC = "Music";
    const string MIXER_SFX = "SFX";
    const string MIXER_VOICE = "Voice";
    void Start()
    {
        if (Instance is null) Instance = this;
        GetDataFromPlayerPrefs();
        // Set Mixer
        SetMixer(Audio_Type.MIXER_MASTER,Audio_Master);
        SetMixer(Audio_Type.MIXER_MUSIC,Audio_Music);
        SetMixer(Audio_Type.MIXER_SFX,Audio_SFX);
        SetMixer(Audio_Type.MIXER_VOICE,Audio_Voice);
    }
    public void GetDataFromPlayerPrefs()
    {
        /// Audio
        // Master
        Audio_Master = PlayerPrefs.GetInt("Audio_Master",5000);
        // Music
        Audio_Music = PlayerPrefs.GetInt("Audio_Music",5000);
        // SFX
        Audio_SFX = PlayerPrefs.GetInt("Audio_SFX",5000);
        // Voice
        Audio_Voice = PlayerPrefs.GetInt("Audio_Voice",5000);


    }

    public void SaveDataToPlayerPrefs()
    {
        /// Audio
        // Audio_Master
        PlayerPrefs.SetInt("Audio_Master",Audio_Master);
        // Audio_Music
        PlayerPrefs.SetInt("Audio_Music",Audio_Music);
        // Audio_SFX
        PlayerPrefs.SetInt("Audio_SFX",Audio_SFX);
        // Audio_Voice
        PlayerPrefs.SetInt("Audio_Voice",Audio_Voice);

    }
    public void SetMixer(Audio_Type type,float value)
    {
        switch (type)
        {
            case Audio_Type.MIXER_MASTER:
                mixer.SetFloat(MIXER_MASTER,Mathf.Log(value/scale)*20);
                break;
            case Audio_Type.MIXER_MUSIC:
                mixer.SetFloat(MIXER_MUSIC,Mathf.Log(value/scale)*20);
                break;
            case Audio_Type.MIXER_SFX:
                mixer.SetFloat(MIXER_SFX,Mathf.Log(value/scale)*20);
                break;
            case Audio_Type.MIXER_VOICE:
                mixer.SetFloat(MIXER_VOICE,Mathf.Log(value/scale)*20);
                break;
        }
    }
    public void SetAllMixer()
    {
        SetMixer(Audio_Type.MIXER_MASTER,Audio_Master);
        SetMixer(Audio_Type.MIXER_MUSIC,Audio_Music);
        SetMixer(Audio_Type.MIXER_SFX,Audio_SFX);
        SetMixer(Audio_Type.MIXER_VOICE,Audio_Voice);
    }
}

}