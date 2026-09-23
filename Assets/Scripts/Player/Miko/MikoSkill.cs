using UnityEngine;

namespace HR.Object.Player{
// Blaze: for WindowSeconds after activating, EVERY bomb Miko places leaves
// lingering fire behind when it explodes (see CharacterBase.bombFireExpiryTime
// / CmdSpawnBomb, BombBase.SetLeavesLingeringFire / LingeringFire) - not
// just the next one. Always succeeds - the SP cost pays for the window up
// front, not a conditional effect like most other skills.
public class MikoSkill : CharacterSkillBase
{
    MikoBlazeData BlazeData => data as MikoBlazeData;
    float WindowSeconds => BlazeData != null ? BlazeData.WindowSeconds : 5f;

    protected override bool Activate()
    {
        owner.bombFireExpiryTime = Time.time + WindowSeconds;
        owner.bombFireLingerDuration = BlazeData != null ? BlazeData.LingerDuration : 0.75f;
        owner.bombFireTickInterval = BlazeData != null ? BlazeData.TickInterval : 0.25f;
        return true;
    }
}
}
