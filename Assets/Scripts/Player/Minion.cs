using System.Collections;
using System.Collections.Generic;
using HR.Object;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using HR.UI;

namespace HR.Object.Minion{
public class Minion : Health
{
    enum State{
        Walk,
        Chase,
        Attack,
        Dead
    };
    NavMeshAgent agent;
    [SerializeField] Animator animator;
    [Header("Final Destination")]
    public Transform FinalDestination;
    [SerializeField] GameObject Target;
    int Layer_Enemy;
    [SerializeField] State current_State;
    [Header("Attack")]
    [SerializeField] float max_distance = 7f;
    [SerializeField] float attack_radius = 5f;
    float distance;
    [Header("Dead")]
    [SerializeField] float deadTime = 1f;
    float timer = 1f;
    [SerializeField] Bar healthBar;
    void Start()
    {
        // Outline NavMeshAgent Check
        if (!TryGetComponent<NavMeshAgent>(out agent))
        {
            Debug.LogError("CharacterBase must have a NavMeshAgent Component.",agent);
        }
        // Outline Component Check
        if (!TryGetComponent<Outline>(out Outline _))
        {
            Debug.LogError("CharacterBase must have a Outline Component.");
        }
        current_State = State.Walk;InitialHealth();
        InitialHealth();
    }

    // Update is called once per frame
    void Update()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, max_distance, Layer_Enemy);
        if (currentHealth <= 0) 
        {
            if (!isDead) timer = deadTime;
            Target = null;
            isDead = true;
            agent.isStopped = true;
            current_State = State.Dead;
        }
        else
        {
            if (Target != null && Target.GetComponent<Health>().currentHealth <= 0) 
            {
                Target = null;
                current_State = State.Walk;
            }
        }
        switch (current_State)
        {
            case State.Walk:
                if(hitColliders.Length > 0)
                {
                    Target = hitColliders[0].gameObject;
                    current_State = State.Chase;
                    return;
                }
                // Set to Final Destination
                agent.SetDestination(FinalDestination.position);
                return;
            case State.Chase:
                // Check Distance
                distance = Vector3.Distance(transform.position, Target.transform.position);
                if (distance > max_distance)
                {
                    Target = null;
                    current_State = State.Walk;
                    return;
                }
                else if (distance <= attack_radius)
                {
                    current_State = State.Attack;
                    return;
                }
                agent.SetDestination(Target.transform.position);
                return;
            case State.Attack:
                // Check Distance
                distance = Vector3.Distance(transform.position, Target.transform.position);
                if (distance > max_distance)
                {
                    Target = null;
                    current_State = State.Walk;
                    return;
                }
                else if (distance >= attack_radius)
                {
                    current_State = State.Chase;
                    return;
                }
                agent.isStopped = true;
                // Face to Target
                Vector3 FaceVector = Target.transform.position - transform.position;
                FaceVector.y = 0;
                transform.LookAt(transform.position + FaceVector );
                // Attack
                // print("attack");

                return;
            case State.Dead:
                // if (timer == deadTime) animator.Play("Dead");
                // Delete Object when timer is done
                timer -= Time.deltaTime;
                if (timer <= 0f )
                {
                    Death();
                }
                return;
        }
    }
    public override void InitialHealth()
    {
        currentHealth = maxHealth;
        healthBar.SetMaxValue(maxHealth);
    }
    public override int GetDamage(int damage)
    {
        int exp = base.GetDamage(damage);
        healthBar.SetValue(currentHealth);

        // Update UI
        Selectable.instance.updateInfo(this);
        return exp;
    }
    public override void Death()
    {
        // Set Health to 0
        currentHealth = 0;
        healthBar.SetValue(0);
        Destroy(gameObject);
    }
    public void Update_Enemy_Layer(int layer)
    {
        // If layer == Team1
        if (LayerMask.NameToLayer("Team1") == layer)
        {
            Layer_Enemy = LayerMask.NameToLayer("Team2");
        }
        // If layer == Team2
        else if (LayerMask.NameToLayer("Team2") == layer)
        {
            Layer_Enemy = LayerMask.NameToLayer("Team1");
        }
        else
        {
            Debug.Log("This Minion Set wrong Layer.");
        }
    }
    void OnDrawGizmosSelected()
    {
        // Draw a yellow sphere at the transform's position
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, max_distance);
    }
}
    
}