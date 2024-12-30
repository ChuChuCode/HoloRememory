using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace HR.UI{
public class StoreSlot : MonoBehaviour
{
    public Equipment_ScriptableObject equipment;
    public Image EquipmentImage;
    public TMP_Text CostMoney_Text;
    public void UpdatePrefab(Equipment_ScriptableObject equipmentSO)
    {
        equipment = equipmentSO;
        EquipmentImage.sprite = equipment.EquipmentImage;
        CostMoney_Text.text = equipment.costMoney.ToString();
    }
    public void ButtonClick()
    {
        StorePanel.Instance.EquipmentButtonClick(equipment);
    }
    
}

}