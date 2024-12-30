using UnityEngine;
using HR.Object.Player;

namespace HR.UI
{
    [CreateAssetMenu(fileName = "NewItem", menuName = "HoloRememory/Game/Item")]
    public class Item_ScriptableObject : Equipment_ScriptableObject
    {
        public int attackValue;
        public int defenseValue;
        [Header("Skill")]
        public GameObject UI_Prefab;
        GameObject UI_Object;
        public override void ItemKeyDown(CharacterBase characterBase)
        {
            if (UI_Prefab != null)
            {
                UI_Object = Instantiate(UI_Prefab, characterBase.mouseProject, Quaternion.identity);
            }
        }

        public override void ItemKeyUp(CharacterBase characterBase)
        {
            if (UI_Object != null)
            {
                // Use the item logic here
                Destroy(UI_Object);
            }
        }

        public override void CharacterInfoChange(CharacterBase characterBase)
        {
            characterBase.attack += attackValue;
            characterBase.defense += defenseValue;
        }
    }
    
}
