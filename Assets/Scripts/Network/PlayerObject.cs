using Mirror;
using Steamworks;
using UnityEngine.SceneManagement;
using HR.Network.Lobby;
using HR.Network.Result;
using UnityEngine;
using HR.Object.Player;
using HR.UI;

namespace HR.Network{
public class PlayerObject : NetworkBehaviour
{
     // PlayerData
    [SyncVar] public int ConnectionID;
    [SyncVar] public int PlayerIdNumber;
    [SyncVar] public ulong PlayerSteamID;
    [SyncVar(hook = nameof(PlayerNameUpdate))] public string PlayerName;
    [SyncVar(hook = nameof(PlayerReadyUpdate))] public bool Ready;
    [SyncVar(hook = nameof(PlayerTeamUpdate))] public int TeamID = 0;
    [SyncVar(hook = nameof(CharacterSelect))] public int CharacterID = -1;
    private Network_Manager manager;

    public Network_Manager Manager
    {
        get
        {
            if (manager != null)
            {
                return manager;
            }
            return manager = Network_Manager.singleton as Network_Manager;
        }
    }
    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }
    public override void OnStartClient()
    {
        Manager.PlayersInfoList.Add(this);
        LobbyController.Instance.UpdateLobbyName();
        LobbyController.Instance.UpdatePlayerList();
    }
    public override void OnStartAuthority()
    {
        CmdSetPlayerName(SteamFriends.GetPersonaName().ToString());
        gameObject.name = "LocalGamePlayer";
        // Set LocalPlayer
        LobbyController.Instance.LocalPlayerController = this;
        // Set Network_Manager
        manager.LocalPlayerObject = this;
        // Set Start/Ready button
        LobbyController.Instance.RefreshMainButton();
        LobbyController.Instance.UpdateLobbyName();
    }
    public override void OnStopClient()
    {
        Manager.PlayersInfoList.Remove(this);
        if (SceneManager.GetActiveScene().name == "Lobby_Scene")  LobbyController.Instance.UpdatePlayerList();
    }
    // if is owned -> call Cmd
    public void ChangeReady(bool ready)
    {
        if (isOwned)
        {
            CmdSetPlayerReady(ready);
        }
    }
    public void ChangeReady()
    {
        // if this oject is player's
        if (isOwned)
        {
            CmdSetPlayerReady(!this.Ready);
        }
    }
    // client -> server(only on server)
    [Command]
    void CmdSetPlayerReady(bool ready)
    {
        this.Ready = ready;
    }
    /// Ready Change
    void PlayerReadyUpdate(bool OldValue,bool NewValue)
    {
        if (SceneManager.GetActiveScene().name == "Lobby_Scene")
        {
            LobbyController.Instance.UpdatePlayerList();
        }
    }
    /// Name Change
    [Command]
    void CmdSetPlayerName(string PlayerName)
    {
        this.PlayerName = PlayerName;
    }
    public void PlayerNameUpdate(string OldValue,string NewValue)
    {
        LobbyController.Instance.UpdatePlayerList();
    }
    /// Start Button
    public void CanStartGame(string SceneName)
    {
        // Reset Ready for select scene
        // ChangeReady();
        if (isOwned)
        {
            CmdCanStartGame(SceneName);
        }
    }
    [Command]
    public void CmdCanStartGame(string SceneName)
    {
        manager.ChangeScene(SceneName);
    }
    /// Team change - auto-assigned on connect (Network_Manager.NextAutoTeam)
    /// for an even initial spread, but freely re-pickable afterward. Teams
    /// are allowed to be uneven in size (asymmetric) - any of the 8 colors
    /// can have any number of players, including zero or several.
    public void CanTeamJoin(int TeamID)
    {
        if (isOwned)
        {
            CmdSetPlayerTeam(TeamID);
        }
    }
    [Command]
    void CmdSetPlayerTeam(int TeamID)
    {
        this.TeamID = TeamID;
    }
    void PlayerTeamUpdate(int OldValue,int NewValue)
    {
        LobbyController.Instance.UpdatePlayerList();
    }
    /// Set Character ID
    public void CanSetCharacter(int CharacterID)
    {
        if (isOwned)
        {
            CmdSeCharacterID(CharacterID);
        }
    }
    [Command]
    void CmdSeCharacterID(int CharacterID)
    {
        this.CharacterID = CharacterID;

    }
    void CharacterSelect(int OldValue,int NewValue)
    {
        if (SceneManager.GetActiveScene().name == "Lobby_Scene")
        {
            LobbyController.Instance.UpdatePlayerList();
        }
    }
    [Command]
    public void CmdAddMessage(string userName, string message)
    {
        Chat_Controller.Instance.RpcAddMessage(userName, message);
    }
    public void LeaveGame()
    {
        // Mirror auto-reloads Main_Scene (offlineScene) below - tell it to
        // land on Game UI instead of defaulting back to Title.
        ModeSelectPanel.ReturnToGameUI = true;
        // isServer
        if (NetworkServer.active)
        {
            Manager.StopHost();
        }
        // Client
        if (NetworkClient.active)
        {
            Manager.StopClient();
        }
        Destroy(Manager.gameObject);
    }
}

}