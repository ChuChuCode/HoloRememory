using UnityEngine;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using HR.Network;
using HR.Network.Lobby;

namespace HR.UI{
public class ModeSelectPanel : MonoBehaviour
{
    // Title screen - the only thing before this one now that Main UI is gone.
    [SerializeField] GameObject Title_UI;
    // Existing Host/Join screen (was shown directly from MainPanel.Game()
    // before Mode Select existed) - unchanged, just gated behind this pick now.
    [SerializeField] GameObject MultiplayerUI;
    // Shown instead of silently letting Host/Join buttons do nothing - Steam
    // only gets ONE init attempt (SteamManager.Awake(), at process start), so
    // if it's not running yet this stays true for the rest of the session;
    // the player needs to launch Steam and restart the game, not just open
    // it mid-session. Optional - leave unassigned if not wired up yet.
    [SerializeField] TMP_Text SteamWarningText;
    [SerializeField] LevelSelectPanel SoloLevelSelect;
    // Merged in from the now-deleted MainPanel/Main UI - this panel is the
    // only screen after Title now, so it owns Setting/Credit/Exit too.
    [SerializeField] GameObject Setting_UI;
    [SerializeField] GameObject Credit_UI;

    // Set by PlayerObject.LeaveGame() right before Mirror auto-reloads
    // Main_Scene (the offlineScene) - survives the scene load since it's
    // static, so Main_Scene can skip straight to Game UI instead of Title.
    public static bool ReturnToGameUI = false;
    // Set by Network_Manager.EndMatchAfterDelay() when a solo level ends
    // (win or loss) - same "survives the Main_Scene reload" static-flag
    // pattern as ReturnToGameUI/Network_Manager.ReturnToLobbyBrowser, so the
    // player lands back on Level Select instead of Title.
    public static bool ReturnToLevelSelect = false;

    void Start()
    {
        if (Setting_UI != null) Setting_UI.SetActive(false);
        if (Credit_UI != null) Credit_UI.SetActive(false);
        if (SteamWarningText != null) SteamWarningText.gameObject.SetActive(false);
    }
    void OnEnable()
    {
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
    }
    void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
    }
    void OnLocaleChanged(Locale locale)
    {
        RefreshSteamWarning();
    }
    void RefreshSteamWarning()
    {
        if (SteamWarningText == null) return;
        bool steamMissing = !SteamManager.Initialized;
        SteamWarningText.gameObject.SetActive(steamMissing);
        if (steamMissing)
        {
            SteamWarningText.text = LocalizationSettings.StringDatabase.GetLocalizedString("UI", "steam_required_warning");
        }
    }
    // "Mode Select UI" starts inactive, so THIS Start() never runs until
    // Open() is first called - checking the flag here would never fire.
    // Called instead by PressAnyKey.Start() (lives on Title, always active).
    public void CheckReturnToGameUI()
    {
        // Set when a solo level just ended (win or loss, see
        // Network_Manager.EndMatchAfterDelay) - skip Title and go straight
        // back to Level Select instead of stranding the player.
        if (ReturnToLevelSelect)
        {
            ReturnToLevelSelect = false;
            ReturnToGameUI = false;
            if (Title_UI != null) Title_UI.SetActive(false);
            gameObject.SetActive(false);
            SoloLevelSelect?.Open();
            return;
        }
        // Set by Network_Manager.OnClientDisconnect() when the connection
        // dropped for a reason we didn't choose (host crashed/quit, or we
        // got kicked) - no real host migration, so this is the fallback:
        // skip Title entirely and go straight to the lobby browser instead
        // of stranding the player with no explanation.
        if (Network_Manager.ReturnToLobbyBrowser)
        {
            Network_Manager.ReturnToLobbyBrowser = false;
            ReturnToGameUI = false;
            if (Title_UI != null) Title_UI.SetActive(false);
            Multiplayer();
            LobbyListManager.instance?.GetListOfLobbies();
            return;
        }
        if (!ReturnToGameUI) return;
        ReturnToGameUI = false;
        if (Title_UI != null) Title_UI.SetActive(false);
        Multiplayer();
    }
    // Called from Title's "press any key" listener (PressAnyKey.cs, on Title).
    public void Open()
    {
        gameObject.SetActive(true);
        if (Title_UI != null) Title_UI.SetActive(false);
    }
    // Multiplayer Card
    public void Multiplayer()
    {
        MultiplayerUI.SetActive(true);
        gameObject.SetActive(false);
        RefreshSteamWarning();
    }
    // Solo Card
    public void Solo()
    {
        if (SoloLevelSelect == null) return;
        gameObject.SetActive(false);
        SoloLevelSelect.Open();
    }
    // Setting Button
    public void Setting()
    {
        Setting_UI.SetActive(true);
        gameObject.SetActive(false);
    }
    // Credit Button
    public void Credit()
    {
        Credit_UI.SetActive(true);
        gameObject.SetActive(false);
    }
    // Exit Button - quits the application.
    public void Exit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}

}
