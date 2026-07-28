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
    [SerializeField] Transform PlayerList_Parent;
    [Header("Player List Info")]
    [SerializeField] List<PlayerInfo_Component> PlayerInfo_Components = new List<PlayerInfo_Component>();
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
            playerInfo.transform.SetParent(PlayerList_Parent);
            PlayerInfo_Components.Add(playerInfo);
            playerInfo.transform.localScale = Vector3.one;
            playerInfo.PlayerID = player.PlayerIdNumber;
            // Initial Info
            // playerInfo.Initial(sprite, character);
        }
        gameObject.SetActive(false);
    }
    void OnEnable()
    {
        UpdateUI();
    }
    void Bind_Character()
    {
        foreach (PlayerInfo_Component playerInfo_Components in PlayerInfo_Components)
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
        foreach (PlayerInfo_Component playerInfo_Components in PlayerInfo_Components)
        {
            if (playerInfo_Components.characterBase == null) continue;
            playerInfo_Components.UpdateInfo();
        }
    }
}

}
