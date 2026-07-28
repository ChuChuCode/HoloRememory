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
using HR.Map;

namespace HR.Network{
public class Network_Manager : NetworkManager
{
    [Header("Lobby")]
    [SerializeField] PlayerObject PlayerObject_Prefab;
    public const int MaxPlayers = 8; // Number of selectable team colors (1..MaxPlayers) - teams can hold any number of players
    public List<PlayerObject> PlayersInfoList = new List<PlayerObject>();
    public PlayerObject LocalPlayerObject;
    [Header("Character Component")]
    public List<CharacterSelectComponent> characterSelectComponentsList = new List<CharacterSelectComponent>();
    public List<CharacterBase> Player_List = new List<CharacterBase>();
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
            // Auto-spread new joiners across the 8 colors by default; teams
            // can be uneven and players can freely re-pick afterward (see
            // PlayerObject.CanTeamJoin), including matching someone else's
            // color - asymmetric team sizes are allowed.
            player.TeamID = NextAutoTeam();
            NetworkServer.AddPlayerForConnection(conn, player.gameObject);

            // Add when PlayerObject in OnStartClient
            // PlayersInfoList.Add(player);
        }
    }
    // Prefer a color nobody currently in the room is using, so a fresh
    // joiner doesn't land on the same color as an existing player by luck
    // of the count (players are free to re-pick, so a plain round robin by
    // count can drift onto an already-occupied color).
    int NextAutoTeam()
    {
        HashSet<int> usedTeams = new HashSet<int>();
        foreach (PlayerObject player in PlayersInfoList)
        {
            usedTeams.Add(player.TeamID);
        }
        for (int team = 1; team <= MaxPlayers; team++)
        {
            if (!usedTeams.Contains(team)) return team;
        }
        // All colors already taken - fall back to round robin (best effort share).
        return (PlayersInfoList.Count % MaxPlayers) + 1;
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
                player.CharacterID = -1;
            }
            // Set LocalPlayer ** need to change to client
            LobbyController.Instance.LocalPlayerController = LocalPlayerObject;
            // Update UI
            LobbyController.Instance.UpdatePlayerList();
        }
        /// Game Scene
        if (newSceneName.StartsWith("Game") )
        {
            // Teams can be asymmetric (any number of players per color), so
            // track how many of each team we've placed already and hand out
            // that team's spawn points in order (wraps via modulo in
            // GetSpawnPosition if a team has more players than spawn points).
            Dictionary<int, int> teamSpawnIndex = new Dictionary<int, int>();
            foreach (PlayerObject player in PlayersInfoList)
            {
                NetworkConnectionToClient conn = player.connectionToClient;
                // GameObject oldPlayer = conn.identity.gameObject;
                // Spawn Prefab
                CharacterSelectComponent characterModelComponent = characterSelectComponentsList.Find(component => component.ID == player.CharacterID);
                CharacterBase characterModel = characterModelComponent.CharacterModel;
                CharacterBase gameplayInsance;

                teamSpawnIndex.TryGetValue(player.TeamID, out int spawnIndex);
                teamSpawnIndex[player.TeamID] = spawnIndex + 1;
                Vector3 SpawnPosition = GridManager.Instance.GetSpawnPosition(player.TeamID, spawnIndex);
                // Face toward the arena center
                Quaternion rotation = Quaternion.LookRotation(Vector3.zero - SpawnPosition);
                gameplayInsance = Instantiate(characterModel,SpawnPosition,rotation);
                // // Set Layer to all child
                // Transform[] children = gameplayInsance.GetComponentsInChildren<Transform>(includeInactive: true);
                // foreach(Transform child in children)
                // {
                //     child.gameObject.layer = PlayerLayer;
                // }
                
                gameplayInsance.ConnectionID = player.ConnectionID;
                gameplayInsance.PlayerIdNumber = player.PlayerIdNumber;
                gameplayInsance.PlayerSteamID = player.PlayerSteamID;
                gameplayInsance.TeamID = player.TeamID;
                gameplayInsance.CharacterID = player.CharacterID;
                gameplayInsance.PlayerName = player.PlayerName;
                // NetworkServer.Destroy(oldPlayer);
                
                // Ensure the client is ready before replacing the player
                if (!NetworkClient.ready)
                {
                    NetworkClient.Ready();
                }
                // NetworkServer.Spawn(gameplayInsance.gameObject);
                NetworkServer.ReplacePlayerForConnection(conn,gameplayInsance.gameObject,ReplacePlayerOptions.KeepAuthority);
                // OneLife/MultiLife force everyone to the same maxHealth (1
                // hit kill, regardless of character). HealthBar leaves each
                // character's own prefab maxHealth alone, so per-character
                // tankier/frailer differences still apply.
                GameModeConfig modeConfig = GameSettings.Instance != null ? GameSettings.Instance.CurrentConfig() : null;
                if (modeConfig != null)
                {
                    if (modeConfig.overrideHealth) gameplayInsance.maxHealth = modeConfig.maxHealth;
                    gameplayInsance.lives = modeConfig.lives;
                }
                // Now actually spawned/server-authoritative, so isServer reads
                // correctly - sets currentHealth = maxHealth instead of
                // relying on the SyncVar's hardcoded default of 1.
                gameplayInsance.InitialHealth();

                // Player_List.Add(gameplayInsance);
                // All Player Info *** need to change to client
                // CharacterInfoPanel.Instance.RpcAdd_to_Info(characterModelComponent.CharacterImage,gameplayInsance.gameObject);
            }
            
        }
        if (newSceneName.StartsWith("Result_Scene"))
        {
            // int LocalPlayerTeamID = 0;
            // Delete all PlayerObject
            foreach(PlayerObject player in PlayersInfoList)
            {
                CharacterBase playerobject = Player_List.Find(playerobject => player.ConnectionID == playerobject.ConnectionID);
                // Change back to PlayerObject
                // Ensure the client is ready before replacing the player
                if (!NetworkClient.ready)
                {
                    NetworkClient.Ready();
                }
                NetworkServer.ReplacePlayerForConnection(playerobject.connectionToClient,player.gameObject,ReplacePlayerOptions.KeepAuthority);

                // Destroy CharacterBase
                NetworkServer.Destroy(playerobject.gameObject);
            }
            Player_List.Clear();
        }
    }
    public override void OnClientSceneChanged()
    {
        base.OnClientSceneChanged();
        if (SceneManager.GetActiveScene().name == "Result_Scene")
        {
            // Delete all PlayerObject
            foreach(CharacterBase playerobject in Player_List)
            {
                // Destroy CharacterBase
                Destroy(playerobject.gameObject);
            }
            Player_List.Clear();
        }
    }
    // Server Change Scene
    public void ChangeScene(string SceneName)
    {
        print($"Change Scene to : {SceneName}");
        ServerChangeScene(SceneName);
    }
    // Called (server-only) whenever a player dies - FFA, so TeamID is really
    // just each player's unique slot; this ends the game once at most one
    // player is still alive.
    public void CheckGameOver()
    {
        HashSet<int> alivePlayers = new HashSet<int>();
        foreach (CharacterBase player in Player_List)
        {
            if (!player.isDead) alivePlayers.Add(player.TeamID);
        }

        if (alivePlayers.Count <= 1)
        {
            ChangeScene("Lobby_Scene");
        }
    }
}

}