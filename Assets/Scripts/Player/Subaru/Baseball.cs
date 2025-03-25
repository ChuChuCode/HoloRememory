using Mirror;
using HR.UI;
using HR.Object.Player;
using HR.Object.Map;
using HR.Object.Minion;

namespace HR.Object.Skill{
public class Baseball : ProjectileBase
{
    public CharacterBase BallOwner;
    protected int attack_radius;
    protected override void TriggerCharacterBaseDead(CharacterBase characterBase)
    {
        // Gain Money
        BallOwner.AddMoney(characterBase.coin);
        // kill number +1
        BallOwner.AddKDA("kill");
        base.TriggerCharacterBaseDead(characterBase);
    }

    protected override void TriggerMinionBaseDead(MinionBase minion)
    {
        BallOwner.AddMoney(minion.coin);
        if (minion is Minions)
        {
            BallOwner.AddKDA("minion");
        }
    }
    protected override void TriggeTowerBaseDead(TowerBase tower)
    {
        BallOwner.AddKDA("tower");
        BallOwner.AddMoney(tower.coin);
    }
    // Since attack damage will change while game playing, so we need to set it when we create the ball.
    public void Set_AttackDamage()
    {
        AttackDamage = BallOwner.attack;
    }
}
}