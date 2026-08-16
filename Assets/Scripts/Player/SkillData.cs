using UnityEngine;

namespace HR.Object.Player{
// Base data for a character's active skill. No cooldown field - skills are
// gated purely by Skill Energy (filled by other actions like destroying
// blocks), not by a timer. Per-skill tunable values (jump distance, push
// force, etc.) belong on a subclass of this, not hardcoded in the
// character/skill controller.
public class SkillData : ScriptableObject
{
    public string SkillName;
    [Tooltip("Energy needed before this skill can be activated. Same 0-100 bar for every character in P0.")]
    public float MaxEnergy = 100f;
    [Tooltip("Delay between activating the skill and it actually happening - reserved for a future wind-up animation. 0 (instant) for every P0 skill.")]
    public float WindUpTime = 0f;
}
}
