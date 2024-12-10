using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace HR.Object.Player{
public class Baseball : NetworkBehaviour
{
    public CharacterSkillBase BallOwner;
    public Transform Target;
    float speed = 10f;
    [SerializeField] int AttackDamage = 20;

    // Update is called once per frame
    void Update()
    {
        if (Target == null || Target.GetComponent<Health>().currentHealth <= 0) 
        {
            Destroy(gameObject);
            return;
        }
        // Get Component center
        Vector3 Center = Target.position;
        Vector3 direction = Center - transform.position;
        transform.position += direction.normalized * speed * Time.deltaTime;
    }
    void OnTriggerEnter(Collider other) 
    {
        // Trigger need Collider and Rigid !!!!
        if (other.transform.root == Target)
        {
            Health health = other.transform.root.GetComponent<Health>();
            int exp = health.GetDamage(AttackDamage);
            // Gain exp if dead
            BallOwner.AddExp(exp);
            Destroy(gameObject);
        }
    }
}
}