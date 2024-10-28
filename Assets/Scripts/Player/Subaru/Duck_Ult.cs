using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Duck_Ult : MonoBehaviour
{
    SphereCollider sphereCollider;
    [SerializeField] float DeleteTime = 2f;
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
    void OnTriggerEnter()
    {
        CameraShake.Instance.ShakeCamera(5f,DeleteTime);
        sphereCollider.enabled = false;
        Timer = DeleteTime;
    }
}
