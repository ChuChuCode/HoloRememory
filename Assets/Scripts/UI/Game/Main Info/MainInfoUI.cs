using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HR.Object.Player;

namespace HR.UI{
public class MainInfoUI : MonoBehaviour
{
    public static MainInfoUI instance;
    [Header("Image and Level")]
    public CharacterBase LocalPlayer;
    public Image Character_Image;
    public TMP_Text Level_Text;
    public Image Level;
    [SerializeField] Bar HP;
    [SerializeField] Bar MP;
    [Header("Money")]
    public TMP_Text Moeny_Text;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    void Update()
    {
        if (LocalPlayer == null) return;
    }
    public void updateInfo()
    {
        HP.SetMaxValue(LocalPlayer.maxHealth);
        HP.SetValue(LocalPlayer.currentHealth);
        MP.SetMaxValue(LocalPlayer.maxMana);
        MP.SetValue(LocalPlayer.currentMana);
        Moeny_Text.text = LocalPlayer.ownMoney.ToString();
    }
    public void Set_Level(int level)
    {
        Level_Text.text = level.ToString();
    }
    public void Set_Level_Raito(float ratio)
    {
        Level.fillAmount = ratio;
    }
}
}
