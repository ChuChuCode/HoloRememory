using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterSelectComponent", menuName = "SelectMenu/CharacterComponent", order = 1)]
public class CharacterSelectComponent : ScriptableObject
{
    public int ID;
    public string CharacterName;

    public Sprite CharacterImage;
    public AudioClip SelectAudio;

}
