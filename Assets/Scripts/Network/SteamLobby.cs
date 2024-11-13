using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Steamworks;
using System;

public class SteamLobby : MonoBehaviour
{
    public static SteamLobby Instance;

    Network_Manager networkManager;
    [Header("Event")]
    protected Callback<LobbyCreated_t> lobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequested;
    protected Callback<LobbyEnter_t> lobbyEntered;
    [Header("variables")]
    public ulong CurrentLobbyID;
    const string HostAddressKey = "HostAddress";

    void Start()
    {
        networkManager = GetComponent<Network_Manager>();
        // networkManager = Network_Manager.singleton as Network_Manager;
        // If initialized failed
        if (!SteamManager.Initialized) return;

        lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
        lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);

        if (Instance == null)
        {
            Instance = this;
        }
    }
    public void HostLobby()
    {
        print("Host Button Press");

        SteamMatchmaking.CreateLobby(
            ELobbyType.k_ELobbyTypeFriendsOnly,
            networkManager.maxConnections
        );
    }
    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        print("Lobby Create");
        // If Lobby Create Error -> Show Host button
        if (callback.m_eResult != EResult.k_EResultOK)
        {
            return;
        }
        networkManager.StartHost();

        SteamMatchmaking.SetLobbyData(
            new CSteamID(callback.m_ulSteamIDLobby), 
            HostAddressKey, SteamUser.GetSteamID().ToString()
        );

        SteamMatchmaking.SetLobbyData(
            new CSteamID(callback.m_ulSteamIDLobby), 
            "name", 
            SteamFriends.GetPersonaName().ToString()+ "'s LOBBY"
        );
    }
    private void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback)
    {
        print("Join Lobby");
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }
    private void OnLobbyEntered(LobbyEnter_t callback)
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
        networkManager.networkAddress = hostAddress;
        networkManager.StartClient();
    }
}
