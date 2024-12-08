using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HR.Object.Player{
public class SubaruSkill : CharacterSkillBase
{
    public override bool Q_Show_LevelUp()
    {
        return true;
    }
    public override bool W_Show_LevelUp()
    {
        return true;
    }
    public override bool E_Show_LevelUp()
    {
        return true;
    }
    public override bool R_Show_LevelUp()
    {
        if(R_Level == 0 && Character_Level == 6) return true;
        else if (R_Level == 1 && Character_Level == 11) return true;
        else return false;
    }
}

}