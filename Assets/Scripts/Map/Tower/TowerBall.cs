using UnityEngine;
using UnityEngine.AI;

namespace HR.Object.Map{
public class TowerBall : MonoBehaviour
{
    public Transform Target;
    float speed = 5f;
    [SerializeField] int towerDamage = 20;

    // Update is called once per frame
    void Update()
    {
        if (Target == null || Target.GetComponent<Health>().currentHealth <= 0) 
        {
            Destroy(gameObject);
            return;
        }
        // Get Component center
        Collider collider = Target.GetComponentInChildren<Collider>();
        Vector3 Center = collider.bounds.center;
        // Vector3 Center = Target.position + new Vector3(0, Target.GetComponent<NavMeshAgent>().height/2 - Target.GetComponent<NavMeshAgent>().baseOffset ,0);
        Vector3 direction = Center - transform.position;
        transform.position += direction.normalized * speed * Time.deltaTime;
    }
    void OnTriggerEnter(Collider other) 
    {
        // Trigger needd Collider and Rigid !!!!
        if (other.transform.root == Target)
        {
            Health health = other.transform.root.GetComponent<Health>();
            health.GetDamage(towerDamage);
            Destroy(gameObject);
        }
    }
}

}
