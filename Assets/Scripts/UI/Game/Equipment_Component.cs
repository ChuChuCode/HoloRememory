using UnityEngine;

namespace HR.UI{
[CreateAssetMenu(fileName = "Equipment",menuName = "HoloRememory/Game/Equipment",order = 1)]
public class Equipment_Component : ScriptableObject
{
    public int EquipmentIndex;
    public string EquipmentName;
    public Sprite EquipmentImage;
    public int costMoney;
}

}