using HR.Object.Player;
using UnityEngine;

namespace HR.Object.Spell{
public class SpellBase : ScriptableObject
{
    public Sprite Spell_Sprite; 
    public int SpellIndex;
    [Header("Cooldown")]
    public float cooldownDuration ;
    public float lastUseTime;
    public virtual void ItemKeyDown(CharacterBase characterBase)
    {
        
    }
    public virtual void ItemKeyUp(CharacterBase characterBase)
    {
        
    }
    public virtual void CharacterInfoChange(CharacterBase characterBase)
    {
        
    }
}

}