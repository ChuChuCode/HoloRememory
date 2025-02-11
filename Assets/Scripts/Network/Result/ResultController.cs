using System.Collections.Generic;
using HR.Object.Player;
using HR.UI;
using TMPro;
using UnityEngine;
using Mirror;

namespace HR.Network.Result{
public class ResultController : MonoBehaviour
{
    public static ResultController Instance;
    [Header("Spawn Prefab")]
    [SerializeField] Result_Component Result_Prefab;
    [SerializeField] Transform Team1_Parent;
    [SerializeField] Transform Team2_Parent;
    [Header("Team List Info")]
    [SerializeField] List<Result_Component> Team1_Result_Components = new List<Result_Component>();
    [SerializeField] List<Result_Component> Team2_Result_Components = new List<Result_Component>();
    [Header("Tower Number")]
    [SerializeField] TMP_Text Team1_Tower_Text;
    [SerializeField] TMP_Text Team2_Tower_Text;
    [Header("Kill Number")]
    [SerializeField] TMP_Text Team1_Kill_Text;
    [SerializeField] TMP_Text Team2_Kill_Text;
    [SerializeField] GameObject Win_Text;
    [SerializeField] GameObject Lose_Text;
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
    public void Spawn_Result_Prefab(Sprite sprite,CharacterBase characterBase)
    {
        Result_Component result_Component_Temp = Instantiate(Result_Prefab,Team1_Parent.position,Quaternion.identity);
        if (characterBase.TeamID == 1)
        {
            result_Component_Temp.transform.SetParent(Team1_Parent);
        }
        else
        {
            result_Component_Temp.transform.SetParent(Team2_Parent);
        }
        result_Component_Temp.transform.localScale = Vector3.one;
        result_Component_Temp.characterBase = characterBase;
        result_Component_Temp.Initial(sprite);
    }
    public void Show_Result(int LoseTeam, int OwnTeam)
    {
        string LoseTeamString = LayerMask.LayerToName(LoseTeam).Split("Building")[0];
        if (LoseTeamString[4] == OwnTeam.ToString()[0])
        {
            Lose_Text.SetActive(true);
        }
        else
        {
            Win_Text.SetActive(true);
        }
    }
    public void Rematch(string RoomName)
    {
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