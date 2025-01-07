using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;
using HR.Object.Player;

namespace HR.UI{
public class LocalPlayerInfo : NetworkBehaviour
{
    public static LocalPlayerInfo Instance;
    [SerializeField] TMP_Text Time_Text;
    [SerializeField] TMP_Text Kill_Text;
    [SerializeField] TMP_Text Death_Text;
    [SerializeField] TMP_Text Assist_Text;    
    float started_time = 0f;
    [SyncVar] public float timer;
    void Start()
    {
        if (Instance == null) Instance = this;
        started_time = Time.time;
    }
    void Update()
    {
        Time_Text.text = Time_format();
    }
    float GetTime()
    {
        float duration = Time.time - started_time;
        return duration;
    }
    string Time_format()
    {
        timer = GetTime();
        int min = (int) timer / 60;
        int sec = (int) timer % 60;
        return string.Format("{0:00}:{1:00}", min, sec);
    }
    public void Update_KDA(CharacterBase characterBase)
    {
        Kill_Text.text = characterBase.kill.ToString();
        Death_Text.text = characterBase.death.ToString();
        Assist_Text.text = characterBase.assist.ToString();
    }
}

}