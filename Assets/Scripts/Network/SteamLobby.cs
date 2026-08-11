using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Steamworks;
using System;
using UnityEngine.UI;
using HR.Network.Lobby;

namespace HR.Network{
public class SteamLobby : MonoBehaviour
{
    public static SteamLobby Instance;

    [Header("Lobby Create Callbacks")]
    protected Callback<LobbyCreated_t> lobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequested;
    protected Callback<LobbyEnter_t> lobbyEntered;
    [Header("Lobby Join Callbacks")]
    protected Callback<LobbyMatchList_t> lobbyList;
    protected Callback<LobbyDataUpdate_t> lobbyDataUpdated;
    public List<CSteamID> lobbyIDs = new List<CSteamID>();
    [Header("variables")]
    public ulong CurrentLobbyID;
    const string HostAddressKey = "HostAddress";
    // Published as Steam Lobby Data so the room *browser* (not yet joined,
    // so no Mirror/GameSettings sync) can still show map/mode per room.
    public const string MapKey = "Map";
    public const string ModeKey = "Mode";
    [SerializeField] Button HostButton;
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
        // networkManager = Network_Manager.singleton as Network_Manager;
        // If initialized failed
        if (!SteamManager.Initialized) return;
        if (Instance == null)
        {
            Instance = this;
        }

        lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
        lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
        lobbyList = Callback<LobbyMatchList_t>.Create(OnGetLobbyList);
        lobbyDataUpdated = Callback<LobbyDataUpdate_t>.Create(OnGetLobbyData);
        
    }
    public void HostLobby()
    {
        print("Host Button Press");
        // Disable host button
        HostButton.interactable = false;
        // Friend only
        // SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly,networkManager.maxConnections);
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic,Manager.maxConnections);
    }
    // Quick Match Button - reuses the same list request as the room
    // browser, but instead of displaying results it auto-joins the first
    // non-full lobby found, or hosts a new one if none qualify.
    bool isQuickMatching = false;
    public void QuickMatch()
    {
        isQuickMatching = true;
        GetLobbyList();
    }
    void OnLobbyCreated(LobbyCreated_t callback)
    {
        print("Lobby Create");
        // If Lobby Create Error -> Show Host button
        if (callback.m_eResult != EResult.k_EResultOK)
        {
            return;
        }
        Manager.StartHost();

        SteamMatchmaking.SetLobbyData(
            new CSteamID(callback.m_ulSteamIDLobby), 
            HostAddressKey, SteamUser.GetSteamID().ToString()
        );

        SteamMatchmaking.SetLobbyData(
            new CSteamID(callback.m_ulSteamIDLobby),
            "name",
            SteamFriends.GetPersonaName().ToString()+ "'s LOBBY"
        );

        // Seed Map/Mode immediately so the room browser has something to
        // show even before the host touches either setting.
        CurrentLobbyID = callback.m_ulSteamIDLobby;
        if (GameSettings.Instance != null)
        {
            UpdateLobbyMap(GameSettings.Instance.MapName);
            GameModeConfig config = GameSettings.Instance.CurrentConfig();
            UpdateLobbyMode(config != null ? config.DisplayName : GameSettings.Instance.Mode.ToString());
        }
    }
    // Called by GameSettings whenever the host changes map/mode, so the
    // room browser (anyone not yet joined) reflects the current pick too.
    public void UpdateLobbyMap(string mapName)
    {
        if (!NetworkServer.active || CurrentLobbyID == 0) return;
        SteamMatchmaking.SetLobbyData(new CSteamID(CurrentLobbyID), MapKey, mapName);
    }
    public void UpdateLobbyMode(string modeName)
    {
        if (!NetworkServer.active || CurrentLobbyID == 0) return;
        SteamMatchmaking.SetLobbyData(new CSteamID(CurrentLobbyID), ModeKey, modeName);
    }
    void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback)
    {
        print("Join Lobby");
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }
    void OnLobbyEntered(LobbyEnter_t callback)
    {
        print("Lobby Entered");
        // Everyone
        CurrentLobbyID = callback.m_ulSteamIDLobby;

        // Client
        // Alreadyt active
        if (NetworkServer.active) return;

        string hostAddress = SteamMatchmaking.GetLobbyData(
            new CSteamID(callback.m_ulSteamIDLobby),
            HostAddressKey
        );
        Manager.networkAddress = hostAddress;
        Manager.StartClient();
    }
    public void GetLobbyList()
    {
        if (lobbyIDs.Count > 0) lobbyIDs.Clear();
        SteamMatchmaking.AddRequestLobbyListResultCountFilter(60);
        SteamMatchmaking.RequestLobbyList();

    }
    public void JoinLobby(CSteamID lobbyID)
    {
        SteamMatchmaking.JoinLobby(lobbyID);
    }
    void OnGetLobbyList(LobbyMatchList_t callback)
    {
        if (isQuickMatching)
        {
            isQuickMatching = false;
            for (int i = 0; i < callback.m_nLobbiesMatching; i++)
            {
                CSteamID lobbyID = SteamMatchmaking.GetLobbyByIndex(i);
                if (SteamMatchmaking.GetNumLobbyMembers(lobbyID) < SteamMatchmaking.GetLobbyMemberLimit(lobbyID))
                {
                    JoinLobby(lobbyID);
                    return;
                }
            }
            // No open room found - host a fresh one instead.
            HostLobby();
            return;
        }

        // Will call twice when Leave Game Back to Lobby
        if (LobbyListManager.instance.listOfLobbies.Count > 0) LobbyListManager.instance.DestroyLobbies();
        for(int i = 0 ; i < callback.m_nLobbiesMatching ; i++)
        {
            CSteamID lobbyID = SteamMatchmaking.GetLobbyByIndex(i);
            lobbyIDs.Add(lobbyID);
            // Triggers a LobbyDataUpdate_t callback.
            SteamMatchmaking.RequestLobbyData(lobbyID);
        }
    }
    void OnGetLobbyData(LobbyDataUpdate_t callback)
    {
        LobbyListManager.instance.DisplayLobbies(lobbyIDs,callback);
    }
}

}