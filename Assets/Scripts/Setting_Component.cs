using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HR.Global{
public class Setting_Component : MonoBehaviour
{
    [SerializeField] int Audio_Master;
    [SerializeField] int Audio_Music;
    [SerializeField] int Audio_SFX;
    [SerializeField] int Audio_Voice;
    void Start()
    {
        GetDataFromPlayerPrefs();
    }
    void GetDataFromPlayerPrefs()
    {
        /// Audio
        // Master
        Audio_Master = PlayerPrefs.GetInt("Audio_Master",50);
        // Music
        Audio_Music = PlayerPrefs.GetInt("Audio_Music",50);
        // SFX
        Audio_SFX = PlayerPrefs.GetInt("Audio_SFX",50);
        // Voice
        Audio_Voice = PlayerPrefs.GetInt("Audio_Voice",50);


    }

    public bool SaveDataToPlayerPrefs<T>(string key, T value)
    {
        switch (key)
        {
            /// Audio
            case "Audio_Master":
            case "Audio_Music":
            case "Audio_SFX":
            case "Audio_Voice":
                // Check value type
                if ( !(value is int i_value)) return false;

                // Check value range
                if (i_value < 0 || i_value > 100) return false;

                // Set local parameter
                if (key is "Audio_Master") Audio_Master = i_value;
                else if (key is "Audio_Music") Audio_Music = i_value;
                else if (key is "Audio_SFX") Audio_SFX = i_value;
                else if (key is "Audio_Voice") Audio_Voice = i_value;

                // Set to PlayerPrefs
                PlayerPrefs.SetInt(key,i_value);
                return true;
            default:
                return false;
        }
    }
}

}