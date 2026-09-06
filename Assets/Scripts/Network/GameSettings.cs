using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using HR.Network.Lobby;
using HR.UI;

namespace HR.Network{
public class GameSettings : NetworkBehaviour
{
    public static GameSettings Instance;

    [SyncVar(hook = nameof(OnModeChanged))] public GameMode Mode = GameMode.OneLife;

    // Synced (not just host-local) so every client's map UI actually shows
    // what the host picked, not just the host's own screen.
    [SyncVar(hook = nameof(OnMapChanged))] public string MapName = "Game_Test_Scene";

    // TODO: fixed at 3:00 for now, not host-adjustable yet - same default on
    // every machine so it doesn't need syncing until it becomes a real setting.
    public float TimeLimit = 180f;

    [SerializeField] GameModeConfig[] configs;
    // Read-only - lets LobbyController generate mode buttons the same way
    // it generates character buttons from characterSelectComponentsList.
    public GameModeConfig[] Configs => configs;

    void Awake()
    {
        // TEMP diagnostic - remove once the Lobby map/mode display bug is found.
        Debug.Log($"[LobbyDebug] GameSettings.Awake fired, gameObject.activeSelf={gameObject.activeSelf}, time={Time.realtimeSinceStartup:F2}");
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    // Applies the mode picked on the Create Room screen (SteamLobby.HostLobby
    // stashes it here since GameSettings doesn't exist yet at that point -
    // it only spawns once Lobby_Scene loads). OnStartServer (not Awake) so
    // this only ever runs on the host that's actually creating the lobby,
    // never on a joining client's own copy of this object.
    public override void OnStartServer()
    {
        if (Network_Manager.PendingInitialMode.HasValue)
        {
            Mode = Network_Manager.PendingInitialMode.Value;
            Network_Manager.PendingInitialMode = null;
        }
    }
    // TEMP diagnostics - remove once the Lobby map/mode display bug is found.
    void OnEnable()
    {
        Debug.Log($"[LobbyDebug] GameSettings.OnEnable, time={Time.realtimeSinceStartup:F2}");
    }
    void OnDisable()
    {
        Debug.Log($"[LobbyDebug] GameSettings.OnDisable, time={Time.realtimeSinceStartup:F2}");
    }
    void OnDestroy()
    {
        Debug.Log($"[LobbyDebug] GameSettings.OnDestroy, time={Time.realtimeSinceStartup:F2}");
    }

    public GameModeConfig CurrentConfig()
    {
        foreach (GameModeConfig config in configs)
        {
            if (config.mode == Mode) return config;
        }
        Debug.LogWarning($"No GameModeConfig set up for {Mode}.");
        return null;
    }

    void OnModeChanged(GameMode oldMode, GameMode newMode)
    {
        if (SceneManager.GetActiveScene().name == "Lobby_Scene" && LobbyController.Instance != null)
        {
            LobbyController.Instance.UpdateModeText();
        }
        // Keep the room browser (anyone not yet joined) showing the current
        // mode too - only the host actually owns the Steam Lobby Data.
        if (NetworkServer.active && SteamLobby.Instance != null)
        {
            GameModeConfig config = CurrentConfig();
            SteamLobby.Instance.UpdateLobbyMode(config != null ? config.DisplayName : newMode.ToString());
        }
    }

    void OnMapChanged(string oldMap, string newMap)
    {
        if (SceneManager.GetActiveScene().name == "Lobby_Scene" && LobbyController.Instance != null)
        {
            LobbyController.Instance.UpdateMapText();
        }
        if (NetworkServer.active && SteamLobby.Instance != null)
        {
            SteamLobby.Instance.UpdateLobbyMap(newMap);
        }
    }

    // Called from the lobby UI - any connected client can set the room's
    // mode (same permission model as team/character selection already has).
    [Command(requiresAuthority = false)]
    public void CmdSetMode(GameMode mode)
    {
        Mode = mode;
    }

    // Map is host-only to pick (LobbyController.change_map gates on
    // NetworkServer.active before calling this), but synced so every
    // client's UI reflects the current pick.
    [Command(requiresAuthority = false)]
    public void CmdSetMap(string mapName)
    {
        MapName = mapName;
    }

    // Set by Network_Manager once, right after every character for a match
    // has spawned - NetworkTime.time (not Time.time), so every machine
    // agrees on the same instant regardless of local clock differences.
    //
    // A SyncVar instead of a ClientRpc deliberately - a fire-once RPC can
    // arrive before a given client (including the host's own client) has
    // actually finished loading the Game scene and re-created
    // MatchCountdownUI, silently doing nothing (same class of race as the
    // Lobby map/mode bug from earlier). A SyncVar is durable state: the
    // hook covers whoever's already watching when it changes, and
    // MatchCountdownUI.Start() separately pulls the current value itself
    // for whoever becomes ready after the fact - correct regardless of
    // which side initializes first.
    [SyncVar(hook = nameof(OnMatchCountdownChanged))] public double matchCountdownEndTime;

    void OnMatchCountdownChanged(double oldValue, double newValue)
    {
        ApplyMatchCountdown();
    }

    public void ApplyMatchCountdown()
    {
        float remaining = (float)(matchCountdownEndTime - NetworkTime.time);
        if (remaining > 0f) MatchCountdownUI.instance?.StartCountdown(remaining);
    }

    // Set true by Network_Manager.EndMatch() the instant a winner's
    // decided, reset back to false at the start of the next match (same
    // spawn point matchCountdownEndTime gets refreshed at) - otherwise this
    // would still read true the next time MatchCountdownUI.Start() pulls
    // it, showing FINISH again the moment the next match begins. Same
    // SyncVar-not-RPC reasoning as matchCountdownEndTime above.
    [SyncVar(hook = nameof(OnMatchFinishedChanged))] public bool matchFinished;

    void OnMatchFinishedChanged(bool oldValue, bool newValue)
    {
        ApplyMatchFinished();
    }

    public void ApplyMatchFinished()
    {
        if (matchFinished) MatchCountdownUI.instance?.ShowFinish();
    }
}
}
