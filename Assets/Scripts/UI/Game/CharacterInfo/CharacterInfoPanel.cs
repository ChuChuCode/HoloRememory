using UnityEngine;
using HR.Network;
using HR.Object.Player;
using HR.Network.Select;
using System.Collections.Generic;

namespace HR.UI{
public class CharacterInfoPanel : MonoBehaviour
{
    public static CharacterInfoPanel Instance;
    [Header("Spawn Prefab")]
    [SerializeField] PlayerInfo_Component PlayerInfo_Component;
    [SerializeField] Transform Team1_Parent;
    [SerializeField] Transform Team2_Parent;
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
        }
        else
        {
            playerInfo.transform.SetParent(Team2_Parent);
        }
        // Set Scale
        playerInfo.transform.localScale = Vector3.one;
        // Initial Info
        playerInfo.Initial(sprite, character);
        PlayerInfo_Components.Add(playerInfo);
    }
    // Need Rpc *****************************
    public void UpdateUI()
    {
        foreach (PlayerInfo_Component playerInfo_Component in PlayerInfo_Components)
        {
            playerInfo_Component.UpdateInfo();
        }
    }
}

}