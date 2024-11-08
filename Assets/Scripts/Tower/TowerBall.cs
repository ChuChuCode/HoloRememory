using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TowerBall : MonoBehaviour
{
    public Transform Target;
    float speed = 5f;
    [SerializeField] int towerDamage = 20;

    // Update is called once per frame
    void Update()
    {
        if (Target == null || Target.GetComponent<IHealth>().currentHealth <= 0) 
        {
            Destroy(gameObject);
            return;
        }
        // Get Component center
        Vector3 Center = Target.position + new Vector3(0, Target.GetComponent<NavMeshAgent>().height/2 - Target.GetComponent<NavMeshAgent>().baseOffset ,0);
        Vector3 direction = Center - transform.position;
        transform.position += direction.normalized * speed * Time.deltaTime;
    }
    void OnTriggerEnter(Collider other) 
    {
        // Trigger needd Collider and Rigid !!!!
        if (other.transform.root == Target)
        {
            IHealth health = other.transform.root.GetComponent<IHealth>();
            health.GetDamage(towerDamage);
            Destroy(gameObject);
        }
    }
}
