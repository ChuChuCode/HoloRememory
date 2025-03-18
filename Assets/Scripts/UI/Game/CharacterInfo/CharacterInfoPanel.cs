using UnityEngine;
using HR.Network;
using System.Collections.Generic;
using TMPro;
using Mirror;
using HR.Object.Player;

namespace HR.UI{
public class CharacterInfoPanel : NetworkBehaviour
{
    public static CharacterInfoPanel Instance;
    [Header("Spawn Prefab")]
    [SerializeField] PlayerInfo_Component PlayerInfo_Component;
    [SerializeField] Transform Team1_Parent;
    [SerializeField] Transform Team2_Parent;
    [Header("Team List Info")]
    [SerializeField] List<PlayerInfo_Component> Team1_PlayerInfo_Components = new List<PlayerInfo_Component>();
    [SerializeField] List<PlayerInfo_Component> Team2_PlayerInfo_Components = new List<PlayerInfo_Component>();
    [Header("Tower Number")]
    [SerializeField] TMP_Text Team1_Tower_Text;
    [SerializeField] TMP_Text Team2_Tower_Text;
    [Header("Kill Number")]
    [SerializeField] TMP_Text Team1_Kill_Text;
    [SerializeField] TMP_Text Team2_Kill_Text;
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
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
    void Start()
    {
        // Initial UI
        foreach (PlayerObject player in Manager.PlayersInfoList)
        {
            PlayerInfo_Component playerInfo = Instantiate(PlayerInfo_Component);
            if (player.TeamID == 1 )
            {
                playerInfo.transform.SetParent(Team1_Parent);
                Team1_PlayerInfo_Components.Add(playerInfo);
            }
            else
            {
                playerInfo.transform.SetParent(Team2_Parent);
                Team2_PlayerInfo_Components.Add(playerInfo);
            }
            playerInfo.transform.localScale = Vector3.one;
            playerInfo.PlayerID = player.PlayerIdNumber;
            // Initial Info
            // playerInfo.Initial(sprite, character);
        }
        gameObject.SetActive(false);
    }
    void Bind_Character()
    {
        foreach (PlayerInfo_Component playerInfo_Components in Team1_PlayerInfo_Components)
        {
            CharacterBase characterBase = Manager.Player_List.Find(x => x.PlayerIdNumber == playerInfo_Components.PlayerID);
            if (characterBase != null)
            {
                playerInfo_Components.Initial(characterBase);
            }
        }
        // Calculate Team 2 tower number and Info
        foreach (PlayerInfo_Component playerInfo_Components in Team2_PlayerInfo_Components)
        {
            CharacterBase characterBase = Manager.Player_List.Find(x => x.PlayerIdNumber == playerInfo_Components.PlayerID);
            if (characterBase != null)
            {
                playerInfo_Components.Initial(characterBase);
            }
        }
    }
    // UI Update
    public void UpdateUI()
    {
        Bind_Character();
        // Calculate Team 1 tower number and Info
        int tower_number = 0;
        int kill_number = 0;
        foreach (PlayerInfo_Component playerInfo_Components in Team1_PlayerInfo_Components)
        {
            if (playerInfo_Components.characterBase == null) continue;
            playerInfo_Components.UpdateInfo();
            tower_number += playerInfo_Components.characterBase.tower;
            kill_number += playerInfo_Components.characterBase.kill;
        }
        Team1_Tower_Text.text = tower_number.ToString();
        Team1_Kill_Text.text = kill_number.ToString();
        // Calculate Team 2 tower number and Info
        tower_number = 0;
        kill_number = 0;
        foreach (PlayerInfo_Component playerInfo_Components in Team2_PlayerInfo_Components)
        {
            if (playerInfo_Components.characterBase == null) continue;
            playerInfo_Components.UpdateInfo();
            tower_number += playerInfo_Components.characterBase.tower;
            kill_number += playerInfo_Components.characterBase.kill;
        }
        Team2_Tower_Text.text = tower_number.ToString();
        Team2_Kill_Text.text = kill_number.ToString();
    }
}

}