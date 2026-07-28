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

    void Start()
    {
        if (Setting_UI != null) Setting_UI.SetActive(false);
        if (Credit_UI != null) Credit_UI.SetActive(false);
    }
    // Called from Title's button.
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
    // Exit Button - not a real app-quit anymore, just leaves this screen
    // and goes back to Title.
    public void Exit()
    {
        if (Title_UI != null) Title_UI.SetActive(true);
        gameObject.SetActive(false);
    }
}

}
