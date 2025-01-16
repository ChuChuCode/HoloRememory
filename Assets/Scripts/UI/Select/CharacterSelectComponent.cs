using UnityEngine;

namespace HR.Network.Select{
[CreateAssetMenu(fileName = "CharacterSelectComponent", menuName = "HoloRememory/SelectMenu/CharacterComponent", order = 1)]
public class CharacterSelectComponent : ScriptableObject
{
    public int ID;
    public string CharacterName;
    [Header("Sprite")]
    public Sprite CharacterImage;
    public Sprite Q_skill_Image;
    public Sprite W_skill_Image;
    public Sprite E_skill_Image;
    public Sprite R_skill_Image;
    public AudioClip SelectAudio;
    public GameObject CharacterModel;
}

}