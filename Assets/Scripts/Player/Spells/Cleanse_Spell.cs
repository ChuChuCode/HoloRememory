using HR.Object.Player;
using UnityEngine;

namespace HR.Object.Spell{
    
[CreateAssetMenu(fileName = "Cleanse Spell", menuName = "HoloRememory/Game/Spell/Cleanse Spell")]
public class Cleanse_Spell : SpellBase
{
    public override void SpellKeyDown(CharacterBase characterBase)
    {
        // Check Cool Down
        if (Time.time - lastUseTime < cooldownDuration)
        {
            return;
        }
    }
    public override void SpellKeyUp(CharacterBase characterBase)
    {
        if (Time.time - lastUseTime < cooldownDuration)
        {
            return;
        }
        lastUseTime = Time.time; // Update the last use time
        CharacterInfoChange(characterBase);
    }
    public override void CharacterInfoChange(CharacterBase characterBase)
    {
        characterBase.ManaRegen(50);
        // Range Effect
        // Collider[] hitColliders = Physics.OverlapSphere(characterBase.mouseProject, radius, Layer_Enemy);
    }
}

}