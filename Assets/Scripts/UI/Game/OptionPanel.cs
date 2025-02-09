using HR.Global;
using HR.Network;
using HR.Object.Player;
using Mirror;
using UnityEngine;

namespace HR.UI{
public class OptionPanel : MonoBehaviour
{
    public static OptionPanel Instance;
    [SerializeField] Setting_Component setting_Component;
    [SerializeField] GameObject End_Button;
    [SerializeField] GameObject Leave_Button;
    [SerializeField] GameObject Back_Button;
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
    public CharacterBase LocalPlayer;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
    void Start()
    {   
        // is Client
        if (!NetworkServer.active)   
        {
            End_Button.SetActive(false);
            Back_Button.SetActive(false);
        }
        gameObject.SetActive(false);
    }
    public void Back_To_Room(string RoomName)
    {
        Manager.ChangeScene(RoomName);
    }
    public void End_Game(string ResultScene_Name)
    {
        Manager.ChangeScene(ResultScene_Name);
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