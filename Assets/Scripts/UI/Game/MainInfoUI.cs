using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HR.Object;

namespace HR.UI{
public class MainInfoUI : MonoBehaviour
{
    public static MainInfoUI instance;
    public Image Character_Image;
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
    public void Show_LevelUp()
    {
        Q.Show_LevelUp();
        W.Show_LevelUp();
        E.Show_LevelUp();
        R.Show_LevelUp();
    }
}
}
