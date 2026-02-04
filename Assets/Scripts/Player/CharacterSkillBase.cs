using UnityEngine;
using Mirror;
using System.Collections.Generic;
using HR.UI;

namespace HR.Object.Player{
public class CharacterSkillBase : NetworkBehaviour
{
    [Header("Character Level")]
    [SerializeField] protected int Character_Level = 0;
    [SerializeField] [SyncVar(hook = nameof(Set_Exp))] protected int Character_exp = -1;
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
    protected virtual void Awake()
    {
        // Set Level exp
        Experience_List = new List<int>
        {
            0,     // Lv.1  0
            280,   // Lv.2  280
            660,   // Lv.3  380
            1140,  // Lv.4  480
            1720,  // Lv.5  580
            2400,  // Lv.6  680
            3180,  // Lv.7  780
            4060,  // Lv.8  880
            5040,  // Lv.9  980
            6120,  // Lv.10 1080
            7300,  // Lv.11 1180
            8580,  // Lv.12 1280
            9960,  // Lv.13 1380
            11440, // Lv.14 1480
            13020, // Lv.15 1580
            14700, // Lv.16 1680
            16480, // Lv.17 1780
            18360  // Lv.18 1880
        };
    }
    // Calculate Level
    int Detect_Level()
    {
        for (int level = 1 ; level < Experience_List.Count ;level++)
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
        if (isServer) this.Character_exp += exp;
        else if (isClient) CmdAddExp(exp);
        // int new_Level = Detect_Level();
        // print("Character_Level : " + Character_Level );
        // print("Character_exp : " + Character_exp );
        // print("new_Level : " + new_Level );
        // Level Up Detect Up
        // if (Character_Level != new_Level || Q_Level + W_Level + E_Level + R_Level != Character_Level)
        // {
        //     Character_Level = new_Level;
        //     // Show Level Up button
        //     MainInfoUI.instance.Show_LevelUp(this);
        //     // Set Level UI
        //     MainInfoUI.instance.Set_Level(Character_Level);
        // }
        // float ratio = Exp_Ratio(Character_Level);
        // // Set EXP RATIO UI
        // MainInfoUI.instance.Set_Level_Raito(ratio);
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
    [Command]
    void CmdAddExp(int exp)
    {
        this.Character_exp += exp;
    }
    public void Set_Exp(int OldValue,int NewValue)
    {
        int new_Level = Detect_Level();
        if (Character_Level != new_Level || Q_Level + W_Level + E_Level + R_Level != Character_Level)
        {
            Character_Level = new_Level;
            if (!isOwned) return;
            // Show Level Up button
            // Set Level UI
            MainInfoUI.instance.Set_Level(Character_Level);
        }
        if (!isOwned) return;
        // print("Set_Exp::Character_Level : " + Character_Level);
        float ratio = Exp_Ratio(Character_Level);
        // print("Set_Exp::ratio : " + ratio);
        // Set EXP RATIO UI
        MainInfoUI.instance.Set_Level_Raito(ratio);
    }
    }

}