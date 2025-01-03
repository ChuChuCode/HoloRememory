using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

namespace HR.UI{
public class StoreSlot : MonoBehaviour , IPointerClickHandler
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
        bool isbought = StorePanel.Instance.EquipmentButtonClick(equipment);
        if (isbought)
        {
            GetComponent<Button>().interactable = false;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Left click
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            print("Show UI");
        }
        // Right click
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            ButtonClick();
        }
    }
}

}