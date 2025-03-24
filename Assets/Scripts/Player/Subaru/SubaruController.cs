using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Mirror;
using HR.UI;
using HR.Object.Minion;

namespace HR.Object.Player{
public class SubaruController : CharacterBase
{
    [SerializeField] SubaruAnimationMethod AnimationMethod;
    [Header("Passive")]
    public List<Duck_AI> duck_array = new List<Duck_AI>();
    [SerializeField] Duck_AI Duck_prefab;
    [SerializeField] int max_duck_num = 3;
    [Header("Duck Skills")]
    [SerializeField] Duck_Ult Duck_Ult;
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
        if (Duck_Spawnable() && duck_array.Count < max_duck_num )
        {
            NavMeshHit hit;
            Vector3 pos;
            duck_spawn_timer = Time.time ;
            // Check spawn point is on Navmesh
            do
            {
                pos = transform.position + 
                        new Vector3(
                            UnityEngine.Random.Range(-1f,1f) * Duck_AI.master_distance,
                            0f,
                            UnityEngine.Random.Range(-1f,1f) * Duck_AI.master_distance);
            }
            while (!NavMesh.SamplePosition(pos,out hit, 1.0f, NavMesh.AllAreas));
            pos = hit.position;
            
            CmdSpawnDuck(pos);
        }
    }
    protected override void Hide_Q_UI()
    {
        // Hide Duck UI
        foreach (var duck in duck_array)
        {
            duck.Q_UI_Set(false);
        }
    }
    protected override void Hide_W_UI()
    {
        
    }
    protected override void Hide_E_UI()
    {

    }
    protected override void Hide_R_UI()
    {
        // Hide R preview
        R_UI.SetActive(false);
    }
    public override bool OnQKeyDown()
    {
        if (Time.time - duck_rush_timer < duck_rush_cd) return false;
        if (duck_array.Count == 0) return false;
        // Show UI preview
        foreach (var duck in duck_array)
        {
            duck.Q_UI_Set(true);
        }
        return true;
    }
    public override bool OnQKeyUp()
    {
        // If count = 0 when key Up 
        if (duck_array.Count == 0) return false;
        GetComponent<NetworkAnimator>().SetTrigger("Special");
        duck_rush_timer = Time.time;
        
        foreach (var duck in duck_array)
        {
            duck.rush_position = mouseProject;
            duck.rush_trigger = true;
            duck.Q_UI_Set(false);
        }
        return true;
    }
    public override bool OnWKeyDown()
    {
        return true;
    }
    public override bool OnWKeyUp()
    {
        return true;
    }
    public override bool OnEKeyDown()
    {
        return true;
    }
    public override bool OnEKeyUp()
    {
        return true;
    } 
    public override bool OnRKeyDown()
    {
        if (Time.time - duck_ult_timer < duck_ult_cd) return false;
        if (duck_array.Count == 0) return false;
        // Show UI preview
        R_UI.SetActive(true);
        return true;
    }
    public override bool OnRKeyUp()
    {
        if (!IsPressed_R) return false;
        if (duck_array.Count == 0) return false;
        GetComponent<NetworkAnimator>().SetTrigger("Special");
        duck_ult_timer = Time.time;
        // Delete Duck
        int duck_index = UnityEngine.Random.Range(0,duck_array.Count);
        GameObject deleteDuck = duck_array[duck_index].gameObject;
        duck_array.Remove(duck_array[duck_index]);
        Destroy(deleteDuck);
        // Hide UI preview
        R_UI.SetActive(false);
        // Spawn Ult Duck
        Duck_Ult duck = Instantiate(Duck_Ult,transform.position + new Vector3(0f,10f,0f) ,Quaternion.identity);
        return true;
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
    protected override void SkillUpdate(bool isRespawn)
    {
        if (isRespawn) 
        {
            MainInfoUI.instance.Q.Set_CoolDown(0,duck_rush_cd);
            // MainInfoUI.instance.W.Set_CoolDown(0,duck_ult_cd);
            // MainInfoUI.instance.E.Set_CoolDown(0,duck_ult_cd);
            MainInfoUI.instance.R.Set_CoolDown(0,duck_ult_cd);
        }
        else
        {
            // Update Cool Down
            MainInfoUI.instance.Q.Set_CoolDown(duck_rush_timer,duck_rush_cd);
            // MainInfoUI.instance.W.Set_CoolDown(duck_ult_timer,duck_ult_cd);
            // MainInfoUI.instance.E.Set_CoolDown(duck_ult_timer,duck_ult_cd);
            MainInfoUI.instance.R.Set_CoolDown(duck_ult_timer,duck_ult_cd);
        }
        
    }
        protected override void OnDestroy()
    {
        if (!NetworkClient.active) return;
        Delete_Ducks();
        base.OnDestroy();
    }
    protected override void NormalAttack()
    {
        // AnimationMethod.Target = Target;
        CmdSetTarget(Target);
        base.NormalAttack();
    }
    protected override void Death()
    {
        // All Duck Dead on Server
        Delete_Ducks();
        base.Death();
    }
    protected override void OnRecall()
    {
        // Teleport all ducks
        foreach (Duck_AI duck in duck_array)
        {
            NavMeshHit hit;
            Vector3 pos;
            do
            {
            pos = transform.position + 
                    new Vector3(
                        UnityEngine.Random.Range(-1f,1f) * Duck_AI.master_distance,
                        0f,
                        UnityEngine.Random.Range(-1f,1f) * Duck_AI.master_distance);
            }
            while (!NavMesh.SamplePosition(pos,out hit, 1.0f, NavMesh.AllAreas));
            pos = hit.position;
            duck.agent.Warp(pos);
        }
    }
    [Command]
    void CmdSpawnDuck(Vector3 pos)
    {
        Duck_AI duck = Instantiate(Duck_prefab,pos,transform.rotation);
        // Set Info
        NetworkServer.Spawn(duck.gameObject,this.gameObject);
        // Set All Client
        Client_Add_Duck(duck);
    }
    [ClientRpc]
    void Client_Add_Duck(Duck_AI duck)
    {
        // Set Layer to all
        Transform[] children = duck.GetComponentsInChildren<Transform>(includeInactive: true);
        foreach(Transform child in children)
        {
            child.gameObject.layer = gameObject.layer;
        }
        // Set Enemy Layer
        duck.Update_Enemy_Layer(gameObject.layer);
        if (!isLocalPlayer) return;
        // Set Q UI
        if (IsPressed_Q) duck.Q_UI_Set(true);
        duck.MainDestination = transform;
        duck_array.Add(duck);
    }
    [Command]
    void CmdSetTarget(Transform Enemy)
    {
        AnimationMethod.Target = Enemy;
    }
    [Command]
    void Delete_Ducks()
    {
        if (!isLocalPlayer) return;
        foreach (var duck in duck_array)
        {
            if (duck == null) continue;
            duck.currentHealth = 0;
        }
    }
}

}