using UnityEngine;
using HR.Object.Player;
using HR.Object.Equipment;

namespace HR.UI
{
    [CreateAssetMenu(fileName = "NewItem", menuName = "HoloRememory/Game/Item")]
    public class Item_ScriptableObject : Equipment_ScriptableObject
    {
        public int attackValue;
        public int defenseValue;
        [Header("Skill")]
        public ItemUse_UI UI_Prefab;
        ItemUse_UI UI_Object;
        [Header("Cooldown")]
        public float cooldownDuration = 5f; // Cooldown duration in seconds
        private float lastUseTime;
        public override void ItemKeyDown(CharacterBase characterBase)
        {
            if (Time.time - lastUseTime < cooldownDuration)
            {
                Debug.Log("Item is on cooldown.");
                return;
            }

            if (UI_Prefab != null)
            {
                UI_Object = Instantiate(UI_Prefab, characterBase.mouseProject, Quaternion.identity);
                UI_Object.characterBase = characterBase;
                lastUseTime = Time.time; // Update the last use time
            }
        }

        public override void ItemKeyUp(CharacterBase characterBase)
        {
            if (UI_Object != null)
            {
                // Use the item logic here
                Destroy(UI_Object.gameObject);
            }
        }

        public override void CharacterInfoChange(CharacterBase characterBase)
        {
            characterBase.attack += attackValue;
            characterBase.defense += defenseValue;
        }
    }
    
}
