using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Steamworks;
using System.Linq;
using HR.UI;
using HR.Object.Spell;

namespace HR.Network.Select{
public class SelectController : MonoBehaviour
{
    public static SelectController Instance;
    [Header("Select Prefab")]
    public Network_SelectPlayer SelectPlayerPrefab;
    [SerializeField] CharacterSelectItem CharacterPrefab;
    [SerializeField] Transform Select_Character_Panel;
    [SerializeField] List<CharacterSelectItem> SelectItemList = new List<CharacterSelectItem>();
    [Header("Team")]
    [SerializeField] Transform Team1_transform;
    [SerializeField] Transform Team2_transform;
    // Other Data
    public ulong CurrentLobbyID;
    public bool PlayerItemCreated = false;
    [Header("UI")]
    public TMP_Text LobbyNameText;
    public Button ReadyButton;
    [Header("Spell")]
    [SerializeField] Transform Spell_1_Position;
    [SerializeField] Transform Spell_2_Position;
    [SerializeField] Spell_Button_Component Spell_Component_Prefab;
    [SerializeField] List<SpellBase> Spell_Buttons = new List<SpellBase>();
    [SerializeField] Spell_Select Spell_1;
    [SerializeField] Spell_Select Spell_2;
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
    public List<Network_SelectPlayer> Team1_networkSelectPlayersList = new List<Network_SelectPlayer>();
    public List<Network_SelectPlayer> Team2_networkSelectPlayersList = new List<Network_SelectPlayer>();
    public PlayerObject LocalPlayerController;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
    void Start()
    {
        // Update Lobby Name
        CurrentLobbyID = SteamLobby.Instance.CurrentLobbyID;
        LobbyNameText.text = SteamMatchmaking.GetLobbyData(new CSteamID(CurrentLobbyID),"name");
        ReadyButton.interactable = false;
        // Set Character Select
        foreach(CharacterSelectComponent characterSelectComponent in Manager.characterSelectComponentsList)
        {
            CharacterSelectItem temp_characterSelectComponent = Instantiate(CharacterPrefab);
            // Set Sprite and CharacterID
            temp_characterSelectComponent.SetImage(characterSelectComponent.CharacterImage);
            temp_characterSelectComponent.CharacterID = characterSelectComponent.ID;
            // Set Audio
            temp_characterSelectComponent.audioClip = characterSelectComponent.SelectAudio;
            // Set Parent
            temp_characterSelectComponent.transform.SetParent(Select_Character_Panel);
            temp_characterSelectComponent.transform.localScale = Vector3.one;
            SelectItemList.Add(temp_characterSelectComponent);
        }
        // Initial Spell_Button_Component
        var SpellObjects = Resources.LoadAll("Data/Spell");
        foreach (var SpellObject in SpellObjects)
        {
            SpellBase spell_sprite = SpellObject as SpellBase;
            // SpellBase add to List
            Spell_Buttons.Add(spell_sprite);
            /// Add to Spell 1
            Spell_Button_Component temp_Spell = Instantiate(Spell_Component_Prefab);
            // Set index
            temp_Spell.SetIndex(1,spell_sprite.SpellIndex);
            // Set Parent
            temp_Spell.transform.SetParent(Spell_1_Position);
            temp_Spell.transform.localScale = Vector3.one;
            // Set Image
            temp_Spell.SetSprite(spell_sprite.Spell_Sprite);

            /// Add to Spell 2
            Spell_Button_Component temp_Spell1 = Instantiate(Spell_Component_Prefab);
            // Set index
            temp_Spell1.SetIndex(2,spell_sprite.SpellIndex);
            // Set Parent
            temp_Spell1.transform.SetParent(Spell_2_Position);
            temp_Spell1.transform.localScale = Vector3.one;
            // Set Image
            temp_Spell1.SetSprite(spell_sprite.Spell_Sprite);
        }
        // Set UI
        foreach (PlayerObject player in  Manager.PlayersInfoList)
        {
            Network_SelectPlayer network_SelectPlayer = Instantiate(SelectPlayerPrefab);
            GameObject SelectPlayer = network_SelectPlayer.gameObject;

            // Set Localplayer
            if (player.isOwned)
            {
                LocalPlayerController = player;
            }

            network_SelectPlayer.PlayerName = player.PlayerName;
            network_SelectPlayer.ConnectionID = player.ConnectionID;
            network_SelectPlayer.PlayerSteamID = player.PlayerSteamID;
            network_SelectPlayer.TeamID = player.TeamID;
            network_SelectPlayer.SetPlayerValues();
            // Set Team
            if (network_SelectPlayer.TeamID == 1)
            {
                // Add to team1 list
                Team1_networkSelectPlayersList.Add(network_SelectPlayer);
                // Add to UI
                SelectPlayer.transform.SetParent(Team1_transform);
            }
            else
            {
                // Add to team2 list
                Team2_networkSelectPlayersList.Add(network_SelectPlayer);
                // Add to UI
                SelectPlayer.transform.SetParent(Team2_transform);
            }
            SelectPlayer.transform.localScale = Vector3.one;
        }
    }
    // Search Character with Character ID
    Sprite Search_Character(int CharacterID)
    {
        if (CharacterID == -1) return null;
        CharacterSelectComponent temp = manager.characterSelectComponentsList.Find(component => component.ID == CharacterID);
        return temp.CharacterImage;
    }
    public void UpdatePlayerList()
    {
        // // Create Host Player(Already exit in room) 
        // // LobbyPlayerList.OnStartAuthority
        // if(!PlayerItemCreated)
        // {
        //     // Host
        //     CreateHostPlayerItem();
        // }
        // // Create Owner Player
        // // LobbyPlayerList.OnStartClient
        // if (Team1_networkSelectPlayersList.Count + Team2_networkSelectPlayersList.Count < Manager.PlayersInfoList.Count)
        // {
        //     CreateClientPlayerItem();
        // }
        // // Check Anyone leave
        // // LobbyPlayerList.OnStopClient
        // if (Team1_networkSelectPlayersList.Count + Team2_networkSelectPlayersList.Count > Manager.PlayersInfoList.Count)
        // {
        //     RemovePlayerItem();
        // }
        // Update things
        // LobbyPlayerList.PlayerNameUdate
        if (Team1_networkSelectPlayersList.Count + Team2_networkSelectPlayersList.Count == Manager.PlayersInfoList.Count)
        {
            UpdatePlayerUI();
        }
    }
    public void CreateHostPlayerItem()
    {
        print("HOST create");
        foreach(PlayerObject player in Manager.PlayersInfoList)
        {
            Network_SelectPlayer network_SelectPlayer = Instantiate(SelectPlayerPrefab);
            GameObject SelectPlayer = network_SelectPlayer.gameObject;

            network_SelectPlayer.PlayerName = player.PlayerName;
            network_SelectPlayer.ConnectionID = player.ConnectionID;
            network_SelectPlayer.PlayerSteamID = player.PlayerSteamID;
            network_SelectPlayer.TeamID = player.TeamID;
            // network_SelectPlayer.isReady = player.Ready;
            network_SelectPlayer.SetPlayerValues();

            SelectPlayer.transform.localScale = Vector3.one;
            // Set Team if #Team1_player < 5 -> add to Team1
            if (network_SelectPlayer.TeamID == 1)
            {
                // Add to team1 list
                Team1_networkSelectPlayersList.Add(network_SelectPlayer);
                // Add to UI
                SelectPlayer.transform.SetParent(Team1_transform);
            }
            else
            {
                // Add to team2 list
                Team2_networkSelectPlayersList.Add(network_SelectPlayer);
                // Add to UI
                SelectPlayer.transform.SetParent(Team2_transform);
            }
        }
        PlayerItemCreated = true;
    }
    public void CreateClientPlayerItem()
    {
        print("CLIENT ceate");
        foreach(PlayerObject player in Manager.PlayersInfoList)
        {
            if (!Team1_networkSelectPlayersList.Any(b => b.ConnectionID == player.ConnectionID) && !Team2_networkSelectPlayersList.Any(b => b.ConnectionID == player.ConnectionID))
            {
                Network_SelectPlayer network_SelectPlayer = Instantiate(SelectPlayerPrefab);
                GameObject SelectPlayer = network_SelectPlayer.gameObject;

                network_SelectPlayer.PlayerName = player.PlayerName;
                network_SelectPlayer.ConnectionID = player.ConnectionID;
                network_SelectPlayer.PlayerSteamID = player.PlayerSteamID;
                network_SelectPlayer.TeamID = player.TeamID;
                // network_SelectPlayer.isReady = player.Ready;
                network_SelectPlayer.SetPlayerValues();

                SelectPlayer.transform.localScale = Vector3.one;
                // Set Team if #Team1_player < 5 -> add to Team1
                if (network_SelectPlayer.TeamID == 1)
                {
                    // Add to team1 list
                    Team1_networkSelectPlayersList.Add(network_SelectPlayer);
                    // Add to UI
                    SelectPlayer.transform.SetParent(Team1_transform);
                }
                else
                {
                    // Add to team2 list
                    Team2_networkSelectPlayersList.Add(network_SelectPlayer);
                    // Add to UI
                    SelectPlayer.transform.SetParent(Team2_transform);
                }
            }
        }
    }
    public void UpdatePlayerUI()
    {
        foreach(PlayerObject player in Manager.PlayersInfoList)
        {
            foreach(Network_SelectPlayer PlayerListItemScript in Team1_networkSelectPlayersList)
            {
                if (PlayerListItemScript.ConnectionID == player.ConnectionID)
                {
                    // Update Character Image
                    PlayerListItemScript.SetCharacterImage(Search_Character(player.CharacterID));
                    // Update Spell Image
                    PlayerListItemScript.Spell_1.sprite = Search_Spell(player.Spell_1)?.Spell_Sprite;
                    PlayerListItemScript.Spell_2.sprite = Search_Spell(player.Spell_2)?.Spell_Sprite;
                    // if player is local player -> update Ready Button
                    if (player == LocalPlayerController)
                    {
                        // UpdateButton();
                    }
                }
            }
            foreach(Network_SelectPlayer PlayerListItemScript in Team2_networkSelectPlayersList)
            {
                if (PlayerListItemScript.ConnectionID == player.ConnectionID)
                {
                    // Update Character Image
                    PlayerListItemScript.SetCharacterImage(Search_Character(player.CharacterID));
                    // Update Spell Image
                    PlayerListItemScript.Spell_1.sprite = Search_Spell(player.Spell_1)?.Spell_Sprite;
                    PlayerListItemScript.Spell_2.sprite = Search_Spell(player.Spell_2)?.Spell_Sprite;
                    // if player is local player -> update Ready Button
                    if (player == LocalPlayerController)
                    {
                        // UpdateButton();
                    }
                }
            }
        }
    }
    public void RemovePlayerItem()
    {
        List<Network_SelectPlayer> playerListItemToRemove = new List<Network_SelectPlayer>();

        foreach(Network_SelectPlayer playerlistItem in Team1_networkSelectPlayersList)
        {
            if(!Manager.PlayersInfoList.Any(b => b.ConnectionID == playerlistItem.ConnectionID))
            {
                playerListItemToRemove.Add(playerlistItem);
            }
        }
        if (playerListItemToRemove.Count > 0)
        {
            foreach(Network_SelectPlayer playerlistItemToRemove in playerListItemToRemove)
            {
                GameObject ObjectToRemove = playerlistItemToRemove.gameObject;
                Team1_networkSelectPlayersList.Remove(playerlistItemToRemove);
                Destroy(ObjectToRemove);
                ObjectToRemove = null;
            }
        }
        playerListItemToRemove = new List<Network_SelectPlayer>();
        foreach(Network_SelectPlayer playerlistItem in Team2_networkSelectPlayersList)
        {
            if(!Manager.PlayersInfoList.Any(b => b.ConnectionID == playerlistItem.ConnectionID))
            {
                playerListItemToRemove.Add(playerlistItem);
            }
        }
        if (playerListItemToRemove.Count > 0)
        {
            foreach(Network_SelectPlayer playerlistItemToRemove in playerListItemToRemove)
            {
                GameObject ObjectToRemove = playerlistItemToRemove.gameObject;
                Team2_networkSelectPlayersList.Remove(playerlistItemToRemove);
                Destroy(ObjectToRemove);
                ObjectToRemove = null;
            }
        }
    }
    public void CheckIfAllReady()
    {
        bool AllReady = false;

        foreach(PlayerObject player in Manager.PlayersInfoList)
        {
            if (player.Ready)
            {
                AllReady = true;
            }
            else
            {
                AllReady = false;
                break;
            }
        }
        // Start Button Clickable
        if (AllReady)
        {
            // Start Game
            LocalPlayerController.CanStartGame("Game_Scene");
        }
    }
    // Select Button
    public void SelectPlayer()
    {
        ReadyButton.interactable = false;
        LocalPlayerController.ChangeReady();
        // UI Disable
        foreach (CharacterSelectItem selectItem in SelectItemList)
        {
            selectItem.GetComponent<Button>().interactable = false;
        }
        StartCoroutine(nameof(CheckStartGame));
    }
    // Set other player interactable false
    public void Character_Interactable(int CharacterID,bool isInteractable)
    {
        CharacterSelectItem selected = SelectItemList.Find(component => component.CharacterID == CharacterID);
        if (selected == null) return;
        selected.GetComponent<Button>().interactable = isInteractable;
    }
    IEnumerator CheckStartGame()
    {
        yield return new WaitForSeconds(1);
        CheckIfAllReady();
    }
    // Spell_Button_Component click method -> Call Spell_Select
    public void SelectSpell(int spell_Slot,int spell_Index)
    {
        if (spell_Slot == 1 )
        {
            Spell_1.Spell_Button_Click(spell_Index);
        }
        else
        {
            Spell_2.Spell_Button_Click(spell_Index);
        }
    }
    public SpellBase Search_Spell(int spell_Index)
    {
        SpellBase spell = Spell_Buttons.Find(x => x.SpellIndex == spell_Index);
        return Spell_Buttons.Find(x => x.SpellIndex == spell_Index);
    }
}

}