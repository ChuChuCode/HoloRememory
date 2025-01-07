using UnityEngine;
using Mirror;
using HR.UI;

namespace HR.Object.Player{
public class Baseball : NetworkBehaviour
{
    public CharacterBase BallOwner;
    public Transform Target;
    float speed = 10f;
    [SerializeField] int AttackDamage;

    // Update is called once per frame
    void Update()
    {
        if (Target == null || Target.GetComponent<Health>().currentHealth <= 0) 
        {
            Destroy(gameObject);
            return;
        }
        // Get Component center
        Collider collider = Target.GetComponentInChildren<Collider>();
        Vector3 Center = collider.bounds.center;
        Vector3 direction = Center - transform.position;
        transform.position += direction.normalized * speed * Time.deltaTime;
    }
    void OnTriggerEnter(Collider other) 
    {
        // Trigger need Collider and Rigid !!!!
        if (other.transform.root == Target)
        {
            bool isdead;
            // Check is player or not
            if (other.transform.root.TryGetComponent<CharacterBase>(out CharacterBase characterBase))
            {
                // Check is dead or not
                isdead = characterBase.GetDamage(AttackDamage);
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
            else 
            { 
                Health health = other.transform.root.GetComponent<Health>();
                // Check is dead or not
                isdead = health.GetDamage(AttackDamage);
                if (isdead)
                {
                    // Add Money
                    BallOwner.AddMoney(health.coin);
                    // Minion or Tower *****
                }
            }
            // Destory Ball
            Destroy(gameObject);
        }
    }
    public void Set_AttackDamage()
    {
        AttackDamage = BallOwner.attack;
    }
}
}