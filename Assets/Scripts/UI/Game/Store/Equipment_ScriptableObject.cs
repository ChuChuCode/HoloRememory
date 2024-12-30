using HR.Global;
using HR.Object.Player;
using UnityEngine;

namespace HR.UI{

public class Equipment_ScriptableObject : ScriptableObject
{
    public int EquipmentIndex;
    public string EquipmentName;
    public Sprite EquipmentImage;
    public int costMoney;
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