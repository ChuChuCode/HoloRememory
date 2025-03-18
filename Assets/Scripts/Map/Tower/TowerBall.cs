using HR.Object.Player;
using HR.UI;
using Mirror;
using UnityEngine;

namespace HR.Object.Skill{
public class TowerBall : ProjectileBase
{
    protected override void TriggerisPlayer(CharacterBase characterBase)
    {
        // Check is dead or not
        bool isdead = characterBase.GetDamage(AttackDamage);
        if (isdead)
        {
            // Update KDA death + 1
            characterBase.AddKDA("death");
        }
    }
    // protected override void TriggerisnotPlayer(Health health)
    // {
    //     // Just Dead
    // }

}

}
