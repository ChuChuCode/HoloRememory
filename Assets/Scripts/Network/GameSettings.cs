using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using HR.Network.Lobby;

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
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
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
}
}
