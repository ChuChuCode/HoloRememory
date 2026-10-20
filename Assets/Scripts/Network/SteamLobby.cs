using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using Mirror;
using Steamworks;
using System;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using HR.Network.Lobby;

namespace HR.Network{
public class SteamLobby : MonoBehaviour
{
    public static SteamLobby Instance;

    [Header("Lobby Create Callbacks")]
    protected Callback<LobbyCreated_t> lobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequested;
    protected Callback<LobbyEnter_t> lobbyEntered;
    [Header("Lobby Join Callbacks")]
    protected Callback<LobbyMatchList_t> lobbyList;
    protected Callback<LobbyDataUpdate_t> lobbyDataUpdated;
    public List<CSteamID> lobbyIDs = new List<CSteamID>();
    [Header("variables")]
    public ulong CurrentLobbyID;
    const string HostAddressKey = "HostAddress";
    const string UITableName = "UI";
    // Published as Steam Lobby Data so the room *browser* (not yet joined,
    // so no Mirror/GameSettings sync) can still show map/mode per room.
    public const string MapKey = "Map";
    public const string ModeKey = "Mode";
    // Stores a SHA256 hash, never the raw password - Steam Lobby Data is
    // publicly readable via GetLobbyData by anyone, joined or not, so a
    // plaintext value here would be trivially visible to anyone who asks.
    public const string PasswordKey = "Password";
    [SerializeField] Button HostButton;
    // Create Room screen fields - all optional, all read at the moment
    // HostLobby()/OnLobbyCreated() actually run, so leaving any of them
    // unassigned just falls back to the old hardcoded behavior.
    [SerializeField] TMP_InputField HostRoomNameInput;
    // Leave unassigned (or empty) to host a public room with no password.
    [SerializeField] TMP_InputField HostPasswordInput;
    // Dropdown option order must be Public, Friends Only, Invite Only.
    [SerializeField] TMP_Dropdown HostVisibilityDropdown;
    // Dropdown option order must match modeOptions index-for-index -
    // GameSettings doesn't exist yet at Create Room time (only spawns once
    // Lobby_Scene loads), so this is its own separate reference to the same
    // GameModeConfig assets, not read from GameSettings.Configs.
    [SerializeField] TMP_Dropdown HostModeDropdown;
    [SerializeField] GameModeConfig[] modeOptions;
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

