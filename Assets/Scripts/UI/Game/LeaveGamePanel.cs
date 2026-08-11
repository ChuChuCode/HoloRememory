using Mirror;
using UnityEngine;
using HR.Network;

namespace HR.UI{
// Minimal ESC menu for leaving a match. Kept separate from OptionPanel (an
// old MOBA panel that isn't placed anywhere in the current scene, and
// whose Start() isn't null-safe enough to partially activate - it touches
// End_Button/Back_Button/AutoAttacktoggle/Setting_Component unconditionally,
// none of which this game uses).
// Uses a CanvasGroup instead of GameObject.SetActive() to show/hide -
// keeping the GameObject itself always active means Awake() (and this
// singleton's Instance) is never at the mercy of whatever active/inactive
// state someone left it in while editing the scene, and the panel can be
// made invisible/non-interactive at design time (Alpha 0 in the Inspector)
// without that fighting with anything at runtime.
[RequireComponent(typeof(CanvasGroup))]
public class LeaveGamePanel : MonoBehaviour
{
    public static LeaveGamePanel Instance;
    CanvasGroup canvasGroup;

    Network_Manager manager;
    public Network_Manager Manager
    {
        get
        {
            if (manager != null) return manager;
            return manager = Network_Manager.singleton as Network_Manager;
        }
    }

    public bool IsVisible => canvasGroup.alpha > 0f;

    void Awake()
    {
        if (Instance == null) Instance = this;
        canvasGroup = GetComponent<CanvasGroup>();
        SetVisible(false);
    }

    public void SetVisible(bool visible)
    {
        canvasGroup.alpha = visible ? 1f : 0f;
        canvasGroup.interactable = visible;
        canvasGroup.blocksRaycasts = visible;
    }

    public void LeaveGame()
    {
        if (NetworkServer.active)
        {
            // Host leaving mid-match ends the match for the whole room and
            // brings everyone back to the Lobby together (same as a normal
            // match end) instead of tearing down the session - the server
            // keeps running, nobody actually disconnects. Leaving the room
            // entirely (before a match even starts) is a different action -
            // see LobbyController/PlayerObject's own leave button.
            Manager.ChangeScene("Lobby_Scene");
            return;
        }

        // Non-host: just this player leaves. Network_Manager.OnServerDisconnect
        // handles removing them from the match and checking whether it
        // should end as a result (see the rules around Player_List there).
        ModeSelectPanel.ReturnToGameUI = true;
        if (NetworkClient.active) Manager.StopClient();
        Destroy(Manager.gameObject);
    }
}
}
