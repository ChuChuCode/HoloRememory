using HR.Object.Player;
using UnityEngine;

namespace HR.UI{
public class StatusController : MonoBehaviour
{
    public static StatusController Instance; 
    [SerializeField] GameObject InGameStatus;
    [SerializeField] GameObject InGameKill;
    public CharacterBase characterBase;
    void Awake()
    {
        if (Instance == null) Instance = this;
    }
    void Start()
    {
        gameObject.SetActive(false);
    }
}

}
