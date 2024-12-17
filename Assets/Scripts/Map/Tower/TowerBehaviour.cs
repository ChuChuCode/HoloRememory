using HR.UI;
using UnityEngine;
using UnityEngine.AI;

namespace HR.Object.Map{
public class TowerBehaviour : Health
{
    [Header("Gizmos")]
    [HideInInspector]public bool ShowGizmos;
    enum State {
        Idle,
        Attack,
        Break
    }
    [SerializeField] State current_State;
    [SerializeField] [Range(0.0f, 10.0f)]float attack_radius = 5f;
    [SerializeField] LayerMask enemy_layer;
    [SerializeField] Transform enemy;
    [SerializeField] Transform top;
    [SerializeField] Transform Base;
    LineRenderer lineRenderer;
    float Attack_CD_timer = -2f;
    float Attack_CD = 2f;
    [SerializeField] TowerBall Attack_Ball;
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 1;
        lineRenderer.SetPosition(0, top.position);
        current_State = State.Idle;
        InitialHealth();
    }

    public override void Death()
    {
        top.gameObject.SetActive(false);
    }

    public override void GetDamage(int damage)
    {
        base.GetDamage(damage);
        // Update UI
        Selectable.instance.updateInfo(this);
    }

    public override void InitialHealth()
    {
        currentHealth = maxHealth;
    }
    void Update()
    {
        if (isDead) return;
        if (current_State == State.Break) return;
        if (currentHealth <= 0) current_State = State.Break;
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, attack_radius,enemy_layer);
        // * Need Check Attack Priority
        switch (current_State)
        {
            case State.Idle:
                if (hitColliders.Length > 0)
                {
                    // Check target is not dead
                    for (int index = 0 ; index < hitColliders.Length; index++)
                    {
                        if (hitColliders[index].transform.root.GetComponent<Health>().currentHealth > 0)
                        {
                            /// need minion find first then other
                            enemy = hitColliders[index].transform.root;
                            current_State = State.Attack;
                            return;
                        }
                    }
                }
                enemy = null;
                lineRenderer.positionCount = 1;
                return;
            case State.Attack:
                if (enemy == null || enemy.GetComponent<Health>().currentHealth <= 0) 
                {
                    enemy = null;
                    lineRenderer.positionCount = 1;
                    current_State = State.Idle;
                    return;
                }
                //check distance 
                Vector3 direction = enemy.position - Base.position;
                direction.y = 0;
                if (direction.magnitude > attack_radius)
                {
                    current_State = State.Idle;
                    enemy = null;
                    lineRenderer.positionCount = 1;
                    return;
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
                // Set Collider Center
                Collider collider = enemy.GetComponentInChildren<Collider>();
                Vector3 Center = collider.bounds.center;
                // Vector3 Center = enemy.position + new Vector3(0, enemy.GetComponent<NavMeshAgent>().height/2 - enemy.GetComponent<NavMeshAgent>().baseOffset ,0);
                lineRenderer.SetPosition(1, Center);
                // Set Model
                top.LookAt(enemy.position);
                Base.LookAt(Base.position+direction);
                return;

            case State.Break:
                if (!isDead)
                {
                    isDead = true;
                    Death();
                }
                return;
        }
    }
    void OnDrawGizmosSelected()
    {
        if (!ShowGizmos) return;
        // Draw a yellow sphere at the transform's position
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, attack_radius);
    }
}

}