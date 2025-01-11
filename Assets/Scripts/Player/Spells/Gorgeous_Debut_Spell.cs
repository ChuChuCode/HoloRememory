using HR.Object.Player;
using UnityEngine;

namespace HR.Object.Spell{
    
[CreateAssetMenu(fileName = "Gorgeous Debut", menuName = "HoloRememory/Game/Spell/Gorgeous Debut")]
public class Gorgeous_Debut_Spell : SpellBase
{
    public override void ItemKeyDown(CharacterBase characterBase)
    {
        // Spawn prefab

    }
    public override void ItemKeyUp(CharacterBase characterBase)
    {
        // Delete prefab
        characterBase.agent.Warp(UI_Object.transform.position);
    }
    public override void CharacterInfoChange(CharacterBase characterBase)
    {
        
    }
}

}