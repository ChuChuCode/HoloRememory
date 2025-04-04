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
    [Header("Skill and HP")]
    public Skill_Icon Q;
    public Skill_Icon W;
    public Skill_Icon E;
    public Skill_Icon R;
    public Spell_Icon D;
    public Spell_Icon F;
    [SerializeField] Bar HP;
    [SerializeField] Bar MP;
    [Header("Equipment and Money")]
    public EquipmentSlot[] EquipmentImage = new EquipmentSlot[6];
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
        // Spell UI Update
        D.Set_CoolDown(LocalPlayer.Spells[0].lastUseTime,LocalPlayer.Spells[0].cooldownDuration);
        F.Set_CoolDown(LocalPlayer.Spells[1].lastUseTime,LocalPlayer.Spells[1].cooldownDuration);
        // Equipment UI Update
        for (int i = 0 ; i < LocalPlayer.EquipmentSlots.Length ; i++)
        {
            if (LocalPlayer.EquipmentSlots[i] is Item_ScriptableObject)
            {
                Item_ScriptableObject tempObject = LocalPlayer.EquipmentSlots[i] as Item_ScriptableObject;
                EquipmentImage[i].Set_CoolDown(tempObject.lastUseTime,tempObject.cooldownDuration);
            }
        }
    }
    public void updateInfo()
    {
        HP.SetMaxValue(LocalPlayer.maxHealth);
        HP.SetValue(LocalPlayer.currentHealth);
        MP.SetMaxValue(LocalPlayer.maxMana);
        MP.SetValue(LocalPlayer.currentMana);
        Moeny_Text.text = LocalPlayer.ownMoney.ToString();
    }
    public void Show_LevelUp(CharacterSkillBase skillComponent)
    {
        Q.Show_LevelUp(skillComponent);
        W.Show_LevelUp(skillComponent);
        E.Show_LevelUp(skillComponent);
        R.Show_LevelUp(skillComponent);
    }
    public void Set_Level(int level)
    {
        Level_Text.text = level.ToString();
    }
    public void Set_Level_Raito(float ratio)
    {
        Level.fillAmount = ratio;
    }
    public void Update_Equipment()
    {
        for (int i = 0 ; i < LocalPlayer.EquipmentSlots.Length ; i++)
        {
            EquipmentImage[i].UpdateImage(LocalPlayer.EquipmentSlots[i]?.EquipmentImage);
        }
    }
}
}
