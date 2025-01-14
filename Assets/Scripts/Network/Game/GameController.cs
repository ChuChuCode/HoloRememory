using UnityEngine;

using HR.Object.Player;
using HR.Object;

namespace HR.Network.Game{
public class GameController : MonoBehaviour
{
    public static GameController Instance;
    [Header("Spawn Point")]
    public Transform Team1_transform;
    public Transform Team2_transform;
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
    public CharacterBase LocalPlayer;
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
        foreach (PlayerObject player in  Manager.PlayersInfoList)
        {
            if (player.isOwned)
            {
                LocalPlayerController = player;
            }
        }
    }
    public void End_Game(int win_team)
    {
        if (win_team == 1)
        {
            if (LocalPlayer.gameObject.layer == LayerMask.NameToLayer("Team1"))
            {
                // Show Victory
            }
            else
            {
                // Show Defeat

            }
        }
        else
        {
            if (LocalPlayer.gameObject.layer == LayerMask.NameToLayer("Team2"))
            {
                // Show Victory
            }
            else
            {
                // Show Defeat

            }
        }
        // PlayerInput Disable
        
    }
}

}