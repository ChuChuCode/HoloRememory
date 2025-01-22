using UnityEngine;
using UnityEngine.AI;
using HR.UI;
using HR.Object.Player;

namespace HR.Object.Minion{
public class Duck_AI : MinionBase
{
    // Constant string
    const string IDLE = "Idle";
    const string WALK = "Walk";
    const string CHASE = "Chase";
    const string ATTACK = "Attack";
    const string DEAD = "Dead";
    const string BACK = "Back";
    const string SPECIAL = "Special";
    [SerializeField] Rigidbody rd;
    public static float master_distance = 5f;
    float agent_speed = 3f;
    float agent_back_speed = 9f;
    Vector3 goal;
    [Header("Q Skill")]
    [SerializeField] GameObject Q_UI;
    public Vector3 rush_position;
    // This Parameter is for same position.
    //float rush_speed = 50f;
    // This Parameter is for same Force.
    float rush_speed = 500f;
    float rush_time = 2f;
    public bool rush_trigger;
    Collider[] hitColliders;
    [Header("Animator")]
    bool isMove;
    bool isAttack;
    protected override void Start()
    {
        current_State = IDLE;
        rush_trigger = false;
        base.Start();
    }
    protected override void Add_State()
    {
        CharacterState.Add(IDLE,() => State_Idle());
        CharacterState.Add(WALK,() => State_Walk());
        CharacterState.Add(CHASE,() => State_Chase());
        CharacterState.Add(ATTACK,() => State_Attack());
        CharacterState.Add(DEAD,() => State_Dead());
        CharacterState.Add(BACK,() => State_Back());
        CharacterState.Add(SPECIAL,() => State_Special());
    }
    protected override void Update()
    {
        isMove = animator.GetBool("isMove");
        isAttack = animator.GetBool("isAttack");
        // Search Enemy use sphere
        hitColliders = Physics.OverlapSphere(transform.position, Search_radius, Layer_Enemy);
        
        // if health < 0 -> to dead mode
        if (currentHealth <= 0) 
        {
            if (!isDead) timer = deadTime;
            isDead = true;
            agent.isStopped = true;
            current_State = DEAD;
        }
        else
        {
            // If far away from player -> to Back state
            if (Vector3.Distance(transform.position,MainDestination.position) >= 2* master_distance)
            {
                // if is still attack
                animator.SetBool("isAttack",false);
                current_State = BACK;
                // new position to player 
                goal = MainDestination.position + 
                    new Vector3(
                        UnityEngine.Random.Range(-1f,1f)*master_distance,
                        0f,
                        UnityEngine.Random.Range(-1f,1f)*master_distance
                    );
                // Change Speed * 5
                animator.speed = 3f;
                agent.speed = agent_back_speed; 
            }
            // Special mode interrupt
            if (rush_trigger)
            {
                animator.SetBool("isAttack",false);
                rush_position.y = 0;
                // to Special mode
                current_State = SPECIAL;
                // This Parameter is for same position.
                //rd.AddForce( (rush_position - transform.position) * rush_speed);
                // This Parameter is for same Force with normalized
                rd.AddForce( (rush_position - transform.position).normalized * rush_speed);
                timer = rush_time;
            }
        }
        base.Update();
    }
    void State_Idle()
    {
        // Has enemy nearby
        if (hitColliders.Length > 0)
        {
            // to Chase mode
            current_State = CHASE;
            return;
        }
        agent.isStopped = true;
        // turn idle if still walk
        if (isMove) animator.SetBool("isMove",false);
        // timer for idle mode
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            // find new position to idle
            goal = MainDestination.position + 
                new Vector3(
                    UnityEngine.Random.Range(-1f,1f)*master_distance,
                    0f,
                    UnityEngine.Random.Range(-1f,1f)*master_distance
                );
            // to Walk mode
            current_State = WALK;
        }
    }
    void State_Walk()
    {
        // Has enemy nearby
        if (hitColliders.Length > 0)
        {
            // to Chase mode
            current_State = CHASE;
            return;
        }
        agent.isStopped = false;
        // turn walk if still idle
        if (!isMove) animator.SetBool("isMove",true);
        // go to new position
        agent.destination = goal;
        // Close to position then to Idle mode
        if (Vector3.Distance(goal,transform.position) < 2f)
        {
            // Reset time
            timer = UnityEngine.Random.Range(1f,3f);
            // to Idle mode
            current_State = IDLE;
        }
    }
    void State_Chase()
    {
        // no enemy nearby(dead or run away)
        if (hitColliders.Length == 0)
        {
            // reset target
            Target = null;
            // Reset time
            timer = UnityEngine.Random.Range(1f,3f);
            // to Idle mode
            current_State = IDLE;
            return;
        }
        agent.isStopped = false;
        // Search target
        if (Target == null)
        { 
            Target = Search_Nearest(hitColliders);
        }
        // If is in attack Range -> to attack state
        if (Vector3.Distance(Target.transform.position,transform.position) < 2f)
        {
            current_State = ATTACK;
            return;
        }
        // turn walk if still idle
        if (!isMove) animator.SetBool("isMove",true);
        // chase enemy
        agent.destination = Target.transform.position;
        // Close to position then to Attack mode
    }
    void State_Attack()
    {
        if (Target == null)
        {
            current_State = WALK;
            return;
        }
        // no enemy nearby(dead or run away)
        if (Target == null || Target.GetComponent<Health>().currentHealth <= 0) 
        {
            // reset enemy
            Target = null;
            // Reset time
            timer = UnityEngine.Random.Range(1f,3f);
            // to Idle mode
            current_State = IDLE;
            return;
        }
        // if is out of attack range -> chase
        if (Vector3.Distance(Target.transform.position,transform.position) >= 2f)
        {
            current_State = CHASE;
            animator.SetBool("isAttack",false);
            return;
        }
        transform.LookAt(Target.transform.position);
        agent.isStopped = true;
        // Set Attack if is not attack
        if (!isAttack) animator.SetBool("isAttack",true);
    }
    void State_Dead()
    {
        if (timer == deadTime) animator.Play("Dead");
        // Delete Object when timer is done
        timer -= Time.deltaTime;
        if (timer <= 0f )
        {
            // Delete from list
            MainDestination.GetComponent<SubaruController>().duck_array.Remove(this);
            Death();
        }
    }
    void State_Back()
    {
        agent.isStopped = false;
        // turn walk if still idle
        if (!isMove) animator.SetBool("isMove",true);
        // go to new position
        agent.destination = goal;
        // Close to position then to Idle mode
        if (Vector3.Distance(goal,transform.position) < attack_radius)
        {
            // Reset time
            timer = UnityEngine.Random.Range(1f,3f);
            // to Idle mode
            current_State = IDLE;
            // Change speed back to 1
            animator.speed = 1f;
            agent.speed = agent_speed; 
        }
    }
    void State_Special()
    {
        agent.isStopped = true;
        timer -= Time.deltaTime;
        agent.speed = 10;
        rush_trigger = false;
        //agent.velocity = (rush_position - transform.position) * rush_speed * Time.deltaTime;
        agent.velocity = rd.velocity;
        //agent.destination = rush_position;
        if (!isMove) animator.SetBool("isMove",true);
        if (timer <= 0f)
        {
            agent.speed = 3;
            // Reset time
            timer = UnityEngine.Random.Range(1f,3f);
            // to Idle mode
            current_State = IDLE;
        }
    }
    Transform Search_Nearest(Collider[] hitColliders)
    {
        Transform target = hitColliders[0].transform.root;
        float distance = Vector3.Distance(transform.position, hitColliders[0].transform.position);

        for (int i = 1; i < hitColliders.Length; i++)
        {
            float temp_distance = Vector3.Distance(transform.position, hitColliders[i].transform.position);
            if ( temp_distance < distance)
            {
                target = hitColliders[i].transform.root;
                distance = temp_distance;
            }
        }
        return target;
    }
    /// <summary>
    /// Set Q Skill Priview Show.
    /// </summary>
    /// <param name="isShow">true-Show / false-Hide</param>
    public void Q_UI_Set(bool isShow)
    {
        Q_UI.SetActive(isShow);
    }
}

}