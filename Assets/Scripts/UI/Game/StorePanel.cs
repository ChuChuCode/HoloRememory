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
    public Equipment_Prefab EquipmentItemPrefab;
    // Equipment Button List
    List<Equipment_Prefab> ListButtons = new List<Equipment_Prefab>();
    // Scriptable Object
    public List<Equipment_Component> ListEquipments = new List<Equipment_Component>();
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
        /// Set Equipment
        foreach (Equipment_Component equipment in ListEquipments)
        {
            Equipment_Prefab tempEquipmentButton = Instantiate(EquipmentItemPrefab);
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
    public Equipment_Component Search(int EquipmentIndex)
    {
        return ListEquipments.Find(item => item.EquipmentIndex == EquipmentIndex);
    }
    public void EquipmentButtonClick(int EquipmentIndex)
    {
        Equipment_Component tempEquipment = Search(EquipmentIndex);
        if (LocalPlayer.ownMoney >= tempEquipment.costMoney)
        {
            // Spend
            LocalPlayer.ownMoney -= tempEquipment.costMoney;
            // Add to Equipment
            Image slot = Array.Find(MainInfoUI.instance.EquipmentImage,item => item.sprite == null);
            slot.sprite = tempEquipment.EquipmentImage;
        }
    }
}

}