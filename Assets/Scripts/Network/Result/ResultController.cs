using System.Collections.Generic;
using HR.Object.Player;
using HR.UI;
using TMPro;
using UnityEngine;
using Mirror;
using HR.Network.Select;

namespace HR.Network.Result{
public class ResultController : MonoBehaviour
{
    public static ResultController Instance;
    [Header("Spawn Prefab")]
    [SerializeField] Result_Component Result_Prefab;
    [SerializeField] Transform PlayerList_Parent;
    [Header("Player List Info")]
    [SerializeField] List<Result_Component> Result_Components = new List<Result_Component>();
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
    public void Start()
    {
        foreach(PlayerObject player in Manager.PlayersInfoList)
        {
            Result_Component result_Component_Temp = Instantiate(Result_Prefab,PlayerList_Parent.position,Quaternion.identity);
            result_Component_Temp.playerObject = player;
            result_Component_Temp.transform.SetParent(PlayerList_Parent);
            Result_Components.Add(result_Component_Temp);
            result_Component_Temp.transform.localScale = Vector3.one;
        }
    }
    public void UpdateUI()
    {
        foreach(Result_Component result_Component in Result_Components)
        {
            CharacterSelectComponent characterModelComponent = Manager.characterSelectComponentsList.Find(component => component.ID == result_Component.playerObject.CharacterID);
            result_Component.Initial(characterModelComponent.CharacterImage);
        }
    }
    public void Rematch(string RoomName)
    {
        if (!NetworkServer.active) return;
        Manager.ChangeScene(RoomName);
    }
    public void Leave_Game()
    {
        // isServer
        if (NetworkServer.active)
        {
            Manager.StopHost();
        }
        // Client
        if (NetworkClient.active)
        {
            Manager.StopClient();
        }
        Destroy(Manager.gameObject);
    }
}

}