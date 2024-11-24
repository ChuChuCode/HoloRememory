using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.AI;
using Mirror;
using UnityEditor;
using Unity.VisualScripting;

public class SubaruMovementController : Health, ICharacter
{
    public List<Duck_AI> duck_array = new List<Duck_AI>();
    NavMeshAgent agent;
    int max_duck_num = 6;
    [Header("Camera ")]
    [SerializeField] GameObject Fixed_Cam;
    [SerializeField] GameObject Free_CameParent;
    [Header("Duck Skills")]
    [SerializeField] Duck_AI Duck_prefab;
    [SerializeField] Duck_Ult Duck_Ult;
    Vector3 mouseProject;
    [SerializeField] Animator animator;
    [SerializeField] ParticleSystem Target;

    int isRunHash;

    Vector3 currentMovement;
    bool isMovementPressed;
    float rotationFactorPerFrame = 10.0f;
    [Header("Skill Pressed")]
    [SerializeField] bool IsPressed_Q = false;
    [SerializeField] bool IsPressed_W = false;
    [SerializeField] bool IsPressed_E = false;
    [SerializeField] bool IsPressed_R = false;
    [Header("Skill Time")]
    float duck_spawn_cd = 10f;
    float duck_spawn_timer = 0f;
    float duck_rush_cd = 10f;
    float duck_rush_timer = -10f;
    float duck_ult_cd = 20f;
    float duck_ult_timer = -20f;
    [Header("Dead Time")]
    float DeadTime = 3f;
    [Header("Character Info")]
    AnimatorStateInfo stateInfo;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        isRunHash = Animator.StringToHash("isMove");
        // Health Initial
        InitialHealth();
    }
    void Start()
    {
        DontDestroyOnLoad(gameObject);
        // Set layer
        Selectable.instance.playerLayerID = gameObject.layer;
        if (!isLocalPlayer) return;
        Free_CameParent.SetActive(true);
        InputSystem.instance.playerInput.Player.Right_Mouse.started += OnRightMouseClick;
        //playerInput.Player.Right_Mouse.canceled += OnRightMouseClick;
        //playerInput.Player.Right_Mouse.performed += OnRightMouseClick;

        InputSystem.instance.playerInput.Player.Left_Mouse.started += OnLeftMouseClick;
        
        InputSystem.instance.playerInput.Player.Q.started += OnQKeyDown;
        InputSystem.instance.playerInput.Player.Q.canceled += OnQKeyUp;

        //playerInput.Player.MousePosition.started += OnMousePositionInput;
        //playerInput.Player.MousePosition.performed += OnMousePositionInput;
        //playerInput.Player.MousePosition.canceled += OnMousePositionInput;

        InputSystem.instance.playerInput.Player.R.started += OnRKeyInput;
        InputSystem.instance.playerInput.Player.Camera_Change.started += OnYKeyClick;

        InputSystem.instance.playerInput.Player.Camera_Reset.started += OnSpaceKeyClick;
        InputSystem.instance.playerInput.Player.Camera_Reset.performed += OnSpaceKeyClick;
        
        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/cursor_g .png");
        Cursor.SetCursor(tex,new Vector2(0.5f,0.5f), CursorMode.ForceSoftware );
    }
    public void Passive()
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

            NetworkServer.Spawn(duck.gameObject);
            // Set Info
            duck.player = this;
            // Add to list
            duck_array.Add(duck);
        }
    }
    public void OnRKeyInput(InputAction.CallbackContext context)
    {
        if (Time.time - duck_ult_timer < duck_ult_cd) return;
        if (duck_array.Count == 0) return;
        animator.SetTrigger("Special");
        duck_ult_timer = Time.time;
        // Delete Duck
        int duck_index = UnityEngine.Random.Range(0,duck_array.Count);
        GameObject deleteDuck = duck_array[duck_index].gameObject;
        duck_array.Remove(duck_array[duck_index]);
        Destroy(deleteDuck);
        // Spawn Ult Duck
        Duck_Ult duck = Instantiate(Duck_Ult,transform.position + new Vector3(0f,10f,0f) ,Quaternion.identity);
    }

    void OnMousePositionInput(InputAction.CallbackContext context)
    {
        Vector3 mousePos = context.ReadValue<Vector2>();
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        if (Physics.Raycast(ray, out hit))
        {
            mouseProject = hit.point;
        }
    }
    public void OnRightMouseClick(InputAction.CallbackContext context)
    {
        // Check skill is Hovering
        
        ParticleSystem temp = Instantiate(Target,mouseProject + new Vector3(0,0.01f,0), Quaternion.identity);
    }
    public void OnLeftMouseClick(InputAction.CallbackContext context)
    {
        // Check if Skill is pressed
    }
    public void OnQKeyDown(InputAction.CallbackContext context)
    {
        if (Time.time - duck_rush_timer < duck_rush_cd) return;
        if (duck_array.Count == 0) return;
        IsPressed_Q = true;
        // Show UI preview
        foreach (var duck in duck_array)
        {
            duck.Q_UI_Set(true);
        }
    }
    public void OnQKeyUp(InputAction.CallbackContext context)
    {
        if (!IsPressed_Q) return;
        // If count = 0 when key Up 
        if (duck_array.Count == 0) return;
        animator.SetTrigger("Special");
        duck_rush_timer = Time.time;
        IsPressed_Q = false;
        foreach (var duck in duck_array)
        {
            duck.rush_position = mouseProject;
            duck.rush_trigger = true;
            duck.Q_UI_Set(false);
        }
    }
    public void OnYKeyClick(InputAction.CallbackContext context)
    {
        // Fixed cam active -> Fixed cam deactive and free cam active
        if (Fixed_Cam.activeSelf)
        {
            Fixed_Cam.SetActive(false);
            Free_CameParent.SetActive(true);
            // set position to gameobject
            Free_CameParent.transform.position = gameObject.transform.position;
        }
        else
        {
            Fixed_Cam.SetActive(true);
            Free_CameParent.SetActive(false);
        }
    } 
    public void OnSpaceKeyClick(InputAction.CallbackContext context)
    {
        if (!Free_CameParent.activeSelf) return;
        Free_CameParent.transform.position = gameObject.transform.position;
    }
    void HandleRotation( )
    {
        Vector3 positionToLookAt;
        positionToLookAt.x = currentMovement.x;
        positionToLookAt.y = 0.0f;
        positionToLookAt.z = currentMovement.z;
        Quaternion currentRotation = transform.rotation;

        if (isMovementPressed)
        {
            Quaternion targetRotation = Quaternion.LookRotation(positionToLookAt);
            transform.rotation = Quaternion.Slerp(currentRotation,targetRotation,rotationFactorPerFrame* Time.deltaTime);
        }
    }
    void HandleAnmation()
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
    void Update()
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
            InputSystem.instance.playerInput.Player.R.started -= OnRKeyInput;
            // UI Update -> Ally Icon
            
            // Dead Time Start
            Death();
            return;
        }
        // check mouse raycast
        Vector3 mousePos = InputSystem.instance.playerInput.Player.MousePosition.ReadValue<Vector2>();
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        if (Physics.Raycast(ray.origin,ray.direction, out hit,Mathf.Infinity,~LayerMask.NameToLayer("Land")))
        {
            mouseProject = hit.point;
        }
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
        // Camera Reset
        if ( InputSystem.instance.playerInput.Player.Camera_Reset.IsPressed())
        {
            if (Free_CameParent.activeSelf)
            {
                Free_CameParent.transform.position = gameObject.transform.position;
            }
        }
        // Move
        if ( InputSystem.instance.playerInput.Player.Right_Mouse.IsPressed())
        {
            Vector3 faceDirection = mouseProject;
            // Spawn Particle
            // Instantiate(Target,mouseProject + new Vector3(0,1f,0), Quaternion.identity);
            Vector3 moveVelocity = mouseProject - transform.position;
            // Rotate Immediately
            agent.velocity = moveVelocity.normalized * agent.speed;
            // Walk goal
            agent.destination = mouseProject;
            Vector3 direction = mouseProject - transform.position;
            direction.y = 0;
            transform.LookAt(transform.position + direction);
        }
        // HandleRotation();
        HandleAnmation();
        
        // Passive skill
        Passive();
        if (InputSystem.instance.playerInput.Player.Camera_Reset.WasPressedThisFrame())
            Debug.Log("Was Just Pressed");

        if (InputSystem.instance.playerInput.Player.Camera_Reset.IsPressed())
            Debug.Log("Is Currently Pressed");

        if (InputSystem.instance.playerInput.Player.Camera_Reset.WasReleasedThisFrame())
            Debug.Log("Was Just Released");
        if (InputSystem.instance.playerInput.Player.Camera_Reset.triggered)
            Debug.Log("Triggered");
    }
    void OnEnable() 
    {
        InputSystem.instance.playerInput.Player.Enable();    
    }
    void OnDisable()
    {
        InputSystem.instance.playerInput.Player.Disable();
    }

    public override void InitialHealth()
    {
        currentHealth = maxHealth;
        MianInfoUI.instance.updateInfo(this);
        Selectable.instance.updateInfo(this);
    }
    public override void GetDamage(int damage)
    {
        currentHealth -= damage;
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
        InputSystem.instance.playerInput.Player.R.started += OnRKeyInput;
        isDead = false;
        // Health
        InitialHealth();
        yield return null;
    }
}
