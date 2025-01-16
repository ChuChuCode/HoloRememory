using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class Skill_Button_Component : MonoBehaviour
{
    [SerializeField] Image Spell_Image ;

    public void SetSprite(Sprite sprite)
    {
        Spell_Image.sprite = sprite;
    }
}
