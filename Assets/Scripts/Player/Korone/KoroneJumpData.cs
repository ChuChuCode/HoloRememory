using UnityEngine;

namespace HR.Object.Player{
[CreateAssetMenu(fileName = "Korone_Jump", menuName = "HoloBomber/Skill/Korone Jump")]
public class KoroneJumpData : SkillData
{
    [Tooltip("How many cells in front of Korone get hopped over (the cell(s) she lands beyond). v1 spec fixes this at 1, kept here instead of hardcoded so it's tunable without touching KoroneSkill.")]
    public int SkipCells = 1;
    [Tooltip("How long the hop takes, start to landing.")]
    public float JumpDuration = 0.3f;
    [Tooltip("Peak height of the jump arc.")]
    public float ArcHeight = 1f;
}
}
