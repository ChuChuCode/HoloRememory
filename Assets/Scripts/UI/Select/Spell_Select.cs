using System.Collections.Generic;
using HR.Network.Select;
using UnityEngine;
using UnityEngine.UI;

namespace HR.UI{
    
public class Spell_Select : MonoBehaviour
{
    public Image Spell_Image;
    [SerializeField] int Slot_Index;
    public int spell_index;
    public GameObject Spell_Slection;
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
        // Close Another if Open
        Another_Spell.GetComponent<Spell_Select>().Spell_Slection.SetActive(false);
        // If open then close. If close then open
        Spell_Slection.SetActive(!Spell_Slection.activeSelf);
    }
    // Spell_Button_Component.ButtonClick -> SelectController.SelectSpell -> Spell_Select.Spell_Button_Click
    public void Spell_Button_Click(int new_spell_Index)
    {
        // Check if another spell is the same as select one -> Swap
        if (Another_Spell.GetComponent<Spell_Select>().spell_index == new_spell_Index)
        {
            // PlayerObject.Spell Update
            if (Slot_Index == 0)
            {
                int index = SelectController.Instance.LocalPlayerController.Spell_2;
                SelectController.Instance.LocalPlayerController.CanSpell2Change(SelectController.Instance.LocalPlayerController.Spell_1);
                SelectController.Instance.LocalPlayerController.CanSpell1Change(index);
            }
            else
            {
                int index = SelectController.Instance.LocalPlayerController.Spell_1;
                SelectController.Instance.LocalPlayerController.CanSpell1Change(SelectController.Instance.LocalPlayerController.Spell_2);
                SelectController.Instance.LocalPlayerController.CanSpell2Change(index);
            }
            // Spell Index Swap
            Another_Spell.GetComponent<Spell_Select>().spell_index = spell_index;
            // Sprite Swap
            Sprite temp =  Another_Spell.GetComponent<Spell_Select>().Spell_Image.sprite;
            Another_Spell.GetComponent<Spell_Select>().Spell_Image.sprite = Spell_Image.sprite;
            Spell_Image.sprite = temp;
        }
        else
        {
            // PlayerObject.Spell_1 Spell_2 Update
            if (Slot_Index == 0)
            {
                SelectController.Instance.LocalPlayerController.CanSpell1Change(new_spell_Index);
            }
            else
            {
                SelectController.Instance.LocalPlayerController.CanSpell2Change(new_spell_Index);
            }
            // Sprite update
            Spell_Image.sprite = SelectController.Instance.Search_Spell(new_spell_Index).Spell_Sprite;
        }
        // Set Spell Index 
        spell_index = new_spell_Index;
        // Close Panel
        Spell_Slection.SetActive(false);
    }
    void Disable_Skill_Selection(int index)
    {
        Skill_Button[index].interactable = false;
    }
}

}