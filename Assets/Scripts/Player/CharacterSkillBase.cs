using UnityEngine;
using Mirror;
using System.Collections.Generic;
using HR.UI;

namespace HR.Object.Player{
public class CharacterSkillBase : NetworkBehaviour
{
    [Header("Character Level")]
    [SerializeField] protected int Character_Level = 0;
    protected int Character_exp = 0;
    [Header("Level Experience")]
    [SerializeField] protected List<int> Experience_List ;
    [Header("Skill Level")]
    [SyncVar] public int Q_Level = 0;
    [SyncVar] public int W_Level = 0;
    [SyncVar] public int E_Level = 0;
    [SyncVar] public int R_Level = 0;
    [Header("Skill Max Level")]
    [SerializeField] protected int Q_MaxLevel ;
    [SerializeField] protected int W_MaxLevel ;
    [SerializeField] protected int E_MaxLevel ;
    [SerializeField] protected int R_MaxLevel ;
    protected virtual void Start()
    {
        // Set Level exp
        Experience_List = new List<int>
        {
            0,    // Lv.1
            280,  // Lv.2
            380,  // Lv.3
            480,  // Lv.4
            580,  // Lv.5
            680,  // Lv.6
            780,  // Lv.7
            880,  // Lv.8
            980,  // Lv.9
            1080, // Lv.10
            1180, // Lv.11
            1280, // Lv.12
            1380, // Lv.13
            1480, // Lv.14
            1580, // Lv.15
            1680, // Lv.16
            1780, // Lv.17
            1880  // Lv.18
        };
    }
    // Calculate Level
    int Detect_Level()
    {
        for (int level = 0 ; level < Experience_List.Count ;level++)
        {
            if (Character_exp < Experience_List[level])
            {
                return level;
            }
        }
        return Experience_List.Count ;
    }
    float Exp_Ratio(int level)
    {
        // the exp from level-1 to current
        int remain = Character_exp - Experience_List[level-1];
        // the exp from level-1 to level
        int gap = Experience_List[level] - Experience_List[level-1];
        return (float) remain / gap;
    }
    public void AddExp(int exp)
    {
        Character_exp += exp;
        int new_Level = Detect_Level();
        // Level Up Detect
        if (Character_Level != new_Level)
        {
            Character_Level = new_Level;
            // Show Level Up button
            MainInfoUI.instance.Show_LevelUp(this);
            // Set Level UI
            MainInfoUI.instance.Set_Level(Character_Level);
        }
        float ratio = Exp_Ratio(Character_Level);
        // Set EXP RATIO UI
        MainInfoUI.instance.Set_Level_Raito(ratio);
    }
    /// Let Character override show Q LevelUp or not
    public virtual bool Q_Show_LevelUp()
    {
        if (Q_Level == Q_MaxLevel) return false;
        return true;
    }
    /// Let Character override show W LevelUp or not
    public virtual bool W_Show_LevelUp()
    {
        if (W_Level == W_MaxLevel) return false;
        return true;
    }
    /// Let Character override show E LevelUp or not
    public virtual bool E_Show_LevelUp()
    {
        if (E_Level == E_MaxLevel) return false;
        return true;
    }
    /// Let Character override show R LevelUp or not
    public virtual bool R_Show_LevelUp()
    {
        if ( R_Level == R_MaxLevel) return false;
        if (R_Level == 0 && Character_Level == 6) return true;
        if (R_Level == 1 && Character_Level == 11) return true;
        else return false;
    }
}

}