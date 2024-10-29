using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.AI;
using UnityEditor;

public class SubaruMovementController : MonoBehaviour,IHealth
{
    public List<Duck_AI> duck_array = new List<Duck_AI>();
    NavMeshAgent agent;
    int max_duck_num = 6;
    [Header("Duck Skills")]
    [SerializeField] Duck_AI Duck_prefab;
    [SerializeField] Duck_Ult Duck_Ult;
    PlayerInputActions playerInput;
    Vector3 mouseProject;
    [SerializeField] Animator animator;
    [SerializeField] ParticleSystem Target;

    int isRunHash;

    Vector2 currentInput;
    Vector3 currentMovement;
    bool isMovementPressed;
    float rotationFactorPerFrame = 10.0f;
    [Header("Skill Time")]
    float duck_spawn_cd = 10f;
    float duck_spawn_timer = 0f;
    float duck_rush_cd = 10f;
    float duck_rush_timer = -10f;
    float duck_ult_cd = 20f;
    float duck_ult_timer = -20f;
    [Header("Character Info")]
    [SerializeField] int maxHealth;
    [SerializeField] int currentHealth;
    AnimatorStateInfo stateInfo;

    void Awake()
    {
        playerInput = new PlayerInputActions();
        agent =GetComponent<NavMeshAgent>();

        isRunHash = Animator.StringToHash("isMove");

        playerInput.Player.Right_Mouse.started += OnRightMouseClick;
        //playerInput.Player.Right_Mouse.canceled += OnRightMouseClick;
        //playerInput.Player.Right_Mouse.performed += OnRightMouseClick;
        
        playerInput.Player.Q.started += OnKeyboardQClick;

        //playerInput.Player.MousePosition.started += OnMousePositionInput;
        //playerInput.Player.MousePosition.performed += OnMousePositionInput;
        //playerInput.Player.MousePosition.canceled += OnMousePositionInput;

        playerInput.Player.R.started += OnRkeyUltInput;

        InitialHealth();
        
    }

    void OnRkeyUltInput(InputAction.CallbackContext context)
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
    void OnRightMouseClick(InputAction.CallbackContext context)
    {
        // Vector3 faceDirection = mouseProject;
        // Spawn Particle
        ParticleSystem temp = Instantiate(Target,mouseProject + new Vector3(0,0.01f,0), Quaternion.identity);
        //faceDirection.y = 0f;
        // Rotate Immediately
        //agent.velocity = (faceDirection - transform.position).normalized * agent.speed;
        //transform.LookAt(faceDirection);
        // agent.destination = mouseProject;
    }
    void OnKeyboardQClick(InputAction.CallbackContext context)
    {
        if (Time.time - duck_rush_timer < duck_rush_cd) return;
        if (duck_array.Count == 0) return;
        animator.SetTrigger("Special");
        duck_rush_timer = Time.time;
        foreach (var duck in duck_array)
        {
            duck.rush_position = mouseProject;
            duck.rush_trigger = true;
        }
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
        // check mouse raycast
        Vector3 mousePos = playerInput.Player.MousePosition.ReadValue<Vector2>();
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        if (Physics.Raycast(ray, out hit))
        {
            mouseProject = hit.point;
        }
        // Move
        if ( playerInput.Player.Right_Mouse.IsPressed())
        {
            Vector3 faceDirection = mouseProject;
            // Spawn Particle
            // Instantiate(Target,mouseProject + new Vector3(0,1f,0), Quaternion.identity);
            Vector3 moveVelocity = mouseProject - transform.position;
            // Rotate Immediately
            agent.velocity = moveVelocity.normalized * agent.speed;

            transform.LookAt(faceDirection);
            agent.destination = mouseProject;
        }
        stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if(stateInfo.IsTag("stand")) return;
        // move
        // HandleRotation();
        HandleAnmation();

        // Passive skill
        if (Duck_Spawnable() && duck_array.Count < max_duck_num )
        {
            duck_spawn_timer = Time.time ;
            Vector3 pos = transform.position + 
                        new Vector3(
                            UnityEngine.Random.Range(-1f,1f) * Duck_AI.master_radius,
                            0f,
                            UnityEngine.Random.Range(-1f,1f) * Duck_AI.master_radius);
            Duck_AI duck = Instantiate(Duck_prefab,pos,transform.rotation);
            duck.player = this.gameObject;
            duck_array.Add(duck);
        }
    }
    void OnEnable() 
    {
        playerInput.Player.Enable();    
    }
    void OnDisable()
    {
        playerInput.Player.Disable();
    }

    public void InitialHealth()
    {
        currentHealth = maxHealth;
    }
    public void GetDamage(int damage)
    {
        currentHealth -= damage;
    }

    public void Death()
    {
        
    }
    void Start()
    {
        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/cursor_g .png");
        Cursor.SetCursor(tex,new Vector2(0.5f,0.5f), CursorMode.ForceSoftware );

    }
}
