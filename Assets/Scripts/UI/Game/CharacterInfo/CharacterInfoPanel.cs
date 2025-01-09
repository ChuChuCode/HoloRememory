using UnityEngine;
using HR.Network;
using System.Collections.Generic;
using TMPro;

namespace HR.UI{
public class CharacterInfoPanel : MonoBehaviour
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
        // Initial_Info();
        gameObject.SetActive(false);
    }
    public void Add_to_Info(Sprite sprite,GameObject character)
    {
        // Spawn prefab
        PlayerInfo_Component playerInfo = Instantiate(PlayerInfo_Component);
        // Add to parent
        if (character.layer == LayerMask.NameToLayer("Team1") )
        {
            playerInfo.transform.SetParent(Team1_Parent);
            Team1_PlayerInfo_Components.Add(playerInfo);
        }
        else
        {
            playerInfo.transform.SetParent(Team2_Parent);
            Team2_PlayerInfo_Components.Add(playerInfo);
        }
        // Set Scale
        playerInfo.transform.localScale = Vector3.one;
        // Initial Info
        playerInfo.Initial(sprite, character);
        
    }
    // Rpc ? when buy Item -> Might add a new script on prefab to update UI itself.
    public void UpdateUI()
    {
        // Calculate Team 1 tower number and Info
        int tower_number = 0;
        int kill_number = 0;
        foreach (PlayerInfo_Component playerInfo_Components in Team1_PlayerInfo_Components)
        {
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
            playerInfo_Components.UpdateInfo();
            tower_number += playerInfo_Components.characterBase.tower;
        }
        Team2_Tower_Text.text = tower_number.ToString();
        Team2_Kill_Text.text = kill_number.ToString();
    }
}

}