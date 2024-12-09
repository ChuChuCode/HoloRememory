using UnityEngine;
using Mirror;
using System.Collections.Generic;
using HR.UI;

namespace HR.Object.Player{
public class CharacterSkillBase : NetworkBehaviour
{
    [Header("Character Level")]
    [SerializeField] protected int Character_Level = 1;
    protected int Character_exp = 0;
    [Header("Level Experience")]
    [SerializeField] protected List<int> Experience_List ;
    [Header("Skill Level")]
    [SyncVar] public int Q_Level = 0;
    [SyncVar] public int W_Level = 0;
    [SyncVar] public int E_Level = 0;
    [SyncVar] public int R_Level = 0;
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
    int Detect_Level()
    {
        for (int level = 0 ; level < Experience_List.Count ;level++)
        {
            print(Character_exp);
            if (Character_exp <= Experience_List[level])
            {
                return level + 1;
            }
        }
        return Experience_List.Count ;
    }
    public void AddExp(int exp)
    {
        Character_exp += exp;
        Character_Level = Detect_Level();
        // Set UI
        MainInfoUI.instance.Set_Level(Character_Level);
    }
    /// Let Character override show Q LevelUp or not
    public virtual bool Q_Show_LevelUp()
    {
        return true;
    }
    /// Let Character override show W LevelUp or not
    public virtual bool W_Show_LevelUp()
    {
        return true;
    }
    /// Let Character override show E LevelUp or not
    public virtual bool E_Show_LevelUp()
    {
        return true;
    }
    /// Let Character override show R LevelUp or not
    public virtual bool R_Show_LevelUp()
    {
        return true;
    }
}

}