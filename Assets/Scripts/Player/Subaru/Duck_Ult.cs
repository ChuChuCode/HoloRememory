using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using HR.Object.Player;

public class Duck_Ult : MonoBehaviour
{
    SphereCollider sphereCollider;
    [SerializeField] float DeleteTime = 2f;
    [SerializeField] ParticleSystem Scanner;
    [SerializeField] float duration = 10;
    [SerializeField] float size = 500;
    float Timer;
    void Awake()
    {
        sphereCollider = GetComponent<SphereCollider>();
    }
    void Update()
    {
        if (Timer > 0)
        {
            Timer -= Time.deltaTime;
            if (Timer <= 0)
            {
                Destroy(gameObject);
            }
        }
    }
    void OnTriggerEnter(Collider other)
    {
        
        ParticleSystem terrianScanner = Instantiate(Scanner, other.ClosestPoint(transform.position),Quaternion.identity);
        var main = terrianScanner.main;
        main.startLifetime = duration;
        main.startSize = size;

        CameraShake.Instance.ShakeCamera(5f,DeleteTime);
        sphereCollider.enabled = false;
        Timer = DeleteTime;
    }
}
