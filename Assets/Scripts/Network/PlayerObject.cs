using Mirror;
using Steamworks;
using UnityEngine.SceneManagement;
using HR.Network.Lobby;
using HR.Network.Select;
using HR.Network.Result;
using UnityEngine;
using HR.Object.Player;

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
    [SyncVar(hook = nameof(PlayerSpellUpdate))] public int Spell_1 = -1;
    [SyncVar(hook = nameof(PlayerSpellUpdate))] public int Spell_2 = -1;
    [SyncVar(hook = nameof(PlayerKDAUpdate))] public int kill = -1;
    [SyncVar(hook = nameof(PlayerKDAUpdate))] public int death = -1;
    [SyncVar(hook = nameof(PlayerKDAUpdate))] public int assist = -1;
    [SyncVar(hook = nameof(PlayerKDAUpdate))] public int minion = -1;
    [SyncVar(hook = nameof(PlayerKDAUpdate))] public int tower = -1;
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
        LobbyController.Instance.SetStartButton();
        LobbyController.Instance.UpdateLobbyName();
        // Set Spell index
        CanSpell1Change( PlayerPrefs.GetInt("Spell_1", 1) );
        CanSpell2Change( PlayerPrefs.GetInt("Spell_2", 2) );
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
        if (SceneManager.GetActiveScene().name == "Select_Scene")
        {
            if (isServer) SelectController.Instance.CheckIfAllReady();
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
        if (SceneManager.GetActiveScene().name == "Select_Scene")
        {
            SelectController.Instance.UpdatePlayerUI();
        }
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
    /// <summary>
    /// When Server Changed Spell 1 will call this method on client
    /// </summary>
    void PlayerSpellUpdate(int OldValue,int NewValue)
    {
        if (SceneManager.GetActiveScene().name == "Select_Scene") 
        {
            SelectController.Instance.UpdatePlayerList();
        }
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
    public void CanKDAChange(CharacterBase characterBase)
    {
        CmdSetKDA(characterBase);
    }
    [ServerCallback]
    void CmdSetKDA(CharacterBase characterBase)
    {
        this.kill = characterBase.kill;
        this.death = characterBase.death;
        this.assist = characterBase.assist;
        this.minion = characterBase.minion;
        this.tower = characterBase.tower;
    }
    void PlayerKDAUpdate(int OldValue,int NewValue)
    {
        if (SceneManager.GetActiveScene().name == "Result_Scene")
        {
            ResultController.Instance.UpdateUI();
            ResultController.Instance.Show_Result(manager.LoseTeam, TeamID);
        } 
    }
}

}