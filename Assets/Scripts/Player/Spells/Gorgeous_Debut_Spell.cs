using HR.Object.Equipment;
using HR.Object.Player;
using UnityEngine;

namespace HR.Object.Spell{
    
[CreateAssetMenu(fileName = "Gorgeous Debut", menuName = "HoloRememory/Game/Spell/Gorgeous Debut")]
public class Gorgeous_Debut_Spell : SpellBase
{
    public ItemUse_UI UI_Prefab;
    protected ItemUse_UI UI_Object;    
    public override void SpellKeyDown(CharacterBase characterBase)
    {
        // Check Cool Down
        if (Time.time - lastUseTime < cooldownDuration)
        {
            return;
        }
        // Spawn prefab
        if (UI_Prefab != null)
        {
            UI_Object = Instantiate(UI_Prefab, characterBase.mouseProject, Quaternion.identity);
            UI_Object.characterBase = characterBase;
        }
    }
    public override void SpellKeyUp(CharacterBase characterBase)
    {
        // Delete prefab
        if (UI_Object != null)
        {
            lastUseTime = Time.time; // Update the last use time
            characterBase.agent.Warp(UI_Object.transform.position);
            Destroy_prefab();
        }
    }
    public override void Destroy_prefab()
    {
        Destroy(UI_Object.gameObject);
    }
    public override void CharacterInfoChange(CharacterBase characterBase)
    {
        
    }
}

}