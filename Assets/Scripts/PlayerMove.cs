using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerMove : NetworkBehaviour
{
    // Update is called once per frame
    void Update()
    {
        if (!isLocalPlayer) return;
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 playerMovement = new Vector3(h, v,0)* Time.deltaTime;

        transform.position += playerMovement;
    }
}
