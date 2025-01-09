using UnityEngine;
using HR.Object.Player;
using HR.Object.Equipment;

namespace HR.UI
{
    [CreateAssetMenu(fileName = "NewItem", menuName = "HoloRememory/Game/Item")]
    public class Item_ScriptableObject : Equipment_ScriptableObject
    {
        [Header("Value Add")]
        public int attackValue;
        public int defenseValue;
        public float moveSpeedValue;
        public float attackSpeedValue;
        [Header("Skill")]
        public ItemUse_UI UI_Prefab;
        ItemUse_UI UI_Object;
        [Header("Cooldown")]
        public float cooldownDuration = 5f; // Cooldown duration in seconds
        public float lastUseTime;
        public override void ItemKeyDown(CharacterBase characterBase)
        {
            // Check Cool Down
            if (Time.time - lastUseTime < cooldownDuration)
            {
                return;
            }

            if (UI_Prefab != null)
            {
                UI_Object = Instantiate(UI_Prefab, characterBase.mouseProject, Quaternion.identity);
                UI_Object.characterBase = characterBase;
            }
        }

        public override void ItemKeyUp(CharacterBase characterBase)
        {
            if (UI_Object != null)
            {
                lastUseTime = Time.time; // Update the last use time
                // Use the item logic here
                Destroy_prefab();
                UI_Object.Spawn_ItemUse();
            }
        }
        public void Destroy_prefab()
        {
            Destroy(UI_Object.gameObject);
        }

        public override void CharacterInfoChange(CharacterBase characterBase)
        {
            characterBase.attack += attackValue;
            characterBase.defense += defenseValue;
            if (moveSpeedValue != 0)
            {
                characterBase.moveSpeed += moveSpeedValue;
            }
            if (attackSpeedValue != 0)
            {
                characterBase.attackSpeed += attackSpeedValue;
            }
        }
    }
    
}
