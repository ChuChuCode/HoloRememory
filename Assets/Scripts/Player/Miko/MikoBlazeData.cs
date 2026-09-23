using UnityEngine;

namespace HR.Object.Player{
[CreateAssetMenu(fileName = "Miko_Blaze", menuName = "HoloBomber/Skill/Miko Blaze")]
public class MikoBlazeData : SkillData
{
    [Tooltip("Every bomb Miko places within this many seconds of activating leaves lingering fire - not just the next one.")]
    public float WindowSeconds = 5f;
    [Tooltip("How long the lingering fire stays on each blast tile after the explosion.")]
    public float LingerDuration = 0.75f;
    [Tooltip("How often the fire re-checks for someone standing in it and deals damage again.")]
    public float TickInterval = 0.25f;
}
}
