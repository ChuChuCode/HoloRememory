using UnityEngine;

namespace HR.UI{
public class ModeSelectPanel : MonoBehaviour
{
    // Title screen - the only thing before this one now that Main UI is gone.
    [SerializeField] GameObject Title_UI;
    // Existing Host/Join screen (was shown directly from MainPanel.Game()
    // before Mode Select existed) - unchanged, just gated behind this pick now.
    [SerializeField] GameObject MultiplayerUI;
    // TODO: solo mode doesn't exist yet (character select -> map select ->
    // start, skipping Lobby entirely). Point this at a placeholder for now.
    [SerializeField] GameObject SoloUI;
    // Merged in from the now-deleted MainPanel/Main UI - this panel is the
    // only screen after Title now, so it owns Setting/Credit/Exit too.
    [SerializeField] GameObject Setting_UI;
    [SerializeField] GameObject Credit_UI;

    // Set by PlayerObject.LeaveGame() right before Mirror auto-reloads
    // Main_Scene (the offlineScene) - survives the scene load since it's
    // static, so Main_Scene can skip straight to Game UI instead of Title.
    public static bool ReturnToGameUI = false;

    void Start()
    {
        if (Setting_UI != null) Setting_UI.SetActive(false);
        if (Credit_UI != null) Credit_UI.SetActive(false);
    }
    // "Mode Select UI" starts inactive, so THIS Start() never runs until
    // Open() is first called - checking the flag here would never fire.
    // Called instead by PressAnyKey.Start() (lives on Title, always active).
    public void CheckReturnToGameUI()
    {
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
    }
    // Solo Card
    public void Solo()
    {
        if (SoloUI == null) return;
        SoloUI.SetActive(true);
        gameObject.SetActive(false);
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
