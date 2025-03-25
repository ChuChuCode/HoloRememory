using UnityEngine;
using HR.Object.Player;
using Mirror;

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
    int attack = 10;
    protected override void Start()
    {
        if (!isOwned) return;
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
        if (!isOwned) return;
        // if (MainDestination == null ) return;
        isMove = animator.GetBool("isMove");
        isAttack = animator.GetBool("isAttack");
        
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
        agent.isStopped = true;
        // turn idle if still walk or still attack
        if (isMove) animator.SetBool("isMove",false);
        if (isAttack) animator.SetBool("isAttack",false);

        // Check enemy nearby 
        hitColliders = Physics.OverlapSphere(transform.position, Search_radius, Layer_Enemy);
        // Has enemy nearby
        if (hitColliders.Length > 0)
        {
            Target = Search_Nearest(hitColliders);
            // to Chase mode
            current_State = CHASE;
            return;
        }

        // timer for idle mode
        timer -= Time.deltaTime;
        // Change Idle to Walk mode
        if (timer <= 0f)
        {
            // find new position as goal
            goal = MainDestination.position + 
                new Vector3(
                    UnityEngine.Random.Range(-1f,1f)*master_distance,
                    0f,
                    UnityEngine.Random.Range(-1f,1f)*master_distance
                );
            // to Walk mode
            current_State = WALK;
            return;
        }
    }
    void State_Walk()
    {
        agent.isStopped = false;
        // turn walk if still idle or still attack
        if (!isMove) animator.SetBool("isMove",true);
        if (isAttack) animator.SetBool("isAttack",false);
        // go to new position
        agent.destination = goal;

        // Check enemy nearby
        hitColliders = Physics.OverlapSphere(transform.position, Search_radius, Layer_Enemy);
        // Has enemy nearby
        if (hitColliders.Length > 0)
        {
            Target = Search_Nearest(hitColliders);
            // to Chase mode
            current_State = CHASE;
            return;
        }

        // Close to position then to Idle mode
        if (Vector3.Distance(goal,transform.position) < 2f)
        {
            // Reset time
            timer = Random.Range(1f,3f);
            goal = transform.position;
            // to Idle mode
            current_State = IDLE;
            return;
        }
    }
    void State_Chase()
    {
        agent.isStopped = false;
        if (!isMove) animator.SetBool("isMove",true);
        if (isAttack) animator.SetBool("isAttack",false);

        // Check enemy faraway or dead
        if (Target == null || Get_Target_Radius(Target) > Search_radius || Target.GetComponent<Health>().currentHealth <= 0) 
        {
            // Check enemy nearby
            hitColliders = Physics.OverlapSphere(transform.position, Search_radius, Layer_Enemy);
            // Has enemy nearby
            if (hitColliders.Length > 0)
            {
                Target = Search_Nearest(hitColliders);
                // to Chase mode
                current_State = CHASE;
                return;
            }
            // no enemy nearby
            else
            {
                // reset target
                Target = null;
                // Reset time
                timer = UnityEngine.Random.Range(1f,3f);
                // to Idle mode
                current_State = IDLE;
                return;
            }
        }

        // If is in attack Range -> to attack state
        // Get Colldier Type and get Distance
        if (Get_Target_Radius(Target) < 2f)
        {
            current_State = ATTACK;
            return;
        }

        // chase enemy
        agent.destination = Target.transform.position;
    }
    void State_Attack()
    {
        agent.isStopped = true;
        // Set Attack if is not attack
        if (!isMove) animator.SetBool("isMove",false);
        if (!isAttack) animator.SetBool("isAttack",true);

        // Check enemy faraway or dead
        if (Target == null || 
            Get_Target_Radius(Target) > Search_radius || 
            Target.GetComponent<Health>().currentHealth <= 0) 
        {
            // Check enemy nearby
            hitColliders = Physics.OverlapSphere(transform.position, Search_radius, Layer_Enemy);
            // Has enemy nearby
            if (hitColliders.Length > 0)
            {
                Target = Search_Nearest(hitColliders);
                // Check distance
                if (Get_Target_Radius(Target) < 2f)
                {
                    // still attack
                    current_State = ATTACK;
                    return;
                }
                else
                {
                    // to Chase mode
                    current_State = CHASE;
                    return;
                }
                
            }
            // no enemy nearby
            else
            {
                // reset target
                Target = null;
                // Reset time
                timer = UnityEngine.Random.Range(1f,3f);
                // to Idle mode
                current_State = IDLE;
                return;
            }
        }
        // if is out of attack range -> chase
        if (Get_Target_Radius(Target) >= 2f)
        {
            current_State = CHASE;
            return;
        }
        transform.LookAt(Target.transform.position);
    }
    void State_Dead()
    {
        if (timer == deadTime) GetComponent<NetworkAnimator>().SetTrigger("isDead");
        // Delete Object when timer is done
        timer -= Time.deltaTime;
        if (timer <= 0f )
        {
            // Delete from list
            MainDestination.GetComponent<SubaruController>().duck_array.Remove(this);
            if (isOwned) CmdDead();
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
    /// <summary>
    /// Set Q Skill Priview Show.
    /// </summary>
    /// <param name="isShow">true-Show / false-Hide</param>
    public void Q_UI_Set(bool isShow)
    {
        Q_UI.SetActive(isShow);
    }
    public override void InitialHealth()
    {
        if (!isOwned) return;
        CmdSetlHealth(maxHealth);
    }
    public void Attack()
    {
        if (!isOwned) return;
        CmdAttack(Target);
    }
    [Command]
    void CmdAttack(Transform enemy)
    {
        if (enemy == null || enemy.GetComponent<Health>().currentHealth <= 0) return;
        if (enemy.TryGetComponent<CharacterBase>(out CharacterBase character))
        {
            character.GetDamage(attack);
        }
        else
        {
            enemy.GetComponent<Health>().GetDamage(attack);
        }
    }
    [Command]
    void CmdDead()
    {
        currentHealth = 0;
        NetworkServer.Destroy(this.gameObject);
    }
}

}