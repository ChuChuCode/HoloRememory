using System.Collections;
using System.Collections.Generic;
using HR.Object;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using HR.UI;
using HR.Object.Player;

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
    [SerializeField] LayerMask Layer_Enemy;
    [SerializeField] State current_State;
    [Header("Attack")]
    [SerializeField] float max_distance = 8f;
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

        if (currentHealth <= 0) 
        {
            if (!isDead) timer = deadTime;
            Target = null;
            isDead = true;
            agent.isStopped = true;
            current_State = State.Dead;
        }
        switch (current_State)
        {
            case State.Walk:
                Collider[] hitColliders = Physics.OverlapSphere(transform.position, max_distance, Layer_Enemy);
                if(hitColliders.Length > 0)
                {
                    // Collider might be in child gameobject
                    Target = hitColliders[0].transform.root.gameObject;
                    current_State = State.Chase;
                    return;
                }
                // Set to Final Destination
                agent.SetDestination(FinalDestination.position);
                return;
            case State.Chase:
                if (Target == null)
                {
                    current_State = State.Walk;
                    return;
                }
                // Check Target
                if (Target != null && Target.GetComponent<Health>().currentHealth <= 0 ) 
                {
                    Target = null;
                    current_State = State.Walk;
                    return;
                }
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
                // Check Target
                if (Target != null && Target.GetComponent<Health>().currentHealth <= 0) 
                {
                    Target = null;
                    current_State = State.Walk;
                    return;
                }
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
                if (timer == deadTime) 
                {
                    Detect_Surround();
                }
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
    public override void GetDamage(int damage)
    {
        base.GetDamage(damage);
        healthBar.SetValue(currentHealth);

        // Update UI
        Selectable.instance.updateInfo(this);
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
        Layer_Enemy &= ~(1 << layer);
    }
    void Detect_Surround()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, max_distance, Layer_Enemy);
        foreach (Collider collider in hitColliders)
        {
            CharacterSkillBase tempSkill = collider.transform.root.GetComponent<CharacterSkillBase>();
            if (tempSkill != null)
            {
                tempSkill.AddExp(exp);
            }
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