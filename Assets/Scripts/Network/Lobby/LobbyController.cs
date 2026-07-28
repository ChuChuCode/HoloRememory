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
    public TMP_Text RoomCountText;
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
    [Header("Map")]
    public GameObject MapSelectPanel;
    public TMP_Text MapNameText;
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
        if (!NetworkServer.active)
        {
            if (MapSelectPanel != null) MapSelectPanel.SetActive(false);
        }
        UpdateRoomCount();
        UpdateModeText();
        UpdateMapText();
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
    public void UpdateRoomCount()
    {
        if (RoomCountText == null) return;
        RoomCountText.text = $"{Manager.PlayersInfoList.Count} / {Manager.maxConnections}";
    }
    // Room Mode display - refreshed on Start and whenever GameSettings.Mode
    // syncs (GameSettings.OnModeChanged), since any client can change it.
    public void UpdateModeText()
    {
        if (ModeText == null || GameSettings.Instance == null) return;
        ModeText.text = GameSettings.Instance.Mode.ToString();
    }
    // Mode Button/Dropdown
    public void SetMode(int mode)
    {
        if (GameSettings.Instance == null) return;
        GameSettings.Instance.CmdSetMode((GameMode)mode);
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
        UpdateRoomCount();
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
    // Map Button - host-only; synced to everyone via GameSettings.MapName.
    public void change_map(string mapName)
    {
        if (!NetworkServer.active) return;
        GameSettings.Instance.CmdSetMap(mapName);
    }
    public void UpdateMapText()
    {
        if (MapNameText == null || GameSettings.Instance == null) return;
        MapNameText.text = GameSettings.Instance.MapName;
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