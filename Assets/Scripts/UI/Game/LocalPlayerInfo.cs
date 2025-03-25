using UnityEngine;
using TMPro;
using Mirror;
using HR.Object.Player;

namespace HR.UI{
public class LocalPlayerInfo : NetworkBehaviour
{
    public static LocalPlayerInfo Instance;
    [SerializeField] TMP_Text Kill_Text;
    [SerializeField] TMP_Text Death_Text;
    [SerializeField] TMP_Text Assist_Text;
    [SerializeField] TMP_Text Minion_Text;
    [SerializeField] TMP_Text Time_Text;
    [SyncVar] double started_time = 0f;
    public double timer;
    void Awake()
    {
        if (Instance == null) Instance = this;
    }
    void Start()
    {
        if (isServer)
        {
            started_time = NetworkTime.time;
        }
    }
        void Update()
    {
        Time_Text.text = Time_format();
    }
    double GetTime()
    {
        double duration = NetworkTime.time - started_time;
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
        Minion_Text.text = characterBase.minion.ToString();
    }
}

}