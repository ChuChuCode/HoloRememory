using UnityEngine;
using Mirror;
using HR.Object.Player;

namespace HR.Object.Skill{
public class ProjectileBase : NetworkBehaviour
{
    public Transform Target;
    [SerializeField] protected float speed;
    [SerializeField] protected int AttackDamage;

    void Update()
    {
        if (!isServer) return;
        if (Target == null || Target.GetComponent<Health>().currentHealth <= 0) 
        {
            Destroy(gameObject);
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
            // Check is player or not
            if (other.transform.root.TryGetComponent<CharacterBase>(out CharacterBase characterBase))
            {
                TriggerisPlayer(characterBase);
            }
            else 
            { 
                Health health = other.transform.root.GetComponent<Health>();
                TriggerisnotPlayer(health);
            }
            // Destory Ball
            NetworkServer.Destroy(gameObject);
        }
    }
    protected virtual void TriggerisPlayer(CharacterBase characterBase)
    {
        // Check is dead or not
        bool isdead = characterBase.GetDamage(AttackDamage);
        if (isdead)
        {
            // Do the things when enemy dead
        }
    }
    protected virtual void TriggerisnotPlayer(Health health)
    {
        // Check is dead or not
        bool isdead = health.GetDamage(AttackDamage);
        if (isdead)
        {
            // Do the things when enemy dead
        }
    }

}

}