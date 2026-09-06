using UnityEngine;
using TMPro;
using Mirror;
using HR.Network;

namespace HR.UI{
public class LocalPlayerInfo : NetworkBehaviour
{
    public static LocalPlayerInfo Instance;
    [SerializeField] TMP_Text Time_Text;
    // Fallback only, in case GameSettings isn't around - real duration comes
    // from GameSettings.TimeLimit, same value shown in the Lobby above.
    [SerializeField] float matchDuration = 180f;
    public double timer;
    bool matchEnded = false;

    private Network_Manager manager;
    public Network_Manager Manager
    {
        get
        {
            if (manager != null) return manager;
            return manager = Network_Manager.singleton as Network_Manager;
        }
    }

    void Awake()
    {
        if (Instance == null) Instance = this;
    }
    void Update()
    {
        Time_Text.text = Time_format();

        // Server-authoritative - once time's up, end the match (a draw,
        // same 5s-then-lobby flow as CheckGameOver()'s last-one-standing case).
        if (isServer && !matchEnded && GetRemaining() <= 0)
        {
            matchEnded = true;
            Manager.EndMatch();
        }
    }
    // Anchored on GameSettings.matchCountdownEndTime (the same instant
    // player input actually unlocks) instead of this object's own spawn
    // time - it used to start counting the moment this HUD object spawned,
    // well before the match-start lock/countdown even finished, silently
    // burning match time nobody could actually play through. Clamped to
    // never exceed duration, so the display just holds steady at the full
    // time limit during the lock window instead of counting down early.
    double GetRemaining()
    {
        if (GameSettings.Instance == null) return matchDuration;
        double elapsed = NetworkTime.time - GameSettings.Instance.matchCountdownEndTime;
        double duration = GameSettings.Instance.TimeLimit;
        return System.Math.Min(duration - elapsed, duration);
    }
    string Time_format()
    {
        timer = Mathf.Max(0f, (float)GetRemaining());
        int min = (int) timer / 60;
        int sec = (int) timer % 60;
        return string.Format("{0:00}:{1:00}", min, sec);
    }
}

}