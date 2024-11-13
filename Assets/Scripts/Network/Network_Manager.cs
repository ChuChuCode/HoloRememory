using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine.SceneManagement;
using UnityEngine;
using Steamworks;

public class Network_Manager : NetworkManager
{
    [Header("Lobby")]
    [SerializeField] PlayerObject PlayerObject_Prefab;
    public List<PlayerObject> PlayersInfoList {get;} = new List<PlayerObject>();
    [Header("Select")]
    [SerializeField] Network_SelectPlayer SelectPlayer;
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        if ( SceneManager.GetActiveScene().name == "Lobby_Scene") 
        {
            PlayerObject player = Instantiate(PlayerObject_Prefab);
             // Set connectID, PlayerID, SteamID
            // Start from 0
            player.ConnectionID = conn.connectionId;
            // Start frome 1
            player.PlayerIdNumber = PlayersInfoList.Count + 1;
            // Steam ID -> For player Info
            player.PlayerSteamID = (ulong)SteamMatchmaking.GetLobbyMemberByIndex((CSteamID)SteamLobby.Instance.CurrentLobbyID,PlayersInfoList.Count);
            // instantiating a "Player" prefab gives it the name "Player(clone)"
            // => appending the connectionId is WAY more useful for debugging!
            player.name = $"{player.name} [connId={conn.connectionId}]";
            NetworkServer.AddPlayerForConnection(conn, player.gameObject);

            // Add when PlayerObject in OnStartClient
            // PlayersInfoList.Add(player);
        }
    }
    public override void ServerChangeScene(string newSceneName)
    {
        base.ServerChangeScene(newSceneName);
        /*
        //Lobby to Select
        if (SceneManager.GetActiveScene().name == "Lobby_Scene" && newSceneName.StartsWith("Select_Scene"))
        {
            
            // NetworkClient.ClearSpawners();
            for (int i = 0 ; i < PlayersInfoList.Count ; i++)
            {
                var conn = PlayersInfoList[i].connectionToClient;
                GameObject oldPlayer = conn.identity.gameObject;
                Network_SelectPlayer gameplayInsance = Instantiate(SelectPlayer);

                gameplayInsance.PlayerName = PlayersInfoList[i].PlayerName;
                gameplayInsance.ConnectionID = PlayersInfoList[i].ConnectionID;
                gameplayInsance.PlayerSteamID = PlayersInfoList[i].PlayerSteamID;
                gameplayInsance.TeamID = PlayersInfoList[i].TeamID;
                NetworkServer.ReplacePlayerForConnection(conn,gameplayInsance.gameObject);
                // Add to Group
                SelectController.Instance.Add_TeamList(gameplayInsance);
                
                // NetworkServer.Spawn(gameplayInsance.gameObject);
                NetworkServer.Destroy(oldPlayer);
            }
            
        }
        */
        if (SceneManager.GetActiveScene().name == "Select_Scene" && newSceneName.StartsWith("Game_Scene"))
        {
            NetworkClient.ClearSpawners();
            for (int i = 0 ; i < PlayersInfoList.Count ; i++)
            {
                var conn = PlayersInfoList[i].connectionToClient;
                GameObject oldPlayer = conn.identity.gameObject;
                print(oldPlayer.name);
                // var gameplayInsance = Instantiate(InGamePlayer);

                // gameplayInsance.ConnectionID = PlayersInfoList[i].ConnectionID;
                // gameplayInsance.PlayerIdNumber = PlayersInfoList[i].PlayerIdNumber;
                // gameplayInsance.PlayerSteamID = PlayersInfoList[i].PlayerSteamID;
                
                // NetworkServer.ReplacePlayerForConnection(conn,gameplayInsance.gameObject);

                //NetworkServer.Destroy(oldPlayer);
                //Play_Room_Player_List.Add(gameplayInsance);
                //Play_Room_Player_List.Add(gameplayInsance.gameObject);
                //NetworkServer.Destroy(oldPlayer);
            }
        }
    }
    // Server Change Scene
    public void StartGame(string SceneName)
    {
        print("Change Scene");
        ServerChangeScene(SceneName);
    }
}
