using UnityEngine.InputSystem;
using UnityEngine;
using UnityEngine.AI;

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
    [Header("Camera ")]
    [Tooltip("Fix Camera on Character")]
    [SerializeField] protected GameObject Fixed_Cam;
    [Tooltip("Free Camera on Character")]
    [SerializeField] protected GameObject Free_CameParent;
    [Tooltip("Particle that show move target")]
    [SerializeField] protected ParticleSystem Target;
    protected Vector3 mouseProject;
    protected virtual void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
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
        if (!isLocalPlayer) return;
        Free_CameParent.SetActive(true);

        // Right Mouse
        InputSystem.instance.playerInput.Player.Right_Mouse.started += OnRightMouseClick;
        //playerInput.Player.Right_Mouse.canceled += OnRightMouseClick;
        //playerInput.Player.Right_Mouse.performed += OnRightMouseClick;

        // Left Mouse
        InputSystem.instance.playerInput.Player.Left_Mouse.started += OnLeftMouseClick;

        // Mous Position
        //playerInput.Player.MousePosition.started += OnMousePositionInput;
        //playerInput.Player.MousePosition.performed += OnMousePositionInput;
        //playerInput.Player.MousePosition.canceled += OnMousePositionInput;

        // Q skill
        InputSystem.instance.playerInput.Player.Q.started += OnQKeyDown;
        InputSystem.instance.playerInput.Player.Q.canceled += OnQKeyUp;
        
        // W skill
        InputSystem.instance.playerInput.Player.W.started += OnWKeyDown;
        InputSystem.instance.playerInput.Player.W.canceled += OnWKeyUp;
        
        // E skill
        InputSystem.instance.playerInput.Player.E.started += OnEKeyDown;
        InputSystem.instance.playerInput.Player.E.canceled += OnEKeyUp;

        // R skill
        InputSystem.instance.playerInput.Player.R.started += OnRKeyDown;
        InputSystem.instance.playerInput.Player.R.canceled += OnRKeyUp;

        InputSystem.instance.playerInput.Player.Camera_Change.started += OnYKeyClick;

        InputSystem.instance.playerInput.Player.Camera_Reset.started += OnSpaceKeyClick;
        InputSystem.instance.playerInput.Player.Camera_Reset.performed += OnSpaceKeyClick;
    }
    public virtual void OnRightMouseClick(InputAction.CallbackContext context)
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
            ParticleSystem temp = Instantiate(Target,mouseProject + new Vector3(0,0.01f,0), Quaternion.identity);
        }
    }
    public virtual void OnLeftMouseClick(InputAction.CallbackContext context)
    {
        // Check skill is Hovering
        if (IsPressed_Q)
        {
            // Hide Q preview
            Hide_Q_UI();
            OnQKeyUp(context);
        }
        else if (IsPressed_W)
        {
            // Hide W preview
            Hide_W_UI();
            OnWKeyUp(context);
        }
        else if (IsPressed_E)
        {
            // Hide E preview
            Hide_E_UI();
            OnEKeyUp(context);
        }
        else if (IsPressed_R)
        {
            // Hide R preview
            Hide_R_UI();
            OnRKeyUp(context);
        }
    }
    // Q skill
    public virtual void OnQKeyDown(InputAction.CallbackContext context)
    {
        IsPressed_Q = true;
    }
    public virtual void OnQKeyUp(InputAction.CallbackContext context)
    {
        IsPressed_Q = false;
    }
    // W skill
    public virtual void OnWKeyDown(InputAction.CallbackContext context)
    {
        IsPressed_W = true;
    }
    public virtual void OnWKeyUp(InputAction.CallbackContext context)
    {
        IsPressed_W = false;
    }
    // E skill
    public virtual void OnEKeyDown(InputAction.CallbackContext context)
    {
        IsPressed_E = true;
    }
    public virtual void OnEKeyUp(InputAction.CallbackContext context)
    {
        IsPressed_E = false;
    }
    // R skill
    public virtual void OnRKeyDown(InputAction.CallbackContext context)
    {
        IsPressed_R = true;
    }
    public virtual void OnRKeyUp(InputAction.CallbackContext context)
    {
        IsPressed_R = false;
    }
    public virtual void OnYKeyClick(InputAction.CallbackContext context)
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
    public virtual void OnSpaceKeyClick(InputAction.CallbackContext context)
    {
        if (!Free_CameParent.activeSelf) return;
        Free_CameParent.transform.position = gameObject.transform.position;
    }
    public virtual void Passive(){}
    // Skill Preview Hide
    protected virtual void Hide_Q_UI(){}
    protected virtual void Hide_W_UI(){}
    protected virtual void Hide_E_UI(){}
    protected virtual void Hide_R_UI(){}
    // Skill Use
    protected virtual void Q_Skill_Use(){}
    protected virtual void W_Skill_Use(){}
    protected virtual void E_Skill_Use(){}
    protected virtual void R_Skill_Use(){}
    public virtual void OnMousePositionInput(InputAction.CallbackContext context)
    {
        Vector3 mousePos = context.ReadValue<Vector2>();
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        if (Physics.Raycast(ray, out hit))
        {
            mouseProject = hit.point;
        }
    }
    protected void Get_Project_Mouse()
    {
        // check mouse raycast
        Vector3 mousePos = InputSystem.instance.playerInput.Player.MousePosition.ReadValue<Vector2>();
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        if (Physics.Raycast(ray, out hit,Mathf.Infinity,~LayerMask.NameToLayer("Land")))
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
}
