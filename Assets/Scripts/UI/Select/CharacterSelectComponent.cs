using HR.Object.Player;
using UnityEngine;

namespace HR.Network.Select{
[CreateAssetMenu(fileName = "CharacterSelectComponent", menuName = "HoloRememory/SelectMenu/CharacterComponent", order = 1)]
public class CharacterSelectComponent : ScriptableObject
{
    public int ID;
    public string CharacterName;
    [Header("Sprite")]
    public Sprite CharacterImage;
    public AudioClip SelectAudio;
    public CharacterBase CharacterModel;
}

}