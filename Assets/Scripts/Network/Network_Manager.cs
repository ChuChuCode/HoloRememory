using System.Collections.Generic;
using Mirror;
using UnityEngine.SceneManagement;
using UnityEngine;
using Steamworks;
using HR.UI;
using HR.Network.Select;
using HR.Network.Game;
using HR.Object.Player;
using HR.Network.Lobby;
using HR.Network.Result;

namespace HR.Network{
public class Network_Manager : NetworkManager
{
    [Header("Lobby")]
    [SerializeField] PlayerObject PlayerObject_Prefab;
    public List<PlayerObject> PlayersInfoList = new List<PlayerObject>();
    public PlayerObject LocalPlayerObject;
    [Header("Select")]
    [SerializeField] Network_SelectPlayer SelectPlayer;
    [Header("Character Component")]
    public List<CharacterSelectComponent> characterSelectComponentsList = new List<CharacterSelectComponent>();
    public List<CharacterBase> Player_List = new List<CharacterBase>();
    public int LoseTeam = 0;
    int Player_num = 0;
    public override void Start()
    {
        // Initial CharacterSelectComponent
        var playerObjects = Resources.LoadAll("Data/Character");
        foreach (var playerobject in playerObjects)
        {
            characterSelectComponentsList.Add(playerobject as CharacterSelectComponent);
        }
        base.Start();
    }
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        // print(PlayersInfoList.Count);
        // print(Player_num);
        // ******Spawn Twice when Rematch
        if ( SceneManager.GetActiveScene().name == "Lobby_Scene" && PlayersInfoList.Count != Player_num) 
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
    public override void OnServerConnect(NetworkConnectionToClient conn)
    {
        Player_num++;
    }
    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        base.OnServerDisconnect(conn);
        Player_num--;
    }
    public override void OnStopServer()
    {
        Player_num = 0;
    }
    public override void OnServerSceneChanged(string newSceneName)
    {
        // base.ServerChangeScene(newSceneName);
        /// Lobby Scene
        if (newSceneName.StartsWith("Lobby_Scene"))
        {
            if (!NetworkClient.ready)
            {
                NetworkClient.Ready();
            }
            // Delete all PlayerObject
            if (SceneManager.GetActiveScene().name == "Game_Scene")
            {
                foreach(CharacterBase playerobject in Player_List)
                {
                    PlayerObject gameplayInsance = PlayersInfoList.Find(player => player.ConnectionID == playerobject.ConnectionID);
                    NetworkServer.ReplacePlayerForConnection(playerobject.connectionToClient,gameplayInsance.gameObject,ReplacePlayerOptions.KeepAuthority);
                    // Delete CharacterBase
                    NetworkServer.Destroy(playerobject.gameObject);
                }
                Player_List.Clear();
            }
            foreach (PlayerObject player in PlayersInfoList)
            {
                player.Ready = false;
            }
            // Set LocalPlayer ** need to change to client
            LobbyController.Instance.LocalPlayerController = LocalPlayerObject;
            // Update UI
            LobbyController.Instance.UpdatePlayerList();
            LoseTeam = 0 ;
        }
        // Lobby to Select 
        if (newSceneName.StartsWith("Select_Scene"))
        {
            foreach (PlayerObject player in PlayersInfoList)
            {
                // Reset all parameter
                player.Reset_All();
                // player.Ready = false;
            }
        }
        /// Game Scene
        if (newSceneName.StartsWith("Game_Scene"))
        {
            foreach (PlayerObject player in PlayersInfoList)
            {
                NetworkConnectionToClient conn = player.connectionToClient;
                // GameObject oldPlayer = conn.identity.gameObject;
                // Spawn Prefab
                CharacterSelectComponent characterModelComponent = characterSelectComponentsList.Find(component => component.ID == player.CharacterID);
                CharacterBase characterModel = characterModelComponent.CharacterModel;
                CharacterBase gameplayInsance;
                // Set Layer
                int LayerIgnoreRaycast = LayerMask.NameToLayer("Team" + player.TeamID.ToString());
                if (player.TeamID == 1)
                {
                    gameplayInsance = Instantiate(characterModel,GameController.Instance.Team1_transform.position,Quaternion.identity);
                }
                else
                {
                    gameplayInsance = Instantiate(characterModel,GameController.Instance.Team2_transform.position,Quaternion.identity);
                }
                // Set Layer to all child
                Transform[] children = gameplayInsance.GetComponentsInChildren<Transform>(includeInactive: true);
                foreach(Transform child in children)
                {
                    child.gameObject.layer = LayerIgnoreRaycast;
                }
                
                gameplayInsance.ConnectionID = player.ConnectionID;
                gameplayInsance.PlayerIdNumber = player.PlayerIdNumber;
                gameplayInsance.PlayerSteamID = player.PlayerSteamID;
                gameplayInsance.TeamID = player.TeamID;
                gameplayInsance.CharacterID = player.CharacterID;
                gameplayInsance.PlayerName = player.PlayerName;
                // NetworkServer.Destroy(oldPlayer);

                gameplayInsance.Spells[0] = SelectController.Instance.Search_Spell(player.Spell_1);
                gameplayInsance.Spells[1] = SelectController.Instance.Search_Spell(player.Spell_2);
                
                // Ensure the client is ready before replacing the player
                if (!NetworkClient.ready)
                {
                    NetworkClient.Ready();
                }
                // NetworkServer.Spawn(gameplayInsance.gameObject);
                NetworkServer.ReplacePlayerForConnection(conn,gameplayInsance.gameObject,ReplacePlayerOptions.KeepAuthority);

                Player_List.Add(gameplayInsance);
                // All Player Info *** need to change to client
                CharacterInfoPanel.Instance.Add_to_Info(characterModelComponent.CharacterImage,gameplayInsance.gameObject);
            }
        }
        if (newSceneName.StartsWith("Result_Scene"))
        {
            // int LocalPlayerTeamID = 0;
            // Delete all PlayerObject
            foreach(CharacterBase playerobject in Player_List)
            {
                PlayerObject player = PlayersInfoList.Find(player => player.ConnectionID == playerobject.ConnectionID);
                // ResultController.Instance.Spawn_Result_Prefab(characterModelComponent.CharacterImage,playerobject);

                // Check Team1 or Team2
                // if (playerobject.GetComponent<NetworkIdentity>().isLocalPlayer)
                // {
                //     LocalPlayerTeamID = playerobject.TeamID;
                // }
                // Change back to PlayerObject
                // Ensure the client is ready before replacing the player
                if (!NetworkClient.ready)
                {
                    NetworkClient.Ready();
                }
                NetworkServer.ReplacePlayerForConnection(playerobject.connectionToClient,player.gameObject,ReplacePlayerOptions.KeepAuthority);

                // Set CharacterBase Info to Result_Player
                player.CanKDAChange(playerobject.kill, playerobject.death,playerobject.assist);
                // Destroy CharacterBase
                NetworkServer.Destroy(playerobject.gameObject);
            }
            Player_List.Clear();
            // *** need to change to client
            // ResultController.Instance.Show_Result(LoseTeam,LocalPlayerTeamID);
        }
        
    }
    // Server Change Scene
    public void ChangeScene(string SceneName)
    {
        print($"Change Scene to : {SceneName}");
        ServerChangeScene(SceneName);
    }
}

}