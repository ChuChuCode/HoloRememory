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
    // Single source of truth for "which skill does this character use" -
    // CharacterSkillBase.data (on the actual character prefab) is
    // protected and only exists on a live/spawned instance, which the
    // Lobby's character-detail preview doesn't have (nothing's spawned
    // there yet). This lets that preview read skill name/icon/description
    // straight from data, the same way it already reads CharacterImage.
    [Header("Skill")]
    public SkillData Skill;
}

}