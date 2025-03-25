using UnityEngine;
using Mirror;
using HR.Object.Player;
using HR.Object.Map;
using HR.Object.Minion;

namespace HR.Object.Skill{
public abstract class ProjectileBase : NetworkBehaviour
{
    public Transform Target;
    [SerializeField] protected float speed;
    [SerializeField] protected int AttackDamage;

    void Update()
    {
        if (!isServer) return;
        if (Target == null || Target.GetComponent<Health>().currentHealth <= 0) 
        {
            NetworkServer.Destroy(gameObject);
            return;
        }
        // Get Component center
        Collider collider = Target.GetComponentInChildren<Collider>();
        Vector3 Center = collider.bounds.center;
        // Chase Target
        Vector3 direction = Center - transform.position;
        transform.position += direction.normalized * speed * Time.deltaTime;
    }
    [ServerCallback]
    void OnTriggerEnter(Collider other) 
    {
        if (other.transform.root == Target)
        {
            Health health = other.transform.root.GetComponent<Health>();
            if (health is CharacterBase )
            {
                CharacterBase character = health as CharacterBase;
                bool isdead = character.GetDamage(AttackDamage);
                if (isdead)
                {
                    TriggerCharacterBaseDead(character);
                }
            }
            else if (health is MinionBase)
            {
                MinionBase minion = health as MinionBase;
                bool isdead = minion.GetDamage(AttackDamage);
                if (isdead)
                {
                    TriggerMinionBaseDead(minion);
                }
            }
            else if (health is TowerBase)
            {
                TowerBase tower = health as TowerBase;
                bool isdead = tower.GetDamage(AttackDamage);
                if (isdead)
                {
                    TriggeTowerBaseDead(tower);
                }
            }
            // Destory Ball
            NetworkServer.Destroy(gameObject);
        }
    }
    protected virtual void TriggerCharacterBaseDead(CharacterBase characterBase)
    {
        characterBase.AddKDA("death");
    }
    protected virtual void TriggerMinionBaseDead(MinionBase minion)
    {
        if (minion is Minions)
        {
            // CharacterBase.AddKDA("minion");
            // CharacterBase.AddMoney(minion.coin);
        }
    }
    protected virtual void TriggeTowerBaseDead(TowerBase tower)
    {
        // CharacterBase.AddKDA("tower");
        // CharacterBase.AddMoney(tower.coin);
    }
}

}