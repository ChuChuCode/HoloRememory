using Mirror;
using HR.UI;
using HR.Object.Player;

namespace HR.Object.Skill{
public class Baseball : ProjectileBase
{
    public CharacterBase BallOwner;
    protected override void TriggerisPlayer(CharacterBase characterBase)
    {
        // Check is dead or not
        bool isdead = characterBase.GetDamage(AttackDamage);
        if (isdead)
        {
            // Add Money
            BallOwner.AddMoney(characterBase.coin);
            // Update KDA
            characterBase.death++;
            BallOwner.kill++;
            if (characterBase.GetComponent<NetworkIdentity>().isLocalPlayer)
            {
                LocalPlayerInfo.Instance.Update_KDA(characterBase);
            }
        }
    }
    protected override void TriggerisnotPlayer(Health health)
    {
        // Check is dead or not
        bool isdead = health.GetDamage(AttackDamage);
        if (isdead)
        {
            // Add Money
            BallOwner.AddMoney(health.coin);
            // Minion or Tower *****
        }
    }
    // Since attack damage will change while game playing, so we need to set it when we create the ball.
    public void Set_AttackDamage()
    {
        AttackDamage = BallOwner.attack;
    }
}
}