using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.AI;
using Mirror;
using UnityEditor;
using Unity.VisualScripting;

public class SubaruMovementController : CharacterBase
{
    public List<Duck_AI> duck_array = new List<Duck_AI>();
    int max_duck_num = 6;
    [Header("Duck Skills")]
    [SerializeField] Duck_AI Duck_prefab;
    [SerializeField] Duck_Ult Duck_Ult;
    [SerializeField] Animator animator;

    int isRunHash;

    [Header("Skill Timer")]
    float duck_spawn_cd = 10f;
    float duck_spawn_timer = 0f;
    float duck_rush_cd = 10f;
    float duck_rush_timer = -10f;
    float duck_ult_cd = 20f;
    float duck_ult_timer = -20f;
    [Header("Dead Time")]
    float DeadTime = 3f;
    [Header("Skill Image")]
    [SerializeField] GameObject R_UI;
    [Header("Character Info")]
    AnimatorStateInfo stateInfo;

    protected override void Awake()
    {
        base.Awake();
        isRunHash = Animator.StringToHash("isMove");;
    }
    protected override void Start()
    {
        base.Start();
        
        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/cursor_g .png");
        Cursor.SetCursor(tex,new Vector2(0.5f,0.5f), CursorMode.ForceSoftware );
    }
    public override void Passive()
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
                            UnityEngine.Random.Range(-1f,1f) * Duck_AI.master_radius,
                            0f,
                            UnityEngine.Random.Range(-1f,1f) * Duck_AI.master_radius);
            }
            while (!NavMesh.SamplePosition(pos,out hit, 1.0f, NavMesh.AllAreas));
            pos = hit.position;
            Duck_AI duck = Instantiate(Duck_prefab,pos,transform.rotation);

            // Set Layer to all
            Transform[] children = duck.GetComponentsInChildren<Transform>(includeInactive: true);
            foreach(Transform child in children)
            {
                child.gameObject.layer = gameObject.layer;
            }
            // Set Enemy Layer
            duck.Update_Enemy_Layer(gameObject.layer);
            // Set Q UI
            if (IsPressed_Q) duck.Q_UI_Set(true);

            NetworkServer.Spawn(duck.gameObject);
            // Set Info
            duck.player = this;
            // Add to list
            duck_array.Add(duck);
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
    protected override void Hide_R_UI()
    {
        // Hide R preview
        R_UI.SetActive(false);
    }
    /// Q skill
    public override void OnQKeyDown(InputAction.CallbackContext context)
    {
        if (Time.time - duck_rush_timer < duck_rush_cd) return;
        if (duck_array.Count == 0) return;
        base.OnQKeyDown(context);
        // Show UI preview
        foreach (var duck in duck_array)
        {
            duck.Q_UI_Set(true);
        }
    }
    public override void OnQKeyUp(InputAction.CallbackContext context)
    {
        if (!IsPressed_Q) return;
        // If count = 0 when key Up 
        if (duck_array.Count == 0) return;
        base.OnQKeyUp(context);
        animator.SetTrigger("Special");
        duck_rush_timer = Time.time;
        
        foreach (var duck in duck_array)
        {
            duck.rush_position = mouseProject;
            duck.rush_trigger = true;
            duck.Q_UI_Set(false);
        }
    }
    /// R skill 
    public override void OnRKeyDown(InputAction.CallbackContext context)
    {
        if (Time.time - duck_ult_timer < duck_ult_cd) return;
        if (duck_array.Count == 0) return;
        base.OnRKeyDown(context);
        // Show UI preview
        R_UI.SetActive(true);
    }
    public override void OnRKeyUp(InputAction.CallbackContext context)
    {
        if (!IsPressed_R) return;
        if (duck_array.Count == 0) return;
        base.OnRKeyUp(context);
        animator.SetTrigger("Special");
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
    }
    void HandleMoveAnmation()
    {
        bool isRun = agent.velocity.magnitude > 0;
        // Run when idle
        if (isRun)
        {
            animator.SetBool(isRunHash,true);
        }
        // idle when run
        else
        {
            animator.SetBool(isRunHash,false);
        }
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
    protected override void Update()
    {
        if (!isLocalPlayer) return;

        // Update Cool Down
        MianInfoUI.instance.Q.Set_CoolDown(duck_rush_timer,duck_rush_cd);
        // MianInfoUI.instance.W.Set_CoolDown(duck_ult_timer,duck_ult_cd);
        // MianInfoUI.instance.E.Set_CoolDown(duck_ult_timer,duck_ult_cd);
        MianInfoUI.instance.R.Set_CoolDown(duck_ult_timer,duck_ult_cd);

        // Dead already and wait to respawn
        if (isDead) 
        {
            return;
        }
        // Dead now 
        if (currentHealth <= 0 && !isDead)
        {
            DeadScreen.instance.isDead(true);
            agent.isStopped = true;
            animator.Play("Dead");
            // All Duck Dead
            foreach (var duck in duck_array)
            {
                duck.currentHealth = 0;
            }
            isDead = true;
            // Unregister control
            InputSystem.instance.playerInput.Player.Right_Mouse.started -= OnRightMouseClick;
            InputSystem.instance.playerInput.Player.Q.started -= OnQKeyDown;
            InputSystem.instance.playerInput.Player.R.started -= OnRKeyDown;
            // UI Update -> Ally Icon
            
            // Dead Time Start
            Death();
            return;
        }
        // Update Mouse raycast -> MousePosition
        Get_Project_Mouse();
        // Check Free Camera Reset -> Camera_Reset
        Camera_Reset();
        // If stand Animation => stop move and rotate
        stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if(stateInfo.IsTag("stand"))
        {
            agent.isStopped = true;
            animator.SetBool(isRunHash,false);
            return;
        }
        else
        {
            agent.isStopped = false;
        }
        HandleMoveAnmation();
        
        base.Update();

        if (InputSystem.instance.playerInput.Player.Camera_Reset.WasPressedThisFrame())
            Debug.Log("Was Just Pressed");

        if (InputSystem.instance.playerInput.Player.Camera_Reset.IsPressed())
            Debug.Log("Is Currently Pressed");

        if (InputSystem.instance.playerInput.Player.Camera_Reset.WasReleasedThisFrame())
            Debug.Log("Was Just Released");
        if (InputSystem.instance.playerInput.Player.Camera_Reset.triggered)
            Debug.Log("Triggered");
    }
    public override void InitialHealth()
    {
        base.InitialHealth();
        MianInfoUI.instance.updateInfo(this);
        Selectable.instance.updateInfo(this);
    }
    public override void GetDamage(int damage)
    {
        base.GetDamage(damage);
        // Update UI
        MianInfoUI.instance.updateInfo(this);
        Selectable.instance.updateInfo(this);
    }

    public override void Death()
    {
        float dead_start_time = Time.time;
        StartCoroutine(nameof(DeadCountDown),dead_start_time);
    }
    IEnumerator DeadCountDown(float dead_start_time)
    {
        while (Time.time - dead_start_time < DeadTime)
        {
            // Update UI wait time
            yield return null;
        }
        if (gameObject.layer == LayerMask.NameToLayer("Team1"))
        {
            agent.Warp(GameController.Instance.Team1_transform.position);
        }
        else if (gameObject.layer == LayerMask.NameToLayer("Team2"))
        {
            agent.Warp(GameController.Instance.Team2_transform.position);
        }
        // Screen Control
        DeadScreen.instance.isDead(false);
        // Animation Control
        agent.isStopped = false;
        animator.Play("Idle");
        // Register control
        InputSystem.instance.playerInput.Player.Right_Mouse.started += OnRightMouseClick;
        InputSystem.instance.playerInput.Player.Q.started += OnQKeyDown;
        InputSystem.instance.playerInput.Player.R.started += OnRKeyDown;
        isDead = false;
        // Health
        InitialHealth();
        yield return null;
    }
}