    void OnEnable()
    {
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
    }
    void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
    }
    // These two dropdowns are plain option lists built from code (not
    // LocalizeStringEvent, which only tracks a single fixed string), so
    // without this they'd only ever pick up a language change the NEXT
    // time the Create Room panel happens to be reopened. Also the fix for
    // the very first population below racing Language_Component's own
    // async locale setup at startup - if that hasn't finished yet when
    // Start() runs, this fires again the moment it actually does, instead
    // of leaving English baked in until something else refreshes it.
    void OnLocaleChanged(Locale locale)
    {
        PopulateModeDropdown();
        PopulateVisibilityDropdown();
        RefreshDefaultRoomName();
    }
    // Tracks our own last auto-filled value so a locale change can refresh
    // it safely - RefreshDefaultRoomName below only overwrites the field if
    // it still holds exactly this, never if the player's actually typed
    // their own room name over it.
    string lastAutoFilledRoomName;
    void Start()
    {
        // Best-effort - see RefreshCreateRoomDefaults for why this alone
        // isn't reliable if the Create Room panel starts inactive.
        RefreshCreateRoomDefaults();

        // networkManager = Network_Manager.singleton as Network_Manager;
        // If initialized failed
        if (!SteamManager.Initialized) return;
        if (Instance == null)
        {
            Instance = this;
        }

        lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
        lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
        lobbyList = Callback<LobbyMatchList_t>.Create(OnGetLobbyList);
        lobbyDataUpdated = Callback<LobbyDataUpdate_t>.Create(OnGetLobbyData);
        
    }
    // Called once from Start() as a best-effort, and again every time the
    // Create Room panel is opened (CreateRoomPanel.OnEnable) - Start() runs
    // once, early, quite possibly before this panel/its dropdowns have ever
    // been active, so populating only there can silently do nothing if the
    // panel starts as SetActive(false).
    public void RefreshCreateRoomDefaults()
    {
        PopulateModeDropdown();
        PopulateVisibilityDropdown();
        RefreshDefaultRoomName();
    }
    // Only overwrites the field if it's empty (fresh open - nothing typed
    // yet) or still holds exactly what WE put there last time (a locale
    // change since then) - never touches an actual player-typed room name.
    void RefreshDefaultRoomName()
    {
        if (HostRoomNameInput == null) return;
        if (!string.IsNullOrEmpty(HostRoomNameInput.text) && HostRoomNameInput.text != lastAutoFilledRoomName) return;

        string playerName = SteamManager.Initialized
            ? SteamFriends.GetPersonaName()
            : LocalizationSettings.StringDatabase.GetLocalizedString(UITableName, "generic_player_name");
        // "{0}'s Lobby" as a format template, not string-concatenation -
        // where the possessive goes relative to the name isn't the same in
        // every language (see the Key's own comment in the CSV). Wrapped in
        // a List<object> (not passed bare) to land on the IList<object>
        // overload - passed positionally as a lone string, the compiler
        // binds it to the optional Locale parameter instead of the
        // trailing params arguments.
        lastAutoFilledRoomName = LocalizationSettings.StringDatabase.GetLocalizedString(UITableName, "create_room_default_name", new List<object> { playerName });
        HostRoomNameInput.text = lastAutoFilledRoomName;
    }
    // config.DisplayName itself is no longer read here - the Key depends on
    // config.mode instead, so every GameModeConfig shows a real translation
    // (from the UI table) rather than whatever freeform text happens to be
    // typed into that field. Re-populated every time this is called (see
    // RefreshCreateRoomDefaults), so it naturally follows whatever the
    // CURRENT language is - unlike Language_Component.GetNativeName, this
    // one SHOULD translate into the active locale, not stay fixed.
    void PopulateModeDropdown()
    {
        if (HostModeDropdown == null || modeOptions == null) return;
        HostModeDropdown.ClearOptions();
        List<string> labels = new List<string>();
        foreach (GameModeConfig config in modeOptions)
        {
            labels.Add(config != null ? GetModeDisplayName(config.mode) : "???");
        }
        HostModeDropdown.AddOptions(labels);
    }
    public static string GetModeDisplayName(GameMode mode)
    {
        string key = mode switch
        {
            GameMode.OneLife => "game_mode_one_life",
            GameMode.MultiLife => "game_mode_multi_life",
            GameMode.HealthBar => "game_mode_health_mode",
            _ => null
        };
        return key != null
            ? LocalizationSettings.StringDatabase.GetLocalizedString(UITableName, key)
            : mode.ToString();
    }
    // Hardcoded, not data-driven like modeOptions - there's no asset behind
    // "visibility", it's always exactly these 3 Steam lobby types.
    void PopulateVisibilityDropdown()
    {
        if (HostVisibilityDropdown == null) return;
        HostVisibilityDropdown.ClearOptions();
        HostVisibilityDropdown.AddOptions(new List<string> {
            LocalizationSettings.StringDatabase.GetLocalizedString(UITableName, "room_visual_public"),
            LocalizationSettings.StringDatabase.GetLocalizedString(UITableName, "room_visual_friends_only"),
            LocalizationSettings.StringDatabase.GetLocalizedString(UITableName, "room_visual_invite_only"),
        });
    }
    // Called by the Create Room screen's own Create button, not the
    // Multiplayer screen's Host button directly anymore - that button now
    // just opens the Create Room screen so its fields are on-screen first.
    public void HostLobby()
    {
        print("Host Button Press");
        // Disable host button
        HostButton.interactable = false;

        ELobbyType visibility = ELobbyType.k_ELobbyTypePublic;
        if (HostVisibilityDropdown != null)
        {
            switch (HostVisibilityDropdown.value)
            {
                case 1: visibility = ELobbyType.k_ELobbyTypeFriendsOnly; break;
                case 2: visibility = ELobbyType.k_ELobbyTypePrivate; break;
                default: visibility = ELobbyType.k_ELobbyTypePublic; break;
            }
        }

        // Stashed for GameSettings.OnStartServer to pick up once it spawns
        // in Lobby_Scene - it doesn't exist yet at this point.
        if (HostModeDropdown != null && modeOptions != null && HostModeDropdown.value < modeOptions.Length)
        {
            Network_Manager.PendingInitialMode = modeOptions[HostModeDropdown.value].mode;
        }

        SteamMatchmaking.CreateLobby(visibility, Manager.maxConnections);
    }
    // Quick Match Button - reuses the same list request as the room
    // browser, but instead of displaying results it auto-joins the first
    // non-full lobby found, or hosts a new one if none qualify.
    bool isQuickMatching = false;
    public void QuickMatch()
    {
        isQuickMatching = true;
        GetLobbyList();
    }
    void OnLobbyCreated(LobbyCreated_t callback)
    {
        print("Lobby Create");
        // If Lobby Create Error -> Show Host button
        if (callback.m_eResult != EResult.k_EResultOK)
        {
            // TEMP diagnostic - remove once the Create-stuck bug is found.
            Debug.LogError($"[CreateRoomDebug] OnLobbyCreated failed: m_eResult={callback.m_eResult}");
            HostButton.interactable = true;
            return;
        }
        Manager.StartHost();

        SteamMatchmaking.SetLobbyData(
            new CSteamID(callback.m_ulSteamIDLobby),
            HostAddressKey, SteamUser.GetSteamID().ToString()
        );

        string roomName = HostRoomNameInput != null && !string.IsNullOrEmpty(HostRoomNameInput.text)
            ? HostRoomNameInput.text
            : SteamFriends.GetPersonaName().ToString() + "'s LOBBY";
        SteamMatchmaking.SetLobbyData(
            new CSteamID(callback.m_ulSteamIDLobby),
            "name",
            roomName
        );

        // Only set at all when a password was actually typed - an unset key
        // reads back as "" via GetLobbyData, which is exactly how the room
        // list/join flow tells "no password" apart from "has one".
        string hostPassword = HostPasswordInput != null ? HostPasswordInput.text : "";
        if (!string.IsNullOrEmpty(hostPassword))
        {
            SteamMatchmaking.SetLobbyData(
                new CSteamID(callback.m_ulSteamIDLobby),
                PasswordKey, HashPassword(hostPassword)
            );
        }

        // Seed Map/Mode immediately so the room browser has something to
        // show even before the host touches either setting.
        CurrentLobbyID = callback.m_ulSteamIDLobby;
        if (GameSettings.Instance != null)
        {
            UpdateLobbyMap(GameSettings.Instance.MapName);
            GameModeConfig config = GameSettings.Instance.CurrentConfig();
            UpdateLobbyMode(config != null ? config.DisplayName : GameSettings.Instance.Mode.ToString());
        }
    }
    // Called by GameSettings whenever the host changes map/mode, so the
    // room browser (anyone not yet joined) reflects the current pick too.
    public void UpdateLobbyMap(string mapName)
    {
        if (!NetworkServer.active || CurrentLobbyID == 0) return;
        SteamMatchmaking.SetLobbyData(new CSteamID(CurrentLobbyID), MapKey, mapName);
    }
    public void UpdateLobbyMode(string modeName)
    {
        if (!NetworkServer.active || CurrentLobbyID == 0) return;
        SteamMatchmaking.SetLobbyData(new CSteamID(CurrentLobbyID), ModeKey, modeName);
    }
    // Fired by a Steam friend invite / "Join Game" from the friends list -
    // a completely separate entry point from the room browser's Join
    // button, so it needs its own password check (GetLobbyData works here
    // too, no need to have joined yet).
    void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback)
    {
        print("Join Lobby");
        string storedHash = SteamMatchmaking.GetLobbyData(callback.m_steamIDLobby, PasswordKey);
        if (!string.IsNullOrEmpty(storedHash))
        {
            PasswordPromptPanel.Instance?.Open(callback.m_steamIDLobby);
            return;
        }
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }
    void OnLobbyEntered(LobbyEnter_t callback)
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
        Manager.networkAddress = hostAddress;
        Manager.StartClient();
    }
    public void GetLobbyList()
    {
        if (lobbyIDs.Count > 0) lobbyIDs.Clear();
        SteamMatchmaking.AddRequestLobbyListResultCountFilter(60);
        SteamMatchmaking.RequestLobbyList();

    }
    public void JoinLobby(CSteamID lobbyID)
    {
        SteamMatchmaking.JoinLobby(lobbyID);
    }
    // SHA256 hex string - never compare/store the raw password, GetLobbyData
    // is public even to non-members.
    public static string HashPassword(string password)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(password ?? ""));
            StringBuilder builder = new StringBuilder(hash.Length * 2);
            foreach (byte b in hash) builder.Append(b.ToString("x2"));
            return builder.ToString();
        }
    }
    // Used by the join-password prompt. A room with no PasswordKey set
    // (GetLobbyData returns "") joins straight through same as before -
    // only a mismatched non-empty stored hash actually blocks the join.
    public bool TryJoinWithPassword(CSteamID lobbyID, string enteredPassword)
    {
        string storedHash = SteamMatchmaking.GetLobbyData(lobbyID, PasswordKey);
        if (!string.IsNullOrEmpty(storedHash) && HashPassword(enteredPassword) != storedHash)
        {
            return false;
        }
        JoinLobby(lobbyID);
        return true;
    }
    void OnGetLobbyList(LobbyMatchList_t callback)
    {
        if (isQuickMatching)
        {
            isQuickMatching = false;
            for (int i = 0; i < callback.m_nLobbiesMatching; i++)
            {
                CSteamID lobbyID = SteamMatchmaking.GetLobbyByIndex(i);
                // Quick Match has no password prompt UI - skip protected
                // rooms entirely rather than failing to join one silently.
                if (!string.IsNullOrEmpty(SteamMatchmaking.GetLobbyData(lobbyID, PasswordKey))) continue;
                if (SteamMatchmaking.GetNumLobbyMembers(lobbyID) < SteamMatchmaking.GetLobbyMemberLimit(lobbyID))
                {
                    JoinLobby(lobbyID);
                    return;
                }
            }
            // No open room found - host a fresh one instead.
            HostLobby();
            return;
        }

        // Will call twice when Leave Game Back to Lobby
        if (LobbyListManager.instance.listOfLobbies.Count > 0) LobbyListManager.instance.DestroyLobbies();
        for(int i = 0 ; i < callback.m_nLobbiesMatching ; i++)
        {
            CSteamID lobbyID = SteamMatchmaking.GetLobbyByIndex(i);
            lobbyIDs.Add(lobbyID);
            // Triggers a LobbyDataUpdate_t callback.
            SteamMatchmaking.RequestLobbyData(lobbyID);
        }
    }
    void OnGetLobbyData(LobbyDataUpdate_t callback)
    {
        LobbyListManager.instance.DisplayLobbies(lobbyIDs,callback);
    }
}

}