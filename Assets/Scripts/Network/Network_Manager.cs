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
    // Same stash pattern as PendingInitialMode - set by SteamLobby.HostLobby()
    // from the Create Room screen's time limit picker, applied once by
    // GameSettings.OnStartServer() the moment it spawns in Lobby_Scene.
    public static float? PendingInitialTimeLimit;
    // Set by OnClientDisconnect below when the connection drops for a reason
    // we didn't choose (host crashed/quit, or we got kicked) - survives the
    // Main_Scene reload since it's static, same pattern as
    // ModeSelectPanel.ReturnToGameUI, so Main_Scene can route straight back
    // to the lobby browser instead of stranding the player on Title with no
    // explanation. There's no real host migration here - this is the
    // fallback for when the host disappears mid-session.
    public static bool ReturnToLobbyBrowser = false;
    int Player_num = 0;
    // Guards against CheckGameOver() (last team standing) and the match
    // timer (time's up) both trying to end the match at once.
    bool matchEnding = false;
    // Whole match-start window, countdown display included - MatchCountdownUI
    // shows this as "3, 2, 1" (one per second) then "Start" for the final
    // second (see its own startFlashDuration), so 4s here reads as 3
    // numbers + 1 Start beat, not a literal 4-count. Input/minion-wander/
    // SP-regen/match-timer-start all wait for the same total, so nothing
    // goes live before "Start" finishes showing.
    [SerializeField] float matchStartLockDuration = 4f;
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
        base.Awake();
    }
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        // A match already running has no spectator/late-join state to put a
        // fresh connection into - rather than leaving it connected with no
        // player object (silently stuck), reject it outright.
        if (SceneManager.GetActiveScene().name != "Lobby_Scene")
        {
            conn.Disconnect();
            return;
        }
        // print(PlayersInfoList.Count);
        // print(Player_num);
        // ******Spawn Twice when Rematch
        if (PlayersInfoList.Count != Player_num)
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
    // Fires on every remaining client when the connection drops for ANY
    // reason - voluntary leave (PlayerObject.LeaveGame), host crash/quit, or
    // getting kicked. A voluntary leave already set ReturnToGameUI right
    // before calling StopHost/StopClient; anything else reaching here wasn't
    // our choice, so route back to the lobby browser instead of the plain
    // Title screen.
    public override void OnClientDisconnect()
    {
        if (!ModeSelectPanel.ReturnToGameUI)
        {
            ReturnToLobbyBrowser = true;
        }
        base.OnClientDisconnect();
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
                    // Use gameplayInsance's OWN connectionToClient, not
                    // playerobject's copy of it - KeepAuthority (see the
                    // Lobby->Game swap below) never cleared gameplayInsance's
                    // ownership, so Mirror's ReplacePlayerForConnection
                    // rejects the swap ("already owned by a different
                    // connection") the instant these two references aren't
                    // the exact same NetworkConnectionToClient instance,
                    // which host-mode's local connection doesn't reliably
                    // stay across the scene change. Sourcing conn from the
                    // object we're restoring ownership to is always
                    // consistent by construction.
                    NetworkServer.ReplacePlayerForConnection(gameplayInsance.connectionToClient,gameplayInsance.gameObject,ReplacePlayerOptions.KeepAuthority);
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
                // Reset for this match's load-wait cycle - gameplayInsance's
                // own OnStartAuthority() (the owning client only) sets this
                // back to true once that client's load actually finishes;
                // see WaitForAllPlayersLoadedThenStartMatch below.
                player.MatchLoaded = false;
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
            // Reset from the last match - otherwise MatchCountdownUI would
            // still see it as true and flash FINISH again the instant this
            // new match starts.
            if (GameSettings.Instance != null) GameSettings.Instance.matchFinished = false;
            // Don't arm the countdown yet - everyone's isInputLocked stays
            // true (set above) until WaitForAllPlayersLoadedThenStartMatch
            // actually starts it, once every connected client's own load
            // finishes (or the timeout gives up waiting on a frozen one).
            StartCoroutine(WaitForAllPlayersLoadedThenStartMatch());
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
    // Server Change Scene - mapId is a MapConfig.MapId (see
    // GameSettings.MapName's comment), not necessarily a literal scene name:
    // a text-file map resolves to the shared Game_Map_Runtime scene, a
    // legacy baked map resolves to its own MapConfig.SceneName. Falls back to
    // treating mapId as a literal scene name directly (e.g. "Lobby_Scene",
    // which has no MapConfig at all) when no MapConfig matches.
    public void ChangeScene(string mapId)
    {
        MapConfig config = mapConfigList.Find(c => c.MapId == mapId);
        string sceneToLoad = config != null
            ? (config.MapFile != null ? "Game_Map_Runtime" : config.SceneName)
            : mapId;
        print($"Change Scene to : {sceneToLoad}");
        ServerChangeScene(sceneToLoad);
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
        // Flashes FINISH on MatchCountdownUI (see GameSettings.matchFinished).
        if (GameSettings.Instance != null) GameSettings.Instance.matchFinished = true;
        StartCoroutine(EndMatchAfterDelay());
    }
    IEnumerator EndMatchAfterDelay()
    {
        yield return new WaitForSeconds(5f);
        ChangeScene("Lobby_Scene");
    }
    // Gates the match-start countdown/timer on every connected player's own
    // client actually finishing its scene load and getting its character
    // (PlayerObject.MatchLoaded, set via CharacterBase.OnStartAuthority())
    // instead of arming it the instant the HOST's own scene change completes
    // server-side, which let a fast machine move/act while slower ones were
    // still loading. A disconnect mid-wait drops that connection's
    // PlayerObject out of PlayersInfoList on its own (OnStopClient), so it
    // can't stall this forever; the timeout is just a last-resort fallback
    // for a frozen/very slow client.
    IEnumerator WaitForAllPlayersLoadedThenStartMatch()
    {
        float elapsed = 0f;
        const float timeout = 20f;
        while (elapsed < timeout && PlayersInfoList.Exists(p => !p.MatchLoaded))
        {
            yield return null;
            elapsed += Time.deltaTime;
        }
        // Setting this SyncVar fires its hook immediately wherever it's set
        // (same as Mode/MapName already do), including on the host's own
        // client - no separate manual push needed.
        if (GameSettings.Instance != null)
        {
            GameSettings.Instance.matchCountdownEndTime = NetworkTime.time + matchStartLockDuration;
        }
        StartCoroutine(UnlockInputAfterMatchStart());
    }
    // Started once the load-wait above clears (see
    // WaitForAllPlayersLoadedThenStartMatch) - not per-character, since it
    // applies uniformly. Player_List is safe to read here even though it's
    // not populated inside the spawn loop itself: CharacterBase.Awake()
    // (which does the Player_List.Add) runs synchronously at Instantiate
    // time, well before any of this elapses.
    IEnumerator UnlockInputAfterMatchStart()
    {
        yield return new WaitForSeconds(matchStartLockDuration);
        foreach (CharacterBase character in Player_List)
        {
            if (character != null && !character.isDead) character.isInputLocked = false;
        }
        // Same reasoning as Player_List above - MinionSpawner.Start() (which
        // spawns them) runs well before this delay elapses, so Minion_List
        // is already fully populated by now.
        foreach (Minion minion in Minion_List)
        {
            if (minion != null) minion.BeginWandering();
        }
    }
}

}