using HR.Object.Player;
using UnityEngine;

namespace HR.Object.Spell{
public class SpellBase : ScriptableObject
{
    public int SpellIndex;
    public GameObject UI_Prefab;
    protected GameObject UI_Object;
    [Header("Cooldown")]
    public float cooldownDuration = 5f; // Cooldown duration in seconds
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