using System;
using System.Collections.Generic;
using UnityEngine;
using HR.Object.Player;
using UnityEngine.UI;
using TMPro;

namespace HR.UI{
public class StorePanel : MonoBehaviour
{
    public static StorePanel Instance;
    public CharacterBase LocalPlayer;
    [Header("Equipment List")]
    [SerializeField] Transform Equipment_Content;
    // Equipment Button Item
    public StoreSlot EquipmentItemPrefab;
    // Equipment Button List
    List<StoreSlot> ListButtons = new List<StoreSlot>();
    // Scriptable Object
    public List<Equipment_ScriptableObject> ListEquipments = new List<Equipment_ScriptableObject>();
    [SerializeField] TMP_Text OwnMoney_Text;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
    void Start()
    {
        gameObject.SetActive(false);
        /// Initialize equipment
        foreach (Equipment_ScriptableObject equipment in ListEquipments)
        {
            StoreSlot tempEquipmentButton = Instantiate(EquipmentItemPrefab);
            tempEquipmentButton.UpdatePrefab(equipment);
            // Add Button to Parent
            tempEquipmentButton.transform.SetParent(Equipment_Content);
            // Add gameobject to List
            ListButtons.Add(tempEquipmentButton);
        }
    }
    void OnEnable()
    {
        if (LocalPlayer == null) return;
        // Own Money Update
        Update_Money(LocalPlayer);
    }
    public void Update_Money(CharacterBase LocalPlayer)
    {
        OwnMoney_Text.text = LocalPlayer.ownMoney.ToString();
    }
    public Equipment_ScriptableObject Search(int EquipmentIndex)
    {
        return ListEquipments.Find(item => item.EquipmentIndex == EquipmentIndex);
    }
    public void EquipmentButtonClick(Equipment_ScriptableObject tempEquipment)
    {
        if (LocalPlayer != null)
        {
            // Check if the player has enough money
            if (LocalPlayer.ownMoney < tempEquipment.costMoney) return;
            // Check if the player has enough space
            for (int i = 0; i < LocalPlayer.EquipmentSlot.Length; i++)
            {
                if (LocalPlayer.EquipmentSlot[i] == null)
                {
                    // Spend Money
                    LocalPlayer.SpendMoney(tempEquipment.costMoney);
                    // Add to EquipmentSlot
                    LocalPlayer.AddEquipItem(tempEquipment, i);
                    MainInfoUI.instance.Update_Equipment(LocalPlayer);
                    return;
                }
            }
            Debug.Log("All equipment slots are full.");
        }
    }
    public virtual void SellButtonClick(int slotIndex)
    {
        // Get costMoney
        Equipment_ScriptableObject tempEquipment = LocalPlayer.EquipmentSlot[slotIndex];
        // Refund 80% of the cost
        int refundAmount = Mathf.FloorToInt(tempEquipment.costMoney * 0.8f);
        LocalPlayer.AddMoney(refundAmount);
        LocalPlayer.DeleteEquipItem(slotIndex);
        MainInfoUI.instance.Update_Equipment(LocalPlayer);
    }
}

}