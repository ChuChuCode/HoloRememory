using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using Steamworks;
using UnityEngine.UI;
using UnityEngine.SocialPlatforms;
using Mirror;
using HR.Network.Select;

namespace HR.Network.Lobby{
public class LobbyController : MonoBehaviour
{
    public bool AllReady;
    public static LobbyController Instance;
    // Fixed 8 slots, pre-placed in the scene - never instantiated/destroyed,
    // just filled or cleared to "empty". TeamID is auto-spread across the 8
    // colors on connect, but players can freely re-pick any of the 8 colored
    // slots via the buttons below (SlotChange) - teams can be uneven in size.
    [Header("Player List (fixed 8 slots)")]
    [SerializeField] LobbyPlayerSlot[] PlayerSlots = new LobbyPlayerSlot[8];
    // Other Data
    public ulong CurrentLobbyID;
    [Header("UI")]
    public TMP_Text LobbyNameText;
    // Sits above ModeText in the layout - fixed at 3:00 for now (see
    // GameSettings.TimeLimit), not adjustable yet.
    public TMP_Text TimeLimitText;
    public TMP_Text ModeText;
    // Single shared button - host sees "Start", everyone else sees "Ready"/
    // "Unready". See MainButtonClick/RefreshMainButton.
    public TMP_Text ReadyButtonText;
    public Button ReadyButton;
    [Header("Slot Buttons (1-8, White/Red/Orange/Yellow/Green/Blue/Purple/Pink)")]
    [SerializeField] Button[] SlotButtons = new Button[8];
    [Header("Character Select (merged in from the old Select_Scene)")]
    [SerializeField] CharacterSelectItem CharacterPrefab;
    [SerializeField] Transform CharacterSelectPanel;
    [SerializeField] List<CharacterSelectItem> SelectItemList = new List<CharacterSelectItem>();
    // Any host-only controls (mode arrows, map select buttons, etc.) - drop
    // in as many GameObjects as needed, all hidden together for non-hosts.
    [Header("Host-Only UI")]
    [SerializeField] List<GameObject> HostOnlyUI = new List<GameObject>();
    [Header("Map")]
    // Big preview above the carousel - always the currently selected map's
    // image, no text label of its own.
    [SerializeField] Image MapPreviewImage;
    // 3-card carousel below - center card is always the currently selected
    // map (same image as MapPreviewImage); left/right show what
    // NextMap()/PreviousMap() would switch to. Each card has its own text.
    [SerializeField] Image MapCardLeftImage;
    [SerializeField] TMP_Text MapCardLeftText;
    [SerializeField] Image MapCardCenterImage;
    [SerializeField] TMP_Text MapCardCenterText;
    [SerializeField] Image MapCardRightImage;
    [SerializeField] TMP_Text MapCardRightText;
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
    [HideInInspector] public PlayerObject LocalPlayerController;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
    void Start()
    {
        // TEMP diagnostic - remove once the Lobby map/mode display bug is found.
        Debug.Log($"[LobbyDebug] LobbyController.Start: GameSettings.Instance={(GameSettings.Instance != null ? "ready" : "NULL")}, characters={Manager.characterSelectComponentsList.Count}, maps={Manager.mapConfigList.Count}, time={Time.realtimeSinceStartup:F2}");
        // Character select buttons - built once per Lobby_Scene load (ported
        // from the old Select_Scene, which no longer exists as a separate step).
        foreach (CharacterSelectComponent characterSelectComponent in Manager.characterSelectComponentsList)
        {
            CharacterSelectItem item = Instantiate(CharacterPrefab);
            item.SetCharacterData(characterSelectComponent.ID, characterSelectComponent.CharacterName, characterSelectComponent.CharacterImage, characterSelectComponent.SelectAudio);
            item.transform.SetParent(CharacterSelectPanel);
            item.transform.localScale = Vector3.one;
            SelectItemList.Add(item);
        }
        // Host-only controls - hidden for everyone else.
        if (!NetworkServer.active)
        {
            foreach (GameObject ui in HostOnlyUI)
            {
                if (ui != null) ui.SetActive(false);
            }
        }
        UpdateTimeLimitText();
        UpdateModeText();
        UpdateMapText();
        // Safety net: both calls above silently no-op if GameSettings.Instance
        // (a scene-placed NetworkBehaviour singleton) isn't set yet at this
        // exact moment - a same-frame ordering race between different
        // objects' Start() calls that a real build can hit even when the
        // Editor never does. Once more, next frame, catches it either way.
        StartCoroutine(RefreshModeAndMapNextFrame());
        // If restart game -> PlayersInfoList has player -> re-get LocalGamePlayer object and refresh the button
        if (Manager.PlayersInfoList.Count > 0)
        {
            LocalPlayerController = GameObject.Find("LocalGamePlayer").GetComponent<PlayerObject>();
            RefreshMainButton();
        }
    }
    public void UpdateLobbyName()
    {
        CurrentLobbyID = SteamLobby.Instance.CurrentLobbyID;
        LobbyNameText.text = SteamMatchmaking.GetLobbyData(new CSteamID(CurrentLobbyID),"name");
    }
    // TODO: fixed display for now, no arrows yet - see GameSettings.TimeLimit.
    public void UpdateTimeLimitText()
    {
        if (TimeLimitText == null || GameSettings.Instance == null) return;
        int min = (int)GameSettings.Instance.TimeLimit / 60;
        int sec = (int)GameSettings.Instance.TimeLimit % 60;
        TimeLimitText.text = string.Format("{0:00}:{1:00}", min, sec);
    }
    // Room Mode display - refreshed on Start and whenever GameSettings.Mode
    // syncs (GameSettings.OnModeChanged), since any client can change it.
    public void UpdateModeText()
    {
        // TEMP diagnostic - remove once the Lobby map/mode display bug is found.
        Debug.Log($"[LobbyDebug] UpdateModeText: ModeText={(ModeText != null ? "ok" : "NULL")}, GameSettings.Instance={(GameSettings.Instance != null ? "ready" : "NULL")}");
        if (ModeText == null || GameSettings.Instance == null) return;
        GameModeConfig config = GameSettings.Instance.CurrentConfig();
        Debug.Log($"[LobbyDebug] UpdateModeText: Mode={GameSettings.Instance.Mode}, config={(config != null ? config.DisplayName : "NULL (no matching GameModeConfig)")}");
        ModeText.text = config != null ? config.DisplayName : GameSettings.Instance.Mode.ToString();
    }
    // Left/Right Arrow Buttons - cycles through GameSettings.Configs in
    // order. The arrows should be in HostOnlyUI (see Start()), so only the
    // host ever has a way to trigger these.
    public void NextMode()
    {
        ChangeMode(1);
    }
    public void PreviousMode()
    {
        ChangeMode(-1);
    }
    void ChangeMode(int direction)
    {
        if (GameSettings.Instance == null) return;
        GameModeConfig[] configs = GameSettings.Instance.Configs;
        if (configs == null || configs.Length == 0) return;

        int currentIndex = System.Array.FindIndex(configs, c => c.mode == GameSettings.Instance.Mode);
        if (currentIndex == -1) currentIndex = 0;

        int nextIndex = (currentIndex + direction + configs.Length) % configs.Length;
        SetMode(configs[nextIndex].mode);
    }
    public void SetMode(GameMode mode)
    {
        if (GameSettings.Instance == null) return;
        GameSettings.Instance.CmdSetMode(mode);
    }
    // Bound to the single shared button - host triggers Start, everyone
    // else toggles their own Ready.
    public void MainButtonClick()
    {
        if (NetworkServer.active)
        {
            StarGame();
        }
        else
        {
            ReadyPlayer();
        }
    }
    // Host: "Start", enabled once every other player is Ready and the host
    // has picked a character themselves.
    // TEMP (testing): the "at least 2 teams" requirement is disabled below
    // so a single color can start solo - re-add "&& HasAtLeastTwoTeams()"
    // once done testing.
    // Everyone else: "Ready"/"Unready", a plain toggle gated only on having
    // picked a character - stays clickable even once Ready so it can be undone.
    public void RefreshMainButton()
    {
        if (ReadyButton == null || LocalPlayerController == null) return;

        bool hasCharacter = LocalPlayerController.CharacterID != -1;

        if (NetworkServer.active)
        {
            if (ReadyButtonText != null) ReadyButtonText.text = "Start";
            ReadyButton.interactable = hasCharacter && AllReady;
        }
        else
        {
            if (ReadyButtonText != null) ReadyButtonText.text = LocalPlayerController.Ready ? "Unready" : "Ready";
            ReadyButton.interactable = hasCharacter;
        }
    }
    // Fills the 8 fixed slots from the current roster - no more
    // Instantiate/Destroy, just data updates. Sorted by ConnectionID (a
    // SyncVar, so identical on every screen) rather than PlayersInfoList's
    // raw order, since Mirror doesn't guarantee that order matches between
    // the host and a client who joined mid-session - without this, slot 3
    // could show a different player on different screens.
    public void UpdatePlayerList()
    {
        List<PlayerObject> sortedPlayers = Manager.PlayersInfoList.OrderBy(p => p.ConnectionID).ToList();

        for (int i = 0; i < PlayerSlots.Length; i++)
        {
            if (PlayerSlots[i] == null) continue;

            if (i < sortedPlayers.Count)
            {
                PlayerObject player = sortedPlayers[i];
                PlayerSlots[i].SetPlayer(player);
            }
            else
            {
                PlayerSlots[i].SetEmpty();
            }
        }
        CheckSlotButtons();
        CheckCharacterButtons();
        CheckIfAllReady();
        RefreshMainButton();
    }
    // The host has no Ready toggle of their own (Start IS their
    // confirmation), so they're excluded from this check - it only asks
    // whether every OTHER player is Ready.
    public void CheckIfAllReady()
    {
        AllReady = true;

        foreach(PlayerObject player in Manager.PlayersInfoList)
        {
            if (player.ConnectionID == 0) continue;
            if (!player.Ready)
            {
                AllReady = false;
                break;
            }
        }
    }
    // Teams are asymmetric - any number of players can share a color, so a
    // slot button only disables when it's already your own pick, or you're
    // Ready (locked in). It never disables just because someone else has it.
    public void CheckSlotButtons()
    {
        for (int i = 0; i < SlotButtons.Length; i++)
        {
            if (SlotButtons[i] == null) continue;
            int slot = i + 1;

            bool isMine = LocalPlayerController != null && LocalPlayerController.TeamID == slot;
            bool locked = LocalPlayerController != null && LocalPlayerController.Ready;

            SlotButtons[i].interactable = !isMine && !locked;
        }
    }
    // Same "locked while Ready" pattern as CheckSlotButtons: a character
    // button only disables for being your own current pick, or Ready.
    public void CheckCharacterButtons()
    {
        bool locked = LocalPlayerController != null && LocalPlayerController.Ready;
        foreach (CharacterSelectItem item in SelectItemList)
        {
            bool isMine = LocalPlayerController != null && LocalPlayerController.CharacterID == item.CharacterID;
            Button btn = item.GetComponent<Button>();
            if (btn != null) btn.interactable = !isMine && !locked;
        }
    }
    // Left/Right Arrow Buttons - same cycling pattern as NextMode/PreviousMode,
    // sourced from Network_Manager.mapConfigList (auto-loaded from
    // Resources/Data/Map). Should be in HostOnlyUI too.
    public void NextMap()
    {
        ChangeMap(1);
    }
    public void PreviousMap()
    {
        ChangeMap(-1);
    }
    void ChangeMap(int direction)
    {
        if (GameSettings.Instance == null) return;
        List<MapConfig> maps = Manager.mapConfigList;
        if (maps == null || maps.Count == 0) return;

        int currentIndex = maps.FindIndex(c => c.SceneName == GameSettings.Instance.MapName);
        if (currentIndex == -1) currentIndex = 0;

        int nextIndex = (currentIndex + direction + maps.Count) % maps.Count;
        change_map(maps[nextIndex].SceneName);
    }
    // Map Button - host-only; synced to everyone via GameSettings.MapName.
    public void change_map(string mapName)
    {
        if (!NetworkServer.active) return;
        GameSettings.Instance.CmdSetMap(mapName);
    }
    public void UpdateMapText()
    {
        // TEMP diagnostic - remove once the Lobby map/mode display bug is found.
        Debug.Log($"[LobbyDebug] UpdateMapText: GameSettings.Instance={(GameSettings.Instance != null ? "ready" : "NULL")}");
        if (GameSettings.Instance == null) return;
        List<MapConfig> maps = Manager.mapConfigList;
        MapConfig current = maps != null ? maps.Find(c => c.SceneName == GameSettings.Instance.MapName) : null;
        Debug.Log($"[LobbyDebug] UpdateMapText: mapConfigList.Count={(maps != null ? maps.Count : -1)}, GameSettings.MapName='{GameSettings.Instance.MapName}', matchedCurrent={(current != null ? current.DisplayName : "NULL (no MapConfig.SceneName matches MapName)")}");
        RefreshMapCarousel(maps, current);
    }
    // GameSettings is a scene-placed NetworkIdentity - the log confirmed its
    // Instance is still null a full frame after LobbyController.Start(),
    // meaning it isn't simply a same-frame ordering race (that's what the
    // single "yield return null" used to assume) - something about the
    // host/server startup sequence sets it up later than that. Polling here
    // is correct either way regardless of the exact Mirror-internal reason.
    IEnumerator RefreshModeAndMapNextFrame()
    {
        float elapsed = 0f;
        const float timeout = 5f;
        while (GameSettings.Instance == null && elapsed < timeout)
        {
            yield return null;
            elapsed += Time.deltaTime;
        }
        // TEMP diagnostic - remove once the Lobby map/mode display bug is found.
        Debug.Log($"[LobbyDebug] RefreshModeAndMapNextFrame: waited {elapsed:F2}s, GameSettings.Instance={(GameSettings.Instance != null ? "ready" : "STILL NULL - gave up")}, maps={Manager.mapConfigList.Count}, MapName={(GameSettings.Instance != null ? GameSettings.Instance.MapName : "n/a")}, time={Time.realtimeSinceStartup:F2}");
        UpdateModeText();
        UpdateMapText();
    }
    // Called on Start and whenever GameSettings.MapName syncs (host or
    // anyone else's screen alike), so every client's carousel stays centered
    // on the room's actual current pick, even though only the host can
    // scroll it (arrows are host-only, see HostOnlyUI).
    void RefreshMapCarousel(List<MapConfig> maps, MapConfig current)
    {
        if (maps == null || maps.Count == 0 || current == null) return;

        int centerIndex = maps.IndexOf(current);
        int count = maps.Count;
        MapConfig left = maps[((centerIndex - 1) % count + count) % count];
        MapConfig right = maps[(centerIndex + 1) % count];

        if (MapPreviewImage != null) MapPreviewImage.sprite = current.Thumbnail;

        SetMapCard(MapCardLeftImage, MapCardLeftText, left);
        SetMapCard(MapCardCenterImage, MapCardCenterText, current);
        SetMapCard(MapCardRightImage, MapCardRightText, right);
    }
    void SetMapCard(Image image, TMP_Text text, MapConfig config)
    {
        if (image != null) image.sprite = config.Thumbnail;
        if (text != null) text.text = config.DisplayName;
    }
    // Start requires at least 2 different colors actually in play - everyone
    // picking the same one would "start" a match with no opponents.
    bool HasAtLeastTwoTeams()
    {
        HashSet<int> teams = new HashSet<int>();
        foreach (PlayerObject player in Manager.PlayersInfoList)
        {
            teams.Add(player.TeamID);
        }
        return teams.Count >= 2;
    }
    // Called from MainButtonClick when the local player isn't the host.
    void ReadyPlayer()
    {
        LocalPlayerController.ChangeReady();
    }
    // Slot Button
    public void SlotChange(int TeamID)
    {
        LocalPlayerController.CanTeamJoin(TeamID);
    }
    // Called from MainButtonClick when the local player is the host - goes
    // straight to the picked map (change_map) since Select_Scene no longer
    // exists as a separate step.
    void StarGame()
    {
        LocalPlayerController.CanStartGame(GameSettings.Instance.MapName);
    }
    // Leave Button
    public void LeaveGame()
    {
        LocalPlayerController.LeaveGame();
    }
}

}