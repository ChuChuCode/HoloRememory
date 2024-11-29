using UnityEngine;

using HR.Object.Player;

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
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

}