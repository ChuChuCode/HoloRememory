using Mirror;
using HR.UI;
using HR.Object.Player;
using HR.Object.Map;
using HR.Object.Minion;

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
            // Minion or Tower
            if (health is Minions)
            {
                // Add Minion number
                BallOwner.minion++;
                if (BallOwner.GetComponent<NetworkIdentity>().isLocalPlayer)
                {
                    CharacterInfoPanel.Instance.UpdateUI();
                    LocalPlayerInfo.Instance.Update_KDA(BallOwner);
                }
            }
            else if (health is TowerBehaviour)
            {
                // Add Destory Tower number
                BallOwner.tower++;
                if (BallOwner.GetComponent<NetworkIdentity>().isLocalPlayer)
                {
                    CharacterInfoPanel.Instance.UpdateUI();
                    LocalPlayerInfo.Instance.Update_KDA(BallOwner);
                }
            }
        }
    }
    // Since attack damage will change while game playing, so we need to set it when we create the ball.
    public void Set_AttackDamage()
    {
        AttackDamage = BallOwner.attack;
    }
}
}