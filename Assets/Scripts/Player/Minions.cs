using UnityEngine;
using Mirror;

namespace HR.Object.Minion{
public class Minions : MinionBase
{
    // Constant string
    const string WALK = "Walk";
    const string CHASE = "Chase";
    const string ATTACK = "Attack";
    const string DEAD = "Dead";
    [SerializeField] MinionAnimationMethod minionAnimationMethod;
    protected override void Start()
    {
        if (!isServer) return;
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
        if (!isServer) return;
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
            Target = hitColliders[0].transform.root;
            current_State = CHASE;
            return;
        }
        animator.SetBool("isAttack",false);
        // Set to Final Destination
        agent.SetDestination(MainDestination.position);
    }
    void State_Chase()
    {
        // Check Target
        if (Target == null || Target.GetComponent<Health>().currentHealth <= 0 ) 
        {
            Target = null;
            current_State = WALK;
            return;
        }
        // Check Distance
        float distance = Vector3.Distance(transform.position, Target.position);
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
        animator.SetBool("isAttack",false);
        agent.SetDestination(Target.position);
    }
    void State_Attack()
    {
        // Check Target
        if (Target == null || Target.GetComponent<Health>().currentHealth <= 0) 
        {
            Target = null;
            current_State = WALK;
            return;
        }
        // Check Distance
        float distance = Vector3.Distance(transform.position, Target.position);
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
        Vector3 FaceVector = Target.position - transform.position;
        FaceVector.y = 0;
        transform.LookAt(transform.position + FaceVector );
        // Attack
        minionAnimationMethod.Target = Target;
        animator.SetBool("isAttack",true);
    }
    void State_Dead()
    {
        if (timer == deadTime) animator.Play("Dead");
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
    [Server]
    public override void CmdSetlHealth(int NewHealth)
    {
        currentHealth = NewHealth;
    }
    public override void InitialHealth()
    {
        if (!isServer) return;
        CmdSetlHealth(maxHealth);
    }
}
    
}