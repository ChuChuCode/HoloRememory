using System;
using System.Collections.Generic;
using HR.Object.Player;
using UnityEngine;

namespace HR.Network.Select{
// Named portrait variant (expression/pose) for dialogue - e.g. "Normal",
// "Happy", "Sad". Looked up by name rather than wiring a raw Sprite
// per-line in every DialogueLine, so the same art asset isn't re-dragged
// into every script that uses it, and swapping art later only means
// editing it here.
[Serializable]
public class PortraitExpression
{
    public string Name;
    public Sprite Sprite;
}

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
    // Dialogue portrait variants - empty until real art exists for this
    // character. DialogueLine references one by name (see GetPortrait).
    [Header("Dialogue Portraits")]
    public List<PortraitExpression> Portraits = new List<PortraitExpression>();

    // "" or no match falls back to the first entry (if any) rather than
    // null, so a DialogueLine that doesn't care about expression still
    // shows SOMETHING as long as at least one portrait is set up.
    public Sprite GetPortrait(string expressionName)
    {
        if (Portraits.Count == 0) return null;
        if (!string.IsNullOrEmpty(expressionName))
        {
            PortraitExpression match = Portraits.Find(p => p.Name == expressionName);
            if (match != null) return match.Sprite;
        }
        return Portraits[0].Sprite;
    }
}

}