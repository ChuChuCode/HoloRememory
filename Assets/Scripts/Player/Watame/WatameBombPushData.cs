using UnityEngine;

namespace HR.Object.Player{
[CreateAssetMenu(fileName = "Watame_BombPush", menuName = "HoloBomber/Skill/Watame Bomb Push")]
public class WatameBombPushData : SkillData
{
    [Tooltip("Seconds of travel per cell the bomb actually moves - push distance itself comes from the caster's current Fire Power (bombPower), not a fixed value here.")]
    public float DurationPerCell = 0.1f;
}
}
