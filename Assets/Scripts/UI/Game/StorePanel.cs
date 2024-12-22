using UnityEngine;
using HR.Object.Player;
using TMPro;
using Unity.VisualScripting;

namespace HR.UI{
public class StorePanel : MonoBehaviour
{
    public static StorePanel Instance;
    public CharacterBase LocalPlayer;
    [SerializeField] TMP_Text OwnMoney_Text;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
    void Start()
    {
        gameObject.SetActive(false);
    }
    void OnEnable()
    {
        if (LocalPlayer == null) return;
        // Own Money Update
        Update_Money(LocalPlayer);
    }
    public void Update_Money(CharacterBase LocalPlayer)
    {
        OwnMoney_Text.text = LocalPlayer.ownMoney.ToString();
    }
}

}