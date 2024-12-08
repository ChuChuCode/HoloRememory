using UnityEngine;
using Mirror;
using System.Collections.Generic;
using System;

namespace HR.Object.Player{
public class CharacterSkillBase : NetworkBehaviour
{
    [Header("Character Level")]
    [SerializeField] protected int Character_Level = 1;
    protected int Character_exp = 0;
    [Header("Level Experience")]
    [SerializeField] List<int> Experience_List = new List<int>();
    [Header("Skill Level")]
    [SyncVar] public int Q_Level = 0;
    [SyncVar] public int W_Level = 0;
    [SyncVar] public int E_Level = 0;
    [SyncVar] public int R_Level = 0;
    int Detect_Level()
    {
        for(int level = 0 ; level < Experience_List.Count ;level++)
        {
            if (Character_exp < Experience_List[level])
            {
                return level + 1;
            }
        }
        return Experience_List.Count ;
    }
    public virtual void AddExp(int exp)
    {
        Character_exp += exp;
        Character_Level = Detect_Level();
    }
    public virtual bool Q_Show_LevelUp()
    {
        return true;
    }
    public virtual bool W_Show_LevelUp()
    {
        return true;
    }
    public virtual bool E_Show_LevelUp()
    {
        return true;
    }
    public virtual bool R_Show_LevelUp()
    {
        return true;
    }
}

}