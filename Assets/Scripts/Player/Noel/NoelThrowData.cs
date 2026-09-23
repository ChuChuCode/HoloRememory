using UnityEngine;

namespace HR.Object.Player{
[CreateAssetMenu(fileName = "Noel_MuscleThrow", menuName = "HoloBomber/Skill/Noel Muscle Throw")]
public class NoelThrowData : SkillData
{
    [Tooltip("How many cells forward the bomb gets thrown - and, if that's blocked, how many cells backward it bounces instead.")]
    public int ThrowDistance = 2;
    [Tooltip("How long the throw's arc takes, start to landing.")]
    public float ThrowDuration = 0.35f;
    [Tooltip("Peak height of the throw arc.")]
    public float ArcHeight = 1.2f;
}
}
