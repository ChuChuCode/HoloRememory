using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using Steamworks;
using UnityEngine.UI;
using UnityEngine.SocialPlatforms;

namespace HR.Network.Lobby{
public class LobbyController : MonoBehaviour
{
    public bool AllReady;
    public static LobbyController Instance;
    [Header("Lobby Prefab")]
    [SerializeField] GameObject LobbyPlayerPrefab;
    [Header("Team")]
    [SerializeField] Transform Team1_transform;
    [SerializeField] Transform Team2_transform;
    // Other Data
    public ulong CurrentLobbyID;
    public bool PlayerItemCreated = false;
    [Header("UI")]
    public TMP_Text LobbyNameText;
    public TMP_Text ReadyButtonText;
    public Button StartButton;
    public Button Team1_Join;
    public Button Team2_Join;
    public Button Viewer_Join;
    [Header("Manager")]
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
    List<Network_LobbyPlayer> LobbyPlayerList = new List<Network_LobbyPlayer>();
    public PlayerObject LocalPlayerController;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
    public void UpdateLobbyName()
    {
        CurrentLobbyID = SteamLobby.Instance.CurrentLobbyID;
        LobbyNameText.text = SteamMatchmaking.GetLobbyData(new CSteamID(CurrentLobbyID),"name");
    }
    public void FindLocalPlayer()
    {
        // LocalPlayerController = GameObject.Find("LocalGamePlayer").GetComponent<PlayerObject>();
        // Hide Start Button if is not Host
        if (LocalPlayerController.PlayerIdNumber != 1)
        {
            StartButton.gameObject.SetActive(false);
        }
    }
    public void UpdatePlayerList()
    {
        // Create Host Player(Already exit in room) 
        // LobbyPlayerList.OnStartAuthority
        if(!PlayerItemCreated)
        {
            // Host
            CreateHostPlayerItem();
        }
        // Create Owner Player
        // LobbyPlayerList.OnStartClient
        if (LobbyPlayerList.Count < Manager.PlayersInfoList.Count)
        {
            CreateClientPlayerItem();
        }
        // Check Anyone leave
        // LobbyPlayerList.OnStopClient
        if (LobbyPlayerList.Count > Manager.PlayersInfoList.Count)
        {
            RemovePlayerItem();
        }
        // Update things
        // LobbyPlayerList.PlayerNameUdate
        if (LobbyPlayerList.Count == Manager.PlayersInfoList.Count)
        {
            UpdatePlayerUI();
        }
    }
    public void CreateHostPlayerItem()
    {
        print("HOST create");
        foreach(PlayerObject player in Manager.PlayersInfoList)
        {
            GameObject LobbyPlayer = Instantiate(LobbyPlayerPrefab);
            Network_LobbyPlayer network_LobbyPlayer = LobbyPlayer.GetComponent<Network_LobbyPlayer>();

            network_LobbyPlayer.PlayerName = player.PlayerName;
            network_LobbyPlayer.ConnectionID = player.ConnectionID;
            network_LobbyPlayer.PlayerSteamID = player.PlayerSteamID;
            network_LobbyPlayer.isReady = player.Ready;
            network_LobbyPlayer.SetPlayerValues();
            // Set Team if #Team1_player < 5 -> add to Team1
            int team1_count = Manager.PlayersInfoList.FindAll(b => b.TeamID == 1).Count;
            if (team1_count < 5)
            {
                LobbyPlayer.transform.SetParent(Team1_transform);
                player.CanTeamJoin(1);
            }
            else
            {
                LobbyPlayer.transform.SetParent(Team2_transform);
                player.CanTeamJoin(2);
            }
            LobbyPlayer.transform.localScale = Vector3.one;
            LobbyPlayerList.Add(network_LobbyPlayer);
        }
        PlayerItemCreated = true;
    }
    public void CreateClientPlayerItem()
    {
        print("CLIENT ceate");
        foreach(PlayerObject player in Manager.PlayersInfoList)
        {
            if (!LobbyPlayerList.Any(b => b.ConnectionID == player.ConnectionID))
            {
                GameObject LobbyPlayer = Instantiate(LobbyPlayerPrefab);
                Network_LobbyPlayer network_LobbyPlayer = LobbyPlayer.GetComponent<Network_LobbyPlayer>();

                network_LobbyPlayer.PlayerName = player.PlayerName;
                network_LobbyPlayer.ConnectionID = player.ConnectionID;
                network_LobbyPlayer.PlayerSteamID = player.PlayerSteamID;
                network_LobbyPlayer.isReady = player.Ready;
                network_LobbyPlayer.SetPlayerValues();
                // Set Team if #Team1_player < 5 -> add to Team1
                int team1_count = Manager.PlayersInfoList.FindAll(b => b.TeamID == 1).Count;
                if (team1_count < 5)
                {
                    LobbyPlayer.transform.SetParent(Team1_transform);
                    // player.CanTeamJoin(1);
                }
                else
                {
                    LobbyPlayer.transform.SetParent(Team2_transform);
                    // player.CanTeamJoin(2);
                }
                LobbyPlayer.transform.localScale = Vector3.one;

                LobbyPlayerList.Add(network_LobbyPlayer);
            }
        }
    }
    public void UpdatePlayerUI()
    {
        foreach(PlayerObject player in Manager.PlayersInfoList)
        {
            foreach(Network_LobbyPlayer network_LobbyPlayer in LobbyPlayerList)
            {
                if (network_LobbyPlayer.ConnectionID == player.ConnectionID)
                {
                    network_LobbyPlayer.PlayerName = player.PlayerName;
                    network_LobbyPlayer.isReady = player.Ready;
                    network_LobbyPlayer.SetPlayerValues();
                    // Set team
                    if (player.TeamID == 1)
                    {
                        network_LobbyPlayer.transform.SetParent(Team1_transform);
                    }
                    else
                    {
                        network_LobbyPlayer.transform.SetParent(Team2_transform);
                    }
                    // if player is local player -> update Ready Button
                    if (player == LocalPlayerController)
                    {
                        UpdateButton();
                    }
                }
            }
        }
        CheckTeamButton();
        CheckIfAllReady();
    }
    public void RemovePlayerItem()
    {
        List<Network_LobbyPlayer> playerListItemToRemove = new List<Network_LobbyPlayer>();

        foreach(Network_LobbyPlayer playerlistItem in LobbyPlayerList)
        {
            if(!Manager.PlayersInfoList.Any(b => b.ConnectionID == playerlistItem.ConnectionID))
            {
                playerListItemToRemove.Add(playerlistItem);
            }
        }
        if (playerListItemToRemove.Count > 0)
        {
            foreach(Network_LobbyPlayer playerlistItemToRemove in playerListItemToRemove)
            {
                GameObject ObjectToRemove = playerlistItemToRemove.gameObject;
                LobbyPlayerList.Remove(playerlistItemToRemove);
                Destroy(ObjectToRemove);
                ObjectToRemove = null;
            }
        }
    }
    public void UpdateButton()
    {
        if (LocalPlayerController.Ready)
        {
            ReadyButtonText.text = "Unready";
        }
        else
        {
            ReadyButtonText.text = "Ready";
        }
    }
    public void CheckIfAllReady()
    {
        AllReady = false;

        foreach(PlayerObject player in Manager.PlayersInfoList)
        {
            if (player.Ready)
            {
                AllReady = true;
            }
            else
            {
                AllReady = false;
                break;
            }
        }
        // Start Button Clickable
        if (AllReady)
        {
            // Is Host
            if (LocalPlayerController.PlayerIdNumber == 1)
            {
                StartButton.interactable = true;
            }
            else
            {
                StartButton.interactable = false;
            }
        }
        else
        {
            StartButton.interactable = false;
        }
    }
    public void CheckTeamButton()
    {
        Team1_Join.interactable = true;
        Team2_Join.interactable = true;
        int team1_count = Manager.PlayersInfoList.FindAll(b => b.TeamID == 1).Count;
        int team2_count = Manager.PlayersInfoList.FindAll(b => b.TeamID == 2).Count;
        if (team1_count == 5)
        {
            Team1_Join.interactable = false;
        }
        if (team2_count == 5)
        {
            Team2_Join.interactable = false;
        }
        if (LocalPlayerController == null) return;
        /// Initial between OnStartClient and OnStartAuthority
        /// OnStartClient go first then OnStartAuthority
        /// but set LocalPlayerController on OnStartAuthority stage
        if (LocalPlayerController.Ready) 
        {
            Team1_Join.interactable = false;
            Team2_Join.interactable = false;
            return;
        }
        if (LocalPlayerController.TeamID == 1)
        {
            Team1_Join.interactable = false;
        }
        else
        {
            Team2_Join.interactable = false;
        }
    }
    // Ready Button
    public void ReadyPlayer()
    {
        LocalPlayerController.ChangeReady();
    }
    // Start Button
    public void StarGame(string SceneName)
    {
        LocalPlayerController.CanStartGame(SceneName);
    }
    // Change Team Button
    public void TeamChange(int TeamID)
    {
        LocalPlayerController.CanTeamJoin(TeamID);
    }
    // Leave Button
    public void LeaveGame()
    {
        LocalPlayerController.LeaveGame();
    }
}

}