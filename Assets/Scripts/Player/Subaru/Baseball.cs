using UnityEngine;
using Mirror;

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
            Health health = other.transform.root.GetComponent<Health>();
            bool isdead = health.GetDamage(AttackDamage);
            // Add Money
            if (isdead)
            {
                BallOwner.AddMoney(health.coin);
            }
            Destroy(gameObject);
        }
    }
    public void Set_AttackDamage()
    {
        AttackDamage = BallOwner.attack;
    }
}
}