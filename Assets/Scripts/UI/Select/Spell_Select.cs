using System.Collections.Generic;
using HR.Network.Select;
using UnityEngine;
using UnityEngine.UI;

namespace HR.UI{
    
public class Spell_Select : MonoBehaviour
{
    Image Spell_Image;
    int Slot_Index;
    int spell_index;
    [SerializeField] GameObject Spell_Slection;
    [SerializeField] GameObject Another_Spell;
    [SerializeField] List<Button> Skill_Button;
    void Start()
    {
        Spell_Image = GetComponent<Image>();
        Spell_Slection.SetActive(false);
    }
    // Spell Button Click
    public void Select_Click()
    {
        Another_Spell.SetActive(false);
        Spell_Slection.SetActive(!Spell_Slection.activeSelf);
    }
    public void SetImage(int index)
    {
        // Spell_Image.sprite = sprite;
        spell_index = index;
        Disable_Skill_Selection(index);
        Set_character_info(index);
    }
    void Set_character_info(int index)
    {
        SelectController.Instance.LocalPlayerController.Spell[Slot_Index] = spell_index;
    }
    void Disable_Skill_Selection(int index)
    {
        Skill_Button[index].interactable = false;
    }
}

}