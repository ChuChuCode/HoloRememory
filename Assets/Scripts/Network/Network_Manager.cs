using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine.SceneManagement;
using UnityEngine;
using Steamworks;
using System.Linq;

public class Network_Manager : NetworkManager
{
    [Header("Lobby")]
    [SerializeField] PlayerObject PlayerObject_Prefab;
    public List<PlayerObject> PlayersInfoList = new List<PlayerObject>();
    [Header("Select")]
    [SerializeField] Network_SelectPlayer SelectPlayer;
    [Header("Character Component")]
    public List<CharacterSelectComponent> characterSelectComponentsList = new List<CharacterSelectComponent>();
    public List<GameObject> Player_List = new List<GameObject>();
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
    public override void OnServerSceneChanged(string newSceneName)
    {
        // base.ServerChangeScene(newSceneName);
        if (newSceneName.StartsWith("Game_Scene"))
        {
            foreach (PlayerObject player in PlayersInfoList)
            {
                NetworkConnectionToClient conn = player.connectionToClient;
                GameObject oldPlayer = conn.identity.gameObject;
                // Spawn Prefab
                CharacterSelectComponent characterModelComponent = characterSelectComponentsList.Find(component => component.ID == player.CharacterID);
                GameObject characterModel = characterModelComponent.CharacterModel;
                GameObject gameplayInsance;
                // Set Layer
                int LayerIgnoreRaycast = LayerMask.NameToLayer("Team" + player.TeamID);
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
                
                // gameplayInsance.ConnectionID = PlayersInfoList[i].ConnectionID;
                // gameplayInsance.PlayerIdNumber = PlayersInfoList[i].PlayerIdNumber;
                // gameplayInsance.PlayerSteamID = PlayersInfoList[i].PlayerSteamID;
                // NetworkServer.Destroy(oldPlayer);
                // Set UI
                if (player.netIdentity.isOwned)
                {
                    MianInfoUI.instance.Character_Image.sprite = characterModelComponent.CharacterImage;
                    MianInfoUI.instance.Q.Set_Skill_Icon(characterModelComponent.Q_skill_Image);
                    MianInfoUI.instance.W.Set_Skill_Icon(characterModelComponent.W_skill_Image);
                    MianInfoUI.instance.E.Set_Skill_Icon(characterModelComponent.E_skill_Image);
                    MianInfoUI.instance.R.Set_Skill_Icon(characterModelComponent.R_skill_Image);
                }
                NetworkServer.ReplacePlayerForConnection(conn,gameplayInsance);

                Player_List.Add(gameplayInsance);
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
