using UnityEngine;
using Steamworks;
using TMPro;

namespace HR.Network.Lobby{
// Opened by LobbyRoomItem.JoinLobby() when the picked room has a password.
// Assign panel/passwordInput/errorText in the Editor - Confirm is meant to
// be wired to this panel's own Join button. Cancel just needs a direct
// GameObject.SetActive(false) on the panel from its own button, same as
// the rest of the UI - no script method needed for that.
public class PasswordPromptPanel : MonoBehaviour
{
    public static PasswordPromptPanel Instance;
    [SerializeField] GameObject panel;
    [SerializeField] TMP_InputField passwordInput;
    [SerializeField] TMP_Text errorText;

    CSteamID pendingLobbyID;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }
    public void Open(CSteamID lobbyID)
    {
        pendingLobbyID = lobbyID;
        if (passwordInput != null) passwordInput.text = "";
        if (errorText != null) errorText.gameObject.SetActive(false);
        if (panel != null) panel.SetActive(true);
    }
    // Confirm Button
    public void Confirm()
    {
        string entered = passwordInput != null ? passwordInput.text : "";
        if (SteamLobby.Instance.TryJoinWithPassword(pendingLobbyID, entered))
        {
            if (panel != null) panel.SetActive(false);
            return;
        }
        if (errorText != null)
        {
            errorText.text = "Wrong password";
            errorText.gameObject.SetActive(true);
        }
    }
}
}
