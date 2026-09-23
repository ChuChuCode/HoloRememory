using UnityEngine;

namespace HR.Object.Player{
[CreateAssetMenu(fileName = "Kronii_TimeRewind", menuName = "HoloBomber/Skill/Kronii Time Rewind")]
public class KroniiRewindData : SkillData
{
    [Tooltip("How far back (seconds) her recorded position trail reaches - Activate() rewinds to the oldest sample still within this window.")]
    public float RecordWindowSeconds = 3f;
    [Tooltip("How long the rewind's visual slide takes.")]
    public float RewindDuration = 0.3f;
}
}
