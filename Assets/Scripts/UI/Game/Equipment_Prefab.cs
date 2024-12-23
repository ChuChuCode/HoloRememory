using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace HR.UI{
public class Equipment_Prefab : MonoBehaviour
{
    public int EquipmentIndex;
    public Image EquipmentImage;
    public TMP_Text CostMoney_Text;
    public void UpdatePrefab(Equipment_Component equipment)
    {
        EquipmentIndex = equipment.EquipmentIndex;
        EquipmentImage.sprite = equipment.EquipmentImage;
        CostMoney_Text.text = equipment.costMoney.ToString();
    }
    public void ButtonClick()
    {
        StorePanel.Instance.EquipmentButtonClick(EquipmentIndex);
    }
}

}