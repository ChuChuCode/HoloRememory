using HR.Object.Equipment;
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

    public virtual void SpellKeyDown(CharacterBase characterBase)
    {
        
    }
    public virtual void SpellKeyUp(CharacterBase characterBase)
    {
        
    }
    public virtual void CharacterInfoChange(CharacterBase characterBase)
    {
        
    }
    public virtual void Destroy_prefab()
    {
        
    }
    public virtual void Set_Initial()
    {
        lastUseTime = - cooldownDuration;
    }
}

}