using Mirror;
using Steamworks;
using UnityEngine.SceneManagement;
using HR.Network.Lobby;
using HR.Network.Select;
using HR.Network.Result;

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
    [SyncVar(hook = nameof(PlayerSpell1Update))] public int Spell_1 = -1;
    [SyncVar(hook = nameof(PlayerSpell2Update))] public int Spell_2 = -1;
    [SyncVar(hook = nameof(PlayerKDAUpdate))] public int kill = -1;
    [SyncVar(hook = nameof(PlayerKDAUpdate))] public int death = -1;
    [SyncVar(hook = nameof(PlayerKDAUpdate))] public int assist = -1;
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
        // Set Start UI
        LobbyController.Instance.FindLocalPlayer();
        LobbyController.Instance.UpdateLobbyName();
        // Spell_1 = PlayerPrefs.GetInt("Spell_1", 1);
        // Spell_2 = PlayerPrefs.GetInt("Spell_2", 2);
    }
    public void Reset_All()
    {
        Ready = false;
        CharacterID = -1;
        Spell_1 = 0;
        Spell_2 = 0;
    }
    public override void OnStopClient()
    {
        Manager.PlayersInfoList.Remove(this);
        if (SceneManager.GetActiveScene().name == "Lobby_Scene")  LobbyController.Instance.UpdatePlayerList();
    }
    // if is owned -> call Cmd
    public void ChangeReady()
    {
        // if this oject is player's
        if (isOwned)
        {
            CmdSetPlayerReady();
        }
    }
    // client -> server(only on server)
    [Command]
    void CmdSetPlayerReady()
    {
        this.Ready = !this.Ready;
    }
    /// Ready Change
    void PlayerReadyUpdate(bool OldValue,bool NewValue)
    {
        if (SceneManager.GetActiveScene().name == "Lobby_Scene") LobbyController.Instance.UpdatePlayerList();
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
    /// TeamID change
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
        SelectController.Instance.UpdatePlayerUI();
    }
    [Command]
    public void CmdAddMessage(string userName, string message)
    {
        Chat_Controller.Instance.RpcAddMessage(userName, message);
    }
    /// Spell 1 change
    public void CanSpell1Change(int SpellID)
    {
        if (isOwned)
        {
            CmdSetSpell1Change(SpellID);
        }
    }
    /// <summary>
    /// Spell 1 to Selected Spell ID (on Server)
    /// </summary>
    /// <param name="SpellID">Selected Spell ID</param>
    [Command]
    void CmdSetSpell1Change(int SpellID)
    {
        Spell_1 = SpellID;
    }
    /// <summary>
    /// When Server Changed Spell 1 will call this method on client
    /// </summary>
    void PlayerSpell1Update(int OldValue,int NewValue)
    {
        SelectController.Instance.UpdatePlayerList();
    }
    /// Spell 2 change
    public void CanSpell2Change(int SpellID)
    {
        if (isOwned)
        {
            CmdSetSpell2Change(SpellID);
        }
    }
    [Command]
    void CmdSetSpell2Change(int SpellID)
    {
        Spell_2 = SpellID;
    }
    void PlayerSpell2Update(int OldValue,int NewValue)
    {
        SelectController.Instance.UpdatePlayerList();
    }
    public void LeaveGame()
    {
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
    public void CanKDAChange(int kill,int death,int assist)
    {
        CmdSetKDA(kill,death,assist);
    }
    [ServerCallback]
    void CmdSetKDA(int kill,int death,int assist)
    {
        this.kill = kill;
        this.death = death;
        this.assist = assist;
    }
    void PlayerKDAUpdate(int OldValue,int NewValue)
    {
        ResultController.Instance.UpdateUI();
        ResultController.Instance.Show_Result(manager.LoseTeam, TeamID);
    }
}

}