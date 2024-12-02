using UnityEngine.InputSystem;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using HR.UI;
using HR.Network.Game;
using Unity.VisualScripting;
using UnityEditor;
using System.Linq;

namespace HR.Object.Player{
[RequireComponent(typeof(NavMeshAgent))]
public class CharacterBase: Health
{
    [Header("Agent")]
    protected NavMeshAgent agent;

    [Header("Skill Pressed")]
    [SerializeField] protected bool IsPressed_Q = false;
    [SerializeField] protected bool IsPressed_W = false;
    [SerializeField] protected bool IsPressed_E = false;
    [SerializeField] protected bool IsPressed_R = false;
    [Header("Character Level")]
    [SerializeField] protected int Character_Level = 1;
    [Header("Skill Level")]
    [SerializeField] protected int Q_Level = 0;
    [SerializeField] protected int W_Level = 0;
    [SerializeField] protected int E_Level = 0;
    [SerializeField] protected int R_Level = 0;
    [Header("Camera")]
    [Tooltip("Fix Camera on Character")]
    [SerializeField] protected GameObject Fixed_Cam;
    [Tooltip("Free Camera on Character")]
    [SerializeField] protected GameObject Free_CameParent;
    [Header("Move Target")]
    [Tooltip("Particle that show move target")]
    [SerializeField] protected ParticleSystem Target_Particle;
    protected Vector3 mouseProject;
    [SerializeField] protected LayerMask MouseTargetLayer;

    [SerializeField] protected Transform Target;
    protected Ray ray;
    [Header("Attack")]
    [SerializeField] protected float Attack_Range;
    protected virtual void Awake()
    {
        // Outline NavMeshAgent Check
        if (!TryGetComponent<NavMeshAgent>(out agent))
        {
            Debug.LogError("CharacterBase must have a NavMeshAgent Component.",agent);
        }
        // Outline Component Check
        if (!TryGetComponent<Outline>(out Outline _))
        {
            Debug.LogError("CharacterBase must have a Outline Component.");
        }
        // Health Initial
        InitialHealth();
    }
    protected virtual void OnEnable() 
    {
        InputSystem.instance.playerInput.Player.Enable();
    }
    protected virtual void OnDisable()
    {
        InputSystem.instance.playerInput.Player.Disable();
    }
    protected virtual void Start()
    {
        DontDestroyOnLoad(gameObject);
        // Set layer
        Selectable.instance.playerLayerID = gameObject.layer;
        // Remove layer to mouse raycast only Enemy and Land
        MouseTargetLayer &= ~(1 <<gameObject.layer);
        if (!isLocalPlayer) return;
        // Set LocalPlayer for MiniMap
        GameController.Instance.LocalPlayer = this;
        Free_CameParent.SetActive(true);

        // Right Mouse
        InputSystem.instance.playerInput.Player.Right_Mouse.started += _ => OnRightMouseClick();
        //playerInput.Player.Right_Mouse.canceled += OnRightMouseClick;
        //playerInput.Player.Right_Mouse.performed += OnRightMouseClick;

        // Left Mouse
        InputSystem.instance.playerInput.Player.Left_Mouse.started += _ => OnLeftMouseClick();

        // Mous Position
        //playerInput.Player.MousePosition.started += OnMousePositionInput;
        //playerInput.Player.MousePosition.performed += OnMousePositionInput;
        //playerInput.Player.MousePosition.canceled += OnMousePositionInput;

        // Q skill
        InputSystem.instance.playerInput.Player.Q.started += _ => OnQKeyDown();
        InputSystem.instance.playerInput.Player.Q.canceled += _ => OnQKeyUp();
        
        // W skill
        InputSystem.instance.playerInput.Player.W.started += _ => OnWKeyDown();
        InputSystem.instance.playerInput.Player.W.canceled += _ => OnWKeyUp();
        
        // E skill
        InputSystem.instance.playerInput.Player.E.started += _ => OnEKeyDown();
        InputSystem.instance.playerInput.Player.E.canceled += _ => OnEKeyUp();

        // R skill
        InputSystem.instance.playerInput.Player.R.started += _ => OnRKeyDown();
        InputSystem.instance.playerInput.Player.R.canceled += _ => OnRKeyUp();

        // Camera Change
        InputSystem.instance.playerInput.Player.Camera_Change.started += _ => OnYKeyClick();

        // Camera Reset
        InputSystem.instance.playerInput.Player.Camera_Reset.started += _ => OnSpaceKeyClick();

        // Option
        InputSystem.instance.playerInput.Player.Option.started += _ => OnEscKeyClick();

        // Store
        InputSystem.instance.playerInput.Player.StoreKey.started += _ => OnPKeyClick();

        // Recall
        InputSystem.instance.playerInput.Player.Recall.started += _ => OnBKeyClick();
    }
    protected virtual void Update()
    {
        // Passive skill
        Passive();
        // RayCast Mouse
        Get_Project_Mouse();
        // Move
        CharacterMove();
        // Normal Attack
        Attack();
    }
    /// <summary>This is invoked when Mouse Right Click Down.</summary>
    public virtual void OnRightMouseClick()
    {
        // Check skill is Hovering
        if (IsPressed_Q)
        {
            IsPressed_Q = false;
            // Hide Q preview
            Hide_Q_UI();
        }
        else if (IsPressed_W)
        {
            IsPressed_W = false;
            // Hide W preview
            Hide_W_UI();
        }
        else if (IsPressed_E)
        {
            IsPressed_E = false;
            // Hide E preview
            Hide_E_UI();
        }
        else if (IsPressed_R)
        {
            IsPressed_R = false;
            // Hide R preview
            Hide_R_UI();
        }
        // Normal Walk
        else
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit,Mathf.Infinity,MouseTargetLayer))
            {
                // If Layer is Land -> Spawn Particle
                if (hit.transform.root.gameObject.layer == LayerMask.NameToLayer("Land"))
                {
                    Instantiate(Target_Particle,mouseProject + new Vector3(0,0.01f,0), Quaternion.identity);
                }
            }
        }
    }
    /// <summary>This is invoked when Mouse Left Click Down.</summary>
    public virtual void OnLeftMouseClick()
    {
        // Check skill is Previewing
        if (IsPressed_Q)
        {
            // Hide Q preview
            Hide_Q_UI();
            // Use Q Skill
            OnQKeyUp();
        }
        else if (IsPressed_W)
        {
            // Hide W preview
            Hide_W_UI();
            // Use W Skill
            OnWKeyUp();
        }
        else if (IsPressed_E)
        {
            // Hide E preview
            Hide_E_UI();
            // Use E Skill
            OnEKeyUp();
        }
        else if (IsPressed_R)
        {
            // Hide R preview
            Hide_R_UI();
            // Use R Skill
            OnRKeyUp();
        }
    }
    #region Skill Method
    // Q skill
    /// <summary>This is invoked when QKey Click Down.</summary>
    public virtual void OnQKeyDown()
    {
        IsPressed_Q = true;
    }
    /// <summary>This is invoked when QKey Click Up.</summary>
    public virtual void OnQKeyUp()
    {
        IsPressed_Q = false;
    }
    // W skill
    /// <summary>This is invoked when WKey Click Down.</summary>
    public virtual void OnWKeyDown()
    {
        IsPressed_W = true;
    }
    /// <summary>This is invoked when WKey Click Up.</summary>
    public virtual void OnWKeyUp()
    {
        IsPressed_W = false;
    }
    // E skill
    /// <summary>This is invoked when EKey Click Down.</summary>
    public virtual void OnEKeyDown()
    {
        IsPressed_E = true;
    }
    /// <summary>This is invoked when EKey Click Up.</summary>
    public virtual void OnEKeyUp()
    {
        IsPressed_E = false;
    }
    // R skill
    /// <summary>This is invoked when RKey Click Down.</summary>
    public virtual void OnRKeyDown()
    {
        IsPressed_R = true;
    }
    /// <summary>This is invoked when RKey Click Up.</summary>
    public virtual void OnRKeyUp()
    {
        IsPressed_R = false;
    }
    #endregion
    // Camera Change
    /// <summary>This is invoked when YKey Click Down.</summary>
    public virtual void OnYKeyClick()
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
    // Camera Reset
    /// <summary>This is invoked when SpaceKey Click Down.</summary>
    public virtual void OnSpaceKeyClick()
    {
        if (!Free_CameParent.activeSelf) return;
        Free_CameParent.transform.position = gameObject.transform.position;
    }
    public virtual void OnEscKeyClick()
    {
        // Show/Hide UI
        GameObject OptionUI = OptionPanel.Instance.gameObject;
        if (OptionUI.activeSelf)
        {
            OptionPanel.Instance.gameObject.SetActive(false);
        }
        else
        {
            OptionPanel.Instance.gameObject.SetActive(true);
        }
    }
    public virtual void OnPKeyClick()
    {}
    public virtual void OnBKeyClick()
    {}
    #region Skill UI
    /// <summary>Hide Q skill preview.</summary>
    protected virtual void Hide_Q_UI(){}
    /// <summary>Hide W skill preview.</summary>
    protected virtual void Hide_W_UI(){}
    /// <summary>Hide E skill preview.</summary>
    protected virtual void Hide_E_UI(){}
    /// <summary>Hide R skill preview.</summary>
    protected virtual void Hide_R_UI(){}
    #endregion
    // Passive Skill
    /// <summary>This method relate to Passive Skill.</summary>
    public virtual void Passive(){}
    /// <summary>This is invoked when Mouse Move. Now use "Get_Project_Mouse" to Update Project Point.</summary>
    public virtual void OnMousePositionInput()
    {
        Vector3 mousePos = InputSystem.instance.playerInput.Player.MousePosition.ReadValue<Vector2>();
        RaycastHit hit;
        ray = Camera.main.ScreenPointToRay(mousePos);
        if (Physics.Raycast(ray, out hit))
        {
            mouseProject = hit.point;
        }
    }
    /// <summary>This method calculate the project point from camera to scene object in Land Layer.</summary>
    protected void Get_Project_Mouse()
    {
        // check mouse raycast
        Vector3 mousePos = InputSystem.instance.playerInput.Player.MousePosition.ReadValue<Vector2>();
        ray = Camera.main.ScreenPointToRay(mousePos);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit,Mathf.Infinity,MouseTargetLayer))
        {
            mouseProject = hit.point;
        }
    }
    protected void Camera_Reset()
    {
        // Camera Reset
        if ( InputSystem.instance.playerInput.Player.Camera_Reset.IsPressed())
        {
            if (Free_CameParent.activeSelf)
            {
                Free_CameParent.transform.position = gameObject.transform.position;
            }
        }
    }
    protected void CharacterMove()
    {
        // Move
        // !EventSystem.current.IsPointerOverGameObject() to prevent on UI hover
        if ( InputSystem.instance.playerInput.Player.Right_Mouse.IsPressed() && !EventSystem.current.IsPointerOverGameObject())
        {
            // Spawn Particle -> Spawn in OnRightMouseClick
            // Instantiate(Target_Particle,mouseProject + new Vector3(0,1f,0), Quaternion.identity);

            // Calculate mouse Project
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit,Mathf.Infinity,MouseTargetLayer))
            {
                Vector3 AgentDestination;
                if (hit.transform.root.gameObject.layer == LayerMask.NameToLayer("Land"))
                {
                    AgentDestination = hit.point;
                    Target = null;
                }
                // Enemy Layer -> Set Enemy Transform and Calculate in CharacterMove()
                else
                {
                    AgentDestination = hit.transform.root.position;
                    Target = hit.transform.root;
                }
                Vector3 moveVelocity = AgentDestination - transform.position;
            
                // Rotate Immediately
                agent.velocity = moveVelocity.normalized * agent.speed;
                // Walk goal
                agent.destination = AgentDestination;
                moveVelocity.y = 0;
                // transform.LookAt(transform.position + moveVelocity);
            }
        }
        // If has target -> Update Target Position
        if (Target != null)
        {
            agent.destination = Target.position;
        }
    }
    protected void Attack()
    {
        // Check if Enemy reach attack range
        if (Target == null) return;
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, Attack_Range);
        if (hitColliders.Any(item => item.transform.root.name == Target.name))
        {
            agent.isStopped = true;
            // Face to Target
            Vector3 moveVelocity = Target.position - transform.position;
            moveVelocity.y = 0;
            transform.LookAt(transform.position + moveVelocity );
            // Normal Attack here
            NormalAttack();
        }
    }
    protected virtual void NormalAttack(){}
    /// Minimap Method
    public void Set_Destination(Vector3 position,bool SpawnParticle)
    {
        // Spawn Particle
        if (SpawnParticle) Instantiate(Target_Particle,position + new Vector3(0,0.01f,0), Quaternion.identity);
        Vector3 moveVelocity = position - transform.position;
        // Rotate Immediately
        agent.velocity = moveVelocity.normalized * agent.speed;
        // Walk goal
        agent.destination = position;
        moveVelocity.y = 0;
        // transform.LookAt(transform.position + moveVelocity);
    }
    public void Set_FreeCamera(Vector3 position)
    {
        if (Free_CameParent.activeSelf)
        {
            Free_CameParent.transform.position = position;
        }
    }
    public void Skill_Up(string skill)
    {
        switch (skill)
        {
            case "Q":
                Q_Level += 1;
                return;
            case "W":
                W_Level += 1;
                return;
            case "E":
                E_Level += 1;
                return;
            case "R":
                R_Level += 1;
                return;
        }
    }
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, Attack_Range);
    }
}

}