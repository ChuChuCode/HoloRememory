using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HR.Object;
using HR.Object.Player;

namespace HR.UI{
public class MainInfoUI : MonoBehaviour
{
    public static MainInfoUI instance;
    public Image Character_Image;
    public TMP_Text Level_Text;
    public Image Level;
    public Skill_Icon Q;
    public Skill_Icon W;
    public Skill_Icon E;
    public Skill_Icon R;
    [SerializeField] Bar HP;
    [SerializeField] Bar MP;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    public void updateInfo(Health health)
    {
        HP.SetMaxValue(health.maxHealth);
        HP.SetValue(health.currentHealth);
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
}
}
