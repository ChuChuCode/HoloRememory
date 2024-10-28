using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SubaruMovementController_old : MonoBehaviour,IHealth
{
    public List<Duck_AI> duck_array = new List<Duck_AI>();
    int max_duck_num = 6;
    [Header("Duck Skills")]
    [SerializeField] Duck_AI Duck_prefab;
    [SerializeField] Duck_Ult Duck_Ult;
    PlayerInputActions playerInput;
    CharacterController characterController;
    Vector3 mouseProject;
    [SerializeField] Animator animator;

    int isRunHash;

    Vector2 currentInput;
    Vector3 currentMovement;
    bool isMovementPressed;
    float rotationFactorPerFrame = 10.0f;
    float movespeed = 5f;
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
        characterController = GetComponent<CharacterController>();

        isRunHash = Animator.StringToHash("isMove");

        playerInput.Player_old.Move.started += OnMovementInput;
        playerInput.Player_old.Move.canceled += OnMovementInput;
        playerInput.Player_old.Move.performed += OnMovementInput;
        
        playerInput.Player_old.Special.started += OnMouseRigtClick;

        playerInput.Player_old.MousePosition.started += OnMousePositionInput;
        playerInput.Player_old.MousePosition.performed += OnMousePositionInput;
        playerInput.Player_old.MousePosition.canceled += OnMousePositionInput;

        playerInput.Player_old.Ult.started += OnRkeyUltInput;

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
    void OnMouseRigtClick(InputAction.CallbackContext context)
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
    void OnMovementInput(InputAction.CallbackContext context)
    {
        currentInput = context.ReadValue<Vector2>();
        currentMovement.x = - currentInput.x;
        currentMovement.z = - currentInput.y;
        isMovementPressed = currentInput.magnitude > 0;
    }
    void HandleAnmation()
    {
        bool isRun = animator.GetBool(isRunHash);
        // Run when idle
        if (isMovementPressed && !isRun)
        {
            animator.SetBool(isRunHash,true);
        }
        // idle when run
        else if (!isMovementPressed && isRun)
        {
            animator.SetBool(isRunHash,false);
        }

    }
    void HandleGravity()
    {
        if (characterController.isGrounded)
        {
            float groundedGravity = -0.05f;
            currentMovement.y = groundedGravity;
        }
        else
        {
            float groundedGravity = -9.8f;
            currentMovement.y += groundedGravity;
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

        stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if(stateInfo.IsTag("stand")) return;
        // move
        HandleGravity();
        HandleRotation();
        HandleAnmation();
        characterController.Move(currentMovement * Time.deltaTime * movespeed);

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
        playerInput.Player_old.Enable();    
    }
    void OnDisable()
    {
        playerInput.Player_old.Disable();
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
}
