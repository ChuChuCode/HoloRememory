using UnityEngine;

namespace HR.Object.Player{
// Base data for a character's active skill. No cooldown field - skills are
// gated by SP, which passively regenerates over time (see
// CharacterSkillBase.RegenRoutine), not by a timer of its own.
public class SkillData : ScriptableObject
{
    public string SkillName;
    [Tooltip("Shown on the Lobby character-detail preview and the in-match skill HUD.")]
    public Sprite SkillIcon;
    [TextArea]
    [Tooltip("Shown on the local player's skill HUD - what the skill actually does.")]
    public string Description;
    [Tooltip("Bar size, in SP units (displayed as current/MaxEnergy, e.g. 10/10). Same bar for every character - only Cost below actually differs per skill.")]
    public float MaxEnergy = 10f;
    [Tooltip("SP required to activate this skill - spent on use, not a full-bar reset. Defaults to MaxEnergy (needs a full bar) until tuned per skill.")]
    public float Cost = 10f;
    [Tooltip("SP regenerated per RegenInterval - 1 (10% of a 10-unit bar) every 3s by default, so a full refill from empty takes 30s.")]
    public float RegenAmount = 1f;
    [Tooltip("Seconds between each passive SP regen tick.")]
    public float RegenInterval = 3f;
    [Tooltip("Delay between activating the skill and it actually happening - reserved for a future wind-up animation. 0 (instant) for every P0 skill.")]
    public float WindUpTime = 0f;
}
}
