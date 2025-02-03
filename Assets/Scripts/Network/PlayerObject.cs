using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;
using HR.Network.Lobby;
using HR.Network.Select;

namespace HR.Network{
public class PlayerObject : NetworkBehaviour
{
     // PlayerData
    [SyncVar] public int ConnectionID;
    [SyncVar] public int PlayerIdNumber;
    [SyncVar] public ulong PlayerSteamID;
    [SyncVar(hook = nameof(PlayerNameUdate))] public string PlayerName;
    [SyncVar(hook = nameof(PlayerReadyUpdate))] public bool Ready;
    [SyncVar(hook = nameof(PlayerTeamUpdate))] public int TeamID = 0;
    [SyncVar(hook = nameof(CharacterSelect))] public int CharacterID = -1;
    [SyncVar(hook = nameof(PlayerSpell1Update))] public int Spell_1 ;
    [SyncVar(hook = nameof(PlayerSpell2Update))] public int Spell_2 ;
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
    }

    public override void OnStopClient()
    {
        Manager.PlayersInfoList.Remove(this);
        if (SceneManager.GetActiveScene().name == "Lobby_Scene")  LobbyController.Instance.UpdatePlayerList();
    }
    /// Ready Change
    void PlayerReadyUpdate(bool OldValue,bool NewValue)
    {
        if (isServer)
        {
            this.Ready = NewValue;
        }
        // Client
        if (isClient)
        {
            if (SceneManager.GetActiveScene().name == "Lobby_Scene") LobbyController.Instance.UpdatePlayerList();
        }
    }
    // client -> server(only on server)
    [Command]
    void CmdSetPlayerReady()
    {
        // Ask Everyone to update my value
        this.PlayerReadyUpdate(this.Ready, !this.Ready);
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
    /// Name Change
    [Command]
    void CmdSetPlayerName(string PlayerName)
    {
        this.PlayerNameUdate(this.PlayerName,PlayerName);
    }
    public void PlayerNameUdate(string OldValue,string NewValue)
    {
        // Host
        if (isServer)
        {
            this.PlayerName = NewValue;
        }
        // Client
        if (isClient)
        {
            LobbyController.Instance.UpdatePlayerList();
        }
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
        manager.ChnageScene(SceneName);
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
        // Ask Everyone to update my value
        this.PlayerTeamUpdate(this.TeamID, TeamID);
    }
    public void PlayerTeamUpdate(int OldValue,int NewValue)
    {
        if (isServer)
        {
            this.TeamID = NewValue;
        }
        // Client
        if (isClient)
        {
            LobbyController.Instance.UpdatePlayerList();
        }
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
        // Ask Everyone to update my value
        this.CharacterSelect(this.CharacterID, CharacterID);
    }
    public void CharacterSelect(int OldValue,int NewValue)
    {
        if (isServer)
        {
            this.CharacterID = NewValue;
        }
        // Client
        if (isClient)
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
    [Command]
    void CmdSetSpell1Change(int SpellID)
    {
        // Ask Everyone to update my value
        this.PlayerSpell1Update(this.Spell_1, SpellID);
    }
    public void PlayerSpell1Update(int OldValue,int NewValue)
    {
        if (isServer)
        {
            this.Spell_1 = NewValue;
        }
        // Client
        if (isClient)
        {
            SelectController.Instance.UpdatePlayerList();
        }
    }
    /// Spell 2 change
    public void CanSpell2Change(int SpellID)
    {
        if (isOwned)
        {
            CmdSetSpell2Change(SpellID);
        }
    }
    void CmdSetSpell2Change(int SpellID)
    {
        // Ask Everyone to update my value
        this.PlayerSpell2Update(this.Spell_2, SpellID);
    }
    public void PlayerSpell2Update(int OldValue,int NewValue)
    {
        if (isServer)
        {
            this.Spell_2 = NewValue;
        }
        // Client
        if (isClient)
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
}

}