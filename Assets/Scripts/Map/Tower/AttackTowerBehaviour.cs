using HR.UI;
using UnityEngine;
using HR.Object.Skill;
using Mirror;

namespace HR.Object.Map{
public class AttackTowerBehaviour : TowerBase
{
    enum State {
        Idle,
        Attack,
        Break
    }
    [SerializeField] State current_State;
    [SerializeField] LayerMask enemy_layer;
    [SerializeField] Transform enemy;
    [SerializeField] Transform top;
    [SerializeField] Transform middle;
    [SerializeField] Transform Base;
    public LineRenderer lineRenderer;
    float Attack_CD_timer = -2f;
    float Attack_CD = 2f;
    [SerializeField] TowerBall Attack_Ball;
    protected override void Start()
    {
        lineRenderer.positionCount = 1;
        lineRenderer.SetPosition(0, top.position);
        current_State = State.Idle;
        base.Start();
    } 
    public override void Death()
    {
        top.gameObject.SetActive(false);
        middle.gameObject.SetActive(false);
        base.Death();
    }
    [ServerCallback]
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
                // lineRenderer.positionCount = 1;
                RpcSetLine(1,null);
                return;
            case State.Attack:
                if (enemy == null || enemy.GetComponent<Health>().currentHealth <= 0) 
                {
                    enemy = null;
                    // lineRenderer.positionCount = 1;
                    RpcSetLine(1,null);
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
                    // lineRenderer.positionCount = 1;
                    RpcSetLine(1,null);
                    return;
                }
                // shoot attack if is not in CD
                if (Time.time - Attack_CD_timer > Attack_CD)
                {
                    // Spawn attack ball and Set Target = enemy 
                    TowerBall ball = Instantiate(Attack_Ball,top.transform.position,Quaternion.identity);
                    // Set Target
                    ball.Target = enemy;
                    // Set new timer
                    Attack_CD_timer = Time.time;
                    // Spawn on Server
                    NetworkServer.Spawn(ball.gameObject);
                }                
                // // Set Line
                // lineRenderer.positionCount = 2;
                // // Set Collider Center
                // Collider collider = enemy.GetComponentInChildren<Collider>();
                // Vector3 Center = collider.bounds.center;
                // // Vector3 Center = enemy.position + new Vector3(0, enemy.GetComponent<NavMeshAgent>().height/2 - enemy.GetComponent<NavMeshAgent>().baseOffset ,0);
                // lineRenderer.SetPosition(1, Center);
                // // Set Model
                // top.LookAt(enemy.position);
                // Base.LookAt(Base.position+direction);
                RpcSetLine(2,enemy);
                return;

            case State.Break:
                if (!isDead)
                {
                    // lineRenderer.positionCount = 1;
                    RpcSetLine(1,null);
                    isDead = true;
                    Death();
                }
                return;
        }
    }
    [ClientRpc]
    void RpcSetLine(int lineNum,Transform enemy)
    {
        if (lineNum == 1) 
        {
            lineRenderer.positionCount = 1;
        }
        else
        {
            lineRenderer.positionCount = 2;
            Vector3 direction = enemy.position - Base.position;
            direction.y = 0;
            // Set Collider Center
            Collider collider = enemy.GetComponentInChildren<Collider>();
            Vector3 Center = collider.bounds.center;
            // Vector3 Center = enemy.position + new Vector3(0, enemy.GetComponent<NavMeshAgent>().height/2 - enemy.GetComponent<NavMeshAgent>().baseOffset ,0);
            lineRenderer.SetPosition(1, Center);
            // Set Model
            top.LookAt(enemy.position);
            Base.LookAt(Base.position + direction);
        }
    }
}

}