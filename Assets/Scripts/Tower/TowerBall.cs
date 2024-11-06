using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerBall : MonoBehaviour
{
    public Transform Target;
    float speed = 5f;

    // Update is called once per frame
    void Update()
    {
        Vector3 direction = Target.position - transform.position;
        transform.position += direction.normalized * speed * Time.deltaTime;
    }
    void OnTriggerEnter(Collider other) 
    {
        if (other.gameObject.transform == Target)
        {
            print("hit");
            Destroy(gameObject);
        }
    }
}
