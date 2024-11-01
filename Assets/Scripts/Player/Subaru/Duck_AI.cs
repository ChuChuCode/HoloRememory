using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Duck_AI : MonoBehaviour,IHealth
{
    enum State{
        Idle,
        Walk,
        Back,
        Chase,
        Attack,
        Special
    };
    public GameObject player;
    GameObject enemy;
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Rigidbody rd;
    Animator animator;
    public static float master_radius = 5f;
    float attack_radius = 5f;
    float agent_speed = 3f;
    float agent_back_speed = 9f;
    Vector3 goal;
    [SerializeField] State current_State;
    [SerializeField] float time = 1f;
    [SerializeField] LayerMask enemy_layer;

    [Header("Skill")]
    public Vector3 rush_position;
    // This Parameter is for same position.
    //float rush_speed = 50f;
    // This Parameter is for same Force.
    float rush_speed = 500f;
    float rush_time = 2f;
    public bool rush_trigger;
    [Header("Character Info")]
    [SerializeField] Bar healthBar;
    [field: SerializeField] public int maxHealth { get ; set; }
    [field: SerializeField] public int currentHealth { get; set ; }


    void Awake() 
    {
        animator = GetComponent<Animator>();
    }
    void Start()
    {
        current_State = State.Idle;
        rush_trigger = false;
        InitialHealth();
    }
    // Update is called once per frame
    void Update()
    {
        bool isMove = animator.GetBool("isMove");
        bool isAttack = animator.GetBool("isAttack");
        // If far away from player -> to Back state
        if (Vector3.Distance(transform.position,player.transform.position) >= 2* master_radius)
        {
            // if is still attack
            animator.SetBool("isAttack",false);
            current_State = State.Back;
            // new position to player 
            goal = player.transform.position + 
                new Vector3(
                    UnityEngine.Random.Range(-1f,1f)*master_radius,
                    0f,
                    UnityEngine.Random.Range(-1f,1f)*master_radius
                );
            // Change Speed * 5
            animator.speed = 3f;
            agent.speed = agent_back_speed; 
        }
        // Search Enemy use sphere
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, attack_radius,enemy_layer);
        // Special mode interrupt
        if (rush_trigger)
        {
            animator.SetBool("isAttack",false);
            rush_position.y = 0;
            // to Special mode
            current_State = State.Special;
            // This Parameter is for same position.
            //rd.AddForce( (rush_position - transform.position) * rush_speed);
            // This Parameter is for same Force with normalized
            rd.AddForce( (rush_position - transform.position).normalized * rush_speed);
            time = rush_time;
        }
        switch (current_State)
        {
            case State.Idle:
                // Has enemy nearby
                if (hitColliders.Length > 0)
                {
                    // to Chase mode
                    current_State = State.Chase;
                    break;
                }
                agent.isStopped = true;
                // turn idle if still walk
                if (isMove) animator.SetBool("isMove",false);
                // timer for idle mode
                time -= Time.deltaTime;
                if (time < 0f)
                {
                    // find new position to idle
                    goal = player.transform.position + 
                        new Vector3(
                            UnityEngine.Random.Range(-1f,1f)*master_radius,
                            0f,
                            UnityEngine.Random.Range(-1f,1f)*master_radius
                        );
                    // to Walk mode
                    current_State = State.Walk;
                    break;
                }
                break;
            case State.Walk:
                // Has enemy nearby
                if (hitColliders.Length > 0)
                {
                    // to Chase mode
                    current_State = State.Chase;
                    break;
                }
                agent.isStopped = false;
                // turn walk if still idle
                if (!isMove) animator.SetBool("isMove",true);
                // go to new position
                agent.destination = goal;
                // Close to position then to Idle mode
                if (Vector3.Distance(goal,transform.position) < 1f)
                {
                    // Reset time
                    time = UnityEngine.Random.Range(1f,3f);
                    // to Idle mode
                    current_State = State.Idle;
                    break;
                }
                break;
            case State.Back:
                agent.isStopped = false;
                // turn walk if still idle
                if (!isMove) animator.SetBool("isMove",true);
                // go to new position
                agent.destination = goal;
                // Close to position then to Idle mode
                if (Vector3.Distance(goal,transform.position) < 1f)
                {
                    // Reset time
                    time = UnityEngine.Random.Range(1f,3f);
                    // to Idle mode
                    current_State = State.Idle;
                    // Change speed back to 1
                    animator.speed = 1f;
                    agent.speed = agent_speed; 
                    break;
                }
                break;
            case State.Chase:
                // no enemy nearby(dead or run away)
                if (hitColliders.Length == 0)
                {
                    // reset enemy
                    enemy = null;
                    // Reset time
                    time = UnityEngine.Random.Range(1f,3f);
                    // to Idle mode
                    current_State = State.Idle;
                    break;
                }
                agent.isStopped = false;
                // Search enemy
                if (enemy == null)
                { 
                    enemy = Search_Nearest(hitColliders);
                }
                // If is in attack Range -> to attack state
                if (Vector3.Distance(enemy.transform.position,transform.position) < 2f)
                {
                    current_State = State.Attack;
                    break;
                }
                // turn walk if still idle
                if (!isMove) animator.SetBool("isMove",true);
                // chase enemy
                agent.destination = enemy.transform.position;
                // Close to position then to Attack mode
                break;
            case State.Attack:
                // no enemy nearby(dead or run away)
                if (hitColliders.Length == 0)
                {
                    // reset enemy
                    enemy = null;
                    // Reset time
                    time = UnityEngine.Random.Range(1f,3f);
                    // to Idle mode
                    current_State = State.Idle;
                    break;
                }
                // if is out of attack range -> chase
                if (Vector3.Distance(enemy.transform.position,transform.position) >= 2f)
                {
                    current_State = State.Chase;
                    animator.SetBool("isAttack",false);
                    break;
                }
                transform.LookAt(enemy.transform.position);
                agent.isStopped = true;
                // Set Attack if is not attack
                if (!isAttack) animator.SetBool("isAttack",true);
                
                break;
            case State.Special:
                agent.isStopped = true;
                time -= Time.deltaTime;
                agent.speed = 10;
                rush_trigger = false;
                //agent.velocity = (rush_position - transform.position) * rush_speed * Time.deltaTime;
                agent.velocity = rd.velocity;
                //agent.destination = rush_position;
                if (!isMove) animator.SetBool("isMove",true);
                if (time < 0f)
                {
                    agent.speed = 3;
                    // Reset time
                    time = UnityEngine.Random.Range(1f,3f);
                    // to Idle mode
                    current_State = State.Idle;
                    break;
                }
                break;
        }
    }
    GameObject Search_Nearest(Collider[] hitColliders)
    {
        GameObject target = hitColliders[0].gameObject;
        float distance = Vector3.Distance(transform.position, hitColliders[0].transform.position);

        for (int i = 1; i < hitColliders.Length; i++)
        {
            float temp_distance = Vector3.Distance(transform.position, hitColliders[i].transform.position);
            if ( temp_distance < distance)
            {
                target = hitColliders[i].gameObject;
                distance = temp_distance;
            }
        }
        return target;
    }

    public void InitialHealth()
    {
        currentHealth = maxHealth;
        healthBar.SetMaxValue(maxHealth);
    }
    public void GetDamage(int damage)
    {
        currentHealth -= damage;
        healthBar.SetValue(currentHealth);
    }

    public void Death()
    {
        
    }
}
