using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using HR.Network.Select;
using UnityEngine.SocialPlatforms;

namespace HR.UI {
public class Spell_Button_Component : MonoBehaviour
{
    public int spell_Slot;
    public int spell_Index;
    [SerializeField] Image Spell_Image ;
    public void SetSprite(Sprite sprite)
    {
        Spell_Image.sprite = sprite;
    }
    public void SetIndex(int spell_Slot, int spell_Index)
    {
        this.spell_Slot = spell_Slot;
        this.spell_Index = spell_Index;
    }
    // Button Click
    public void ButtonClick()
    {
        // Set Selected
        SelectController.Instance.SelectSpell(spell_Slot, spell_Index);
    }
}

}