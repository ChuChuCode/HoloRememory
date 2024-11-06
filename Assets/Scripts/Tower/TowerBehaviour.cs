using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerBehaviour : MonoBehaviour,IHealth
{
    enum State {
        Idle,
        Attack,
        Break
    }
    State current_State;
    [field: SerializeField] public int maxHealth { get ; set ; }
    [field: SerializeField] public int currentHealth { get; set ; }
    [SerializeField] [Range(0.0f, 10.0f)]float attack_radius = 5f;
    [SerializeField] LayerMask enemy_layer;
    Transform enemy;
    [SerializeField] Transform top;
    [SerializeField] Transform Base;
    LineRenderer lineRenderer;
    float Attack_CD_timer = -1f;
    float Attack_CD = 1f;
    [SerializeField] TowerBall Attack_Ball;
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 1;
        lineRenderer.SetPosition(0, top.position);
        current_State = State.Idle;
        InitialHealth();
    }

    public void Death()
    {
        top.gameObject.SetActive(false);
    }

    public void GetDamage(int damage)
    {
        
    }

    public void InitialHealth()
    {
        currentHealth = maxHealth;
    }
    void Update()
    {
        if (current_State == State.Break) return;
        if (currentHealth == 0) current_State = State.Break;
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, attack_radius,enemy_layer);
        // * Need Check Attack Priority
        switch (current_State)
        {
            case State.Idle:
                if (hitColliders.Length > 0)
                {
                    enemy = hitColliders[0].transform;
                    current_State = State.Attack;
                    break;
                }
                lineRenderer.positionCount = 1;
                break;
            case State.Attack:
                //check distance 
                Vector3 direction = enemy.position - Base.position;
                direction.y = 0;
                if (direction.magnitude > attack_radius)
                {
                    current_State = State.Idle;
                    break;
                }
                // shoot attack if is not in CD
                if (Time.time - Attack_CD_timer > Attack_CD)
                {
                    // Spawn attack ball and Set Target = enemy 
                    TowerBall ball = Instantiate(Attack_Ball,top.transform.position,Quaternion.identity);
                    ball.Target = enemy;
                    // Set new timer
                    Attack_CD_timer = Time.time;
                }                
                // Set Line
                lineRenderer.positionCount = 2;
                lineRenderer.SetPosition(1, enemy.position);
                // Set Model
                top.LookAt(enemy.position);
                Base.LookAt(Base.position+direction);
                break;

            case State.Break:
                Death();
                break;
        }
    }
    void OnDrawGizmosSelected()
    {
        // Draw a yellow sphere at the transform's position
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, attack_radius);
    }
}
