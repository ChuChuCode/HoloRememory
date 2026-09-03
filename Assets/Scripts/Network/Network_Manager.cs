using System.Collections;
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
using HR.Object.Minions;

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
    [Header("Map Component")]
    public List<MapConfig> mapConfigList = new List<MapConfig>();
    public List<CharacterBase> Player_List = new List<CharacterBase>();
    // Populated/cleaned up by Minion itself (Awake/OnDestroy) - same
    // per-machine local list pattern as Player_List, read by
    // ExplosionSegment to damage mobs the same way it damages players.
    public List<Minion> Minion_List = new List<Minion>();
    // Set by SteamLobby.HostLobby() from the Create Room screen's mode
    // picker, before GameSettings exists (it only spawns once Lobby_Scene
    // loads) - GameSettings.OnStartServer applies and clears this once it
    // does. Null on every machine that isn't actively creating a lobby right
    // now, including this same host for every match after the first.
    public static GameMode? PendingInitialMode;
    int Player_num = 0;
    // Guards against CheckGameOver() (last team standing) and the match
    // timer (time's up) both trying to end the match at once.
    bool matchEnding = false;
    [SerializeField] float matchStartLockDuration = 3f;
    // Was in Start() - but LobbyController.Start() (a different GameObject)
    // reads characterSelectComponentsList/mapConfigList too, and Unity only
    // guarantees ALL Awakes run before ANY Starts, not any ordering between
    // different objects' own Start() calls. In the Editor this apparently
    // always resolved in our favor by luck of load order; in an actual
    // build it didn't, so the Lobby's map/mode carousel could still be
    // sitting on its placeholder text when LobbyController.Start() ran.
    public override void Awake()
    {
        // Initial CharacterSelectComponent
        var playerObjects = Resources.LoadAll("Data/Character");
        foreach (var playerobject in playerObjects)
        {
            characterSelectComponentsList.Add(playerobject as CharacterSelectComponent);
        }
        // Initial MapConfig
        var mapObjects = Resources.LoadAll("Data/Map");
        foreach (var mapObject in mapObjects)
        {
            mapConfigList.Add(mapObject as MapConfig);
        }
        // TEMP diagnostic - remove once the Lobby map/mode display bug is found.
        Debug.Log($"[LobbyDebug] Network_Manager.Awake: loaded {characterSelectComponentsList.Count} characters, {mapConfigList.Count} maps, time={Time.realtimeSinceStartup:F2}");
        base.Awake();
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
            // Steam ID -> For player Info. SteamLobby.Instance is null under
            // the KCP fallback (no Steam running) - PlayerSteamID just stays
            // 0 in that case, there's no Steam lobby membership to look up.
            if (SteamLobby.Instance != null)
            {
                player.PlayerSteamID = (ulong)SteamMatchmaking.GetLobbyMemberByIndex((CSteamID)SteamLobby.Instance.CurrentLobbyID,PlayersInfoList.Count);
            }
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
        // Find the mid-match character for this connection (if any) before
        // the base call destroys it (ReplacePlayerForConnection made this
        // the connection's actual player object when the match started, so
        // Mirror's own disconnect handling already tears it down - nothing
        // currently removes the now-dead reference from Player_List though,
        // which would crash the next CheckGameOver() call).
        CharacterBase disconnectedCharacter = Player_List.Find(p => p != null && p.ConnectionID == conn.connectionId);
        
        // TODO(disconnect self-destruct): notify everyone before the object
        // is torn down below - this is the hook a future disconnect
        // "self-destruct" animation plays from (see
        // CharacterBase.OnDisconnect), not wired to anything yet. Once that
        // exists, the NetworkServer.Destroy() below needs to be deferred
        // until the animation finishes instead of firing immediately via
        // base.OnServerDisconnect().
        disconnectedCharacter?.RpcOnDisconnect();

        base.OnServerDisconnect(conn);
        Player_num--;

        if (disconnectedCharacter != null)
        {
            Player_List.Remove(disconnectedCharacter);
            // A remaining player leaving mid-match should end the match
            // once at most one player is left, same as if they'd been
            // eliminated. If the HOST leaves instead, StopHost() (already
            // wired to the leave button) tears down the whole server for
            // everyone before this would even run.
            CheckGameOver();
        }
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
            // Delete all PlayerObject - Player_List is only ever populated
            // while a match is running (see the "Game" branch below), so a
            // non-empty list here means we're returning FROM one, regardless
            // of which specific map scene it was. (Previously checked
            // SceneManager.GetActiveScene().name == "Game_Scene", but no
            // scene is ever actually named that - real matches use whatever
            // GameSettings.MapName/MapConfig.SceneName says, e.g.
            // "Game_Test_Scene" - and by the time OnServerSceneChanged runs,
            // the active scene is already newSceneName (Lobby_Scene) anyway.
            // This whole block was dead code: old match characters were
            // never destroyed or swapped back to PlayerObject, so they kept
            // running via DontDestroyOnLoad with all their end-of-match state
            // (health, bombAmount, isHoldingBomb, mount, debuffs...) straight
            // into the next match, and their still-live input bindings could
            // fire (e.g. auto-placing a bomb the instant a new life spawned).
            if (Player_List.Count > 0)
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
                // Ready still resets - everyone has to re-confirm before the
                // next match starts - but CharacterID carries over so each
                // player's already-picked character stays selected/shown in
                // the lobby instead of forcing a re-pick every round.
                player.Ready = false;
            }
            // Set LocalPlayer ** need to change to client
            LobbyController.Instance.LocalPlayerController = LocalPlayerObject;
            // Update UI
            LobbyController.Instance.UpdatePlayerList();
        }
        // TODO(loading screen): not implemented yet - right now whoever's
        // client finishes loading this scene first can immediately see/move
        // while slower clients are still loading. Planned approach (avoids
        // relying on Mirror's own scene-load AsyncOperation progress, which
        // is fiddly/version-dependent to read):
        //   1. Add [SyncVar] float LoadProgress to PlayerObject, same
        //      broadcast pattern as Ready/CharacterID.
        //   2. Each client shows a loading screen the instant the scene
        //      change starts, jumps LoadProgress to a fixed "started
        //      loading" milestone (e.g. 20-30%) for visual feedback, then
        //      Cmds LoadProgress = 100 once ITS OWN OnClientSceneChanged()
        //      fires (scene + character actually ready on that machine).
        //   3. Loading screen UI reads every PlayersInfoList entry's
        //      LoadProgress (same as the Lobby player list already does) so
        //      everyone can see who's still loading.
        //   4. Server watches for all players hitting 100 (check on each
        //      LoadProgress hook), then runs a 5s countdown coroutine before
        //      RpcAll-ing "match officially starts" (hide loading screen,
        //      unlock input, start the match timer).
        //   5. A disconnect mid-load has to drop out of the "everyone" check
        //      (mirror the existing OnServerDisconnect cleanup), or the
        //      count never reaches 100%.
        //   6. Characters currently spawn immediately in the block below the
        //      instant the scene change completes server-side - to actually
        //      gate on step 4 instead of just cosmetically showing a loading
        //      screen, spawned characters need to stay input-locked until
        //      the "match officially starts" signal arrives.
        /// Game Scene
        if (newSceneName.StartsWith("Game") )
        {
            matchEnding = false;
            // Spawn points are no longer scoped by team/color (a color isn't
            // a strict side of the map anymore) - everyone draws from the
            // same random pool, tracked here so nobody doubles up.
            HashSet<Vector2Int> usedSpawnCoords = new HashSet<Vector2Int>();
            foreach (PlayerObject player in PlayersInfoList)
            {
                NetworkConnectionToClient conn = player.connectionToClient;
                // GameObject oldPlayer = conn.identity.gameObject;
                // Spawn Prefab
                CharacterSelectComponent characterModelComponent = characterSelectComponentsList.Find(component => component.ID == player.CharacterID);
                CharacterBase characterModel = characterModelComponent.CharacterModel;
                CharacterBase gameplayInsance;

                Vector3 SpawnPosition = GridManager.Instance.GetRandomUnusedSpawnPosition(usedSpawnCoords);
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
                // Defaults true on the prefab already - set explicitly here
                // too so intent isn't just an implicit field default.
                gameplayInsance.isInputLocked = true;

                // Player_List.Add(gameplayInsance);
                // All Player Info *** need to change to client
                // CharacterInfoPanel.Instance.RpcAdd_to_Info(characterModelComponent.CharacterImage,gameplayInsance.gameObject);
            }
            StartCoroutine(UnlockInputAfterMatchStart());
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
            EndMatch();
        }
    }
    // Called by CheckGameOver() (last one standing) or LocalPlayerInfo (time
    // ran out - a draw). Either way, pause 5s before returning to the lobby
    // so the ending actually registers, even without a win/loss screen yet.
    public void EndMatch()
    {
        if (matchEnding) return;
        matchEnding = true;
        // Result's decided - nobody should be able to keep moving/bombing/
        // using skills during the 5s pause before the scene changes.
        foreach (CharacterBase character in Player_List)
        {
            if (character != null) character.isInputLocked = true;
        }
        StartCoroutine(EndMatchAfterDelay());
    }
    IEnumerator EndMatchAfterDelay()
    {
        yield return new WaitForSeconds(5f);
        ChangeScene("Lobby_Scene");
    }
    // Started once, right after every character for this match has spawned
    // (see the "Game" scene branch above) - not per-character, since it
    // applies uniformly. Player_List is safe to read here even though it's
    // not populated inside that spawn loop itself: CharacterBase.Awake()
    // (which does the Player_List.Add) runs synchronously at Instantiate
    // time, well before this delay elapses.
    IEnumerator UnlockInputAfterMatchStart()
    {
        yield return new WaitForSeconds(matchStartLockDuration);
        foreach (CharacterBase character in Player_List)
        {
            if (character != null && !character.isDead) character.isInputLocked = false;
        }
    }
}

}