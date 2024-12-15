using System.Collections.Generic;
using UnityEngine;
using HR.UI;
using HR.Object.Player;

namespace HR.Object.Minion{
public class Minion : MinionBase
{
    // Constant string
    const string WALK = "Walk";
    const string CHASE = "Chase";
    const string ATTACK = "Attack";
    const string DEAD = "Dead";
    protected override void Start()
    {
        current_State = WALK;
        base.Start();
    }
        protected override void Add_State()
    {
        CharacterState.Add(WALK,() => State_Walk());
        CharacterState.Add(CHASE,() => State_Chase());
        CharacterState.Add(ATTACK,() => State_Attack());
        CharacterState.Add(DEAD,() => State_Dead());
    }
    protected override void Update()
    {
        if (currentHealth <= 0) 
        {
            // if (!isDead) timer = deadTime;
            Target = null;
            isDead = true;
            agent.isStopped = true;
            current_State = DEAD;
        }
        base.Update();
    }
    void State_Walk()
    {
        agent.isStopped = false;
        //Check has enemy arround
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, Search_radius, Layer_Enemy);
        if(hitColliders.Length > 0)
        {
            // Collider might be in child gameobject
            Target = hitColliders[0].transform.root.gameObject;
            current_State = CHASE;
            return;
        }
        // Set to Final Destination
        agent.SetDestination(MainDestination.position);
    }
    void State_Chase()
    {
        if (Target == null)
        {
            current_State = WALK;
            return;
        }
        // Check Target
        if (Target != null && Target.GetComponent<Health>().currentHealth <= 0 ) 
        {
            Target = null;
            current_State = WALK;
            return;
        }
        // Check Distance
        float distance = Vector3.Distance(transform.position, Target.transform.position);
        if (distance > Search_radius)
        {
            Target = null;
            current_State = WALK;
            return;
        }
        else if (distance <= attack_radius)
        {
            current_State = ATTACK;
            return;
        }
        agent.SetDestination(Target.transform.position);
    }
    void State_Attack()
    {
        // Check Target
        if (Target != null && Target.GetComponent<Health>().currentHealth <= 0) 
        {
            Target = null;
            current_State = WALK;
            return;
        }
        // Check Distance
        float distance = Vector3.Distance(transform.position, Target.transform.position);
        if (distance > Search_radius)
        {
            Target = null;
            current_State = WALK;
            return;
        }
        else if (distance >= attack_radius)
        {
            current_State = CHASE;
            return;
        }
        agent.isStopped = true;
        // Face to Target
        Vector3 FaceVector = Target.transform.position - transform.position;
        FaceVector.y = 0;
        transform.LookAt(transform.position + FaceVector );
        // Attack
        // print("attack");
    }
    void State_Dead()
    {
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
    
}