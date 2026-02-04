using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Mirror;
using HR.UI;
using UnityEngine.InputSystem;

namespace HR.Object.Player{
public class SubaruController : CharacterBase
{
    [SerializeField] SubaruAnimationMethod AnimationMethod;


    [Header("Skill Timer")]
    float duck_spawn_cd = 10f;
    float duck_spawn_timer = 5f;
    float duck_rush_cd = 10f;
    float duck_rush_timer = -10f;
    float duck_ult_cd = 20f;
    float duck_ult_timer = -20f;

    [Header("Skill Image")]
    [SerializeField] GameObject R_UI;
    
    protected override void Passive()
    {
        // if (Duck_Spawnable())
        // {
        //     NavMeshHit hit;
        //     Vector3 pos;
        //     duck_spawn_timer = Time.time ;
        //     // Check spawn point is on Navmesh
        //     do
        //     {
        //         pos = transform.position + 
        //                 new Vector3(
        //                     UnityEngine.Random.Range(-1f,1f) * Duck_AI.master_distance,
        //                     0f,
        //                     UnityEngine.Random.Range(-1f,1f) * Duck_AI.master_distance);
        //     }
        //     while (!NavMesh.SamplePosition(pos,out hit, 1.0f, NavMesh.AllAreas));
        //     pos = hit.position;
            
        //     // CmdSpawnDuck(pos);
        // }
    }
    // Duck_Spawn_Timer
    bool Duck_Spawnable()
    {
        if (Time.time - duck_spawn_timer > duck_spawn_cd)
        {
            return true;
        }
        return false;
    }
    // Update is called once per frame
    /// Authority Object will Delect itself when diconnnected
    // protected override void OnDestroy()
    // {
    //     if (!isLocalPlayer) return;
    //     Delete_Ducks();
    //     base.OnDestroy();
    // }
    protected override void NormalAttack()
    {
        // AnimationMethod.Target = Target;
        // CmdSetTarget(Target);
        base.NormalAttack();
    }
    // protected override void Death()
    // {
    //     if (!isLocalPlayer) return;
    //     // All Duck Dead on Server 
    //     // duck_array in on client
    //     foreach (Duck_AI duck in duck_array)
    //     {
    //         Dead_Ducks(duck);
    //     }
    //     base.Death();
    // }
    // [Command]
    // void CmdSpawnDuck(Vector3 pos)
    // {
    //     Duck_AI duck = Instantiate(Duck_prefab,pos,transform.rotation);
    //     // Set Info
    //     NetworkServer.Spawn(duck.gameObject,this.gameObject);
    //     // Set All Client
    //     Client_Add_Duck(duck);
    // }
    // [ClientRpc]
    // void Client_Add_Duck(Duck_AI duck)
    // {
    //     // Set Layer to all
    //     Transform[] children = duck.GetComponentsInChildren<Transform>(includeInactive: true);
    //     foreach(Transform child in children)
    //     {
    //         child.gameObject.layer = gameObject.layer;
    //     }
    //     // Set Enemy Layer
    //     duck.Update_Enemy_Layer(gameObject.layer);
    //     if (!isLocalPlayer) return;
    //     // Set Q UI
    //     duck.MainDestination = transform;
    //     duck_array.Add(duck);
    // }
    [Command]
    void CmdSetTarget(Transform Enemy)
    {
        AnimationMethod.Target = Enemy;
    }
    // [Command]
    // void Dead_Ducks(Duck_AI duck)
    // {
    //     if (duck == null) return;
    //     duck.currentHealth = 0;
    // }
}

}