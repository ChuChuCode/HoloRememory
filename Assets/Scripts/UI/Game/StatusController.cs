using HR.Object.Player;
using UnityEngine;

namespace HR.UI{
public class StatusController : MonoBehaviour
{
    public static StatusController Instance; 
    [SerializeField] GameObject InGameStatus;
    [SerializeField] GameObject InGameKill;
    [SerializeField] GameObject Result_Win;
    [SerializeField] GameObject Result_Lose;
    public CharacterBase characterBase;
    void Awake()
    {
        if (Instance == null) Instance = this;
    }
    void Start()
    {
        gameObject.SetActive(false);
    }
    public void Show_Result(string LoseTowerLayer)
    {
        string LoseTeamString = LoseTowerLayer.Split("Building")[0];
        // Lose
        if (LayerMask.LayerToName(characterBase.gameObject.layer) == LoseTeamString)
        {
            Result_Lose.SetActive(true);
        }
        else
        {
            Result_Win.SetActive(true);
        }
    }
}

}
