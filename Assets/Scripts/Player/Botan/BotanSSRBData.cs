using UnityEngine;

namespace HR.Object.Player{
[CreateAssetMenu(fileName = "Botan_SSRB", menuName = "HoloBomber/Skill/Botan SSRB")]
public class BotanSSRBData : SkillData
{
    [Tooltip("Grid cells SSRB chases before it stops and arms, regardless of whether it still has a live target.")]
    public int MaxChaseDistance = 3;
    [Tooltip("Seconds between each 1-cell hop while chasing - not numerically specified by the design doc, tune to taste.")]
    public float MoveInterval = 0.3f;
    [Tooltip("Fuse time once SSRB stops moving and arms - NOT counted down while it's still chasing.")]
    public float FuseTime = 1.5f;
}
}
