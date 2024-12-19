using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using HR.UI;
using HR.Network.Game;
using System.Linq;
using HR.Global;

namespace HR.Object.Player{
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(CharacterSkillBase))]
public class CharacterBase: Health
{
    [Header("Animator")]
    [SerializeField] protected Animator animator;
    [Header("Agent")]
    public NavMeshAgent agent;
    protected CharacterSkillBase skillComponent;

    [Header("Skill Pressed")]
    [SerializeField] protected bool IsPressed_Q = false;
    [SerializeField] protected bool IsPressed_W = false;
    [SerializeField] protected bool IsPressed_E = false;
    [SerializeField] protected bool IsPressed_R = false;
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
        // NavMeshAgent Check
        if (!TryGetComponent<NavMeshAgent>(out agent))
        {
            Debug.LogError("CharacterBase must have a NavMeshAgent Component.",agent);
        }
        // Outline Component Check
        if (!TryGetComponent<Outline>(out Outline _))
        {
            Debug.LogError("CharacterBase must have a Outline Component.");
        }
        //  CharacterSkillBase Check
        if (!TryGetComponent<CharacterSkillBase>(out skillComponent))
        {
            Debug.LogError("CharacterBase must have a CharacterSkillBase Component.",skillComponent);
        }
        // Health Initial
        InitialHealth();
    }
    protected virtual void OnEnable() 
    {
        InputComponent.instance.playerInput.Player.Enable();
    }
    protected virtual void OnDisable()
    {
        InputComponent.instance.playerInput.Player.Disable();
    }
    protected virtual void Start()
    {
        DontDestroyOnLoad(gameObject);
        // Set layer
        Selectable.instance.playerLayerID = gameObject.layer;
        // Remove layer to mouse raycast only Enemy and Land
        MouseTargetLayer &= ~(1 <<gameObject.layer);
        if (!isLocalPlayer) return;
        // Set Level
        skillComponent.AddExp(0);
        // Set LocalPlayer for MiniMap
        GameController.Instance.LocalPlayer = this;
        Free_CameParent.SetActive(true);

        // Right Mouse
        InputComponent.instance.playerInput.Player.Right_Mouse.started += _ => OnRightMouseClick();
        //playerInput.Player.Right_Mouse.canceled += OnRightMouseClick;
        //playerInput.Player.Right_Mouse.performed += OnRightMouseClick;

        // Left Mouse
        InputComponent.instance.playerInput.Player.Left_Mouse.started += _ => OnLeftMouseClick();

        // Mous Position
        //playerInput.Player.MousePosition.started += OnMousePositionInput;
        //playerInput.Player.MousePosition.performed += OnMousePositionInput;
        //playerInput.Player.MousePosition.canceled += OnMousePositionInput;

        // Q skill
        InputComponent.instance.playerInput.Player.Q.started += _ => QKeyDown();
        InputComponent.instance.playerInput.Player.Q.canceled += _ => QKeyUp();
        
        // W skill
        InputComponent.instance.playerInput.Player.W.started += _ => WKeyDown();
        InputComponent.instance.playerInput.Player.W.canceled += _ => WKeyUp();
        
        // E skill
        InputComponent.instance.playerInput.Player.E.started += _ => EKeyDown();
        InputComponent.instance.playerInput.Player.E.canceled += _ => EKeyUp();

        // R skill
        InputComponent.instance.playerInput.Player.R.started += _ => RKeyDown();
        InputComponent.instance.playerInput.Player.R.canceled += _ => RKeyUp();

        // Camera Change
        InputComponent.instance.playerInput.Player.Camera_Change.started += _ => OnYKeyClick();

        // Camera Reset
        InputComponent.instance.playerInput.Player.Camera_Reset.started += _ => OnSpaceKeyClick();

        // Option
        InputComponent.instance.playerInput.Player.Option.started += _ => OnEscKeyClick();

        // Store
        InputComponent.instance.playerInput.Player.StoreKey.started += _ => OnPKeyClick();

        // Recall
        InputComponent.instance.playerInput.Player.Recall.started += _ => OnBKeyClick();
        
        // Skill Up Modifier
        InputComponent.instance.playerInput.Player.QLevelUp.canceled += _ => OnQKeyModifierUp();
        InputComponent.instance.playerInput.Player.WLevelUp.canceled += _ => OnWKeyModifierUp();
        InputComponent.instance.playerInput.Player.ELevelUp.canceled += _ => OnEKeyModifierUp();
        InputComponent.instance.playerInput.Player.RLevelUp.canceled += _ => OnRKeyModifierUp();

        // Tab for Player Info
        InputComponent.instance.playerInput.Player.Tab.started += _ => OnTabKeyDown();
        InputComponent.instance.playerInput.Player.Tab.canceled += _ => OnTabKeyUp();

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
            QKeyUp();
        }
        else if (IsPressed_W)
        {
            // Hide W preview
            Hide_W_UI();
            // Use W Skill
            WKeyUp();
        }
        else if (IsPressed_E)
        {
            // Hide E preview
            Hide_E_UI();
            // Use E Skill
            EKeyUp();
        }
        else if (IsPressed_R)
        {
            // Hide R preview
            Hide_R_UI();
            // Use R Skill
            RKeyUp();
        }
    }
    #region Skill Method
    // Q skill
    /// <summary>This is invoked when QKey Click Down.</summary>
    public void QKeyDown()
    {
        if (skillComponent.Q_Level == 0) return;
        if (!OnQKeyDown()) return;
        IsPressed_Q = true;
    }
    /// <summary>This is invoked when QKey Click Up.</summary>
    public void QKeyUp()
    {
        if (!IsPressed_Q) return;
        if (!OnQKeyUp()) return;
        IsPressed_Q = false;
    }
    // W skill
    /// <summary>This is invoked when WKey Click Down.</summary>
    public virtual void WKeyDown()
    {
        if (skillComponent.W_Level == 0) return;
        if (!OnWKeyDown()) return;
        IsPressed_W = true;
    }
    /// <summary>This is invoked when WKey Click Up.</summary>
    public virtual void WKeyUp()
    {
        if (!IsPressed_W) return;
        if (!OnWKeyUp()) return;
        IsPressed_W = false;
    }
    // E skill
    /// <summary>This is invoked when EKey Click Down.</summary>
    public virtual void EKeyDown()
    {
        if (skillComponent.E_Level == 0) return;
        if (!OnEKeyDown()) return;
        IsPressed_E = true;
    }
    /// <summary>This is invoked when EKey Click Up.</summary>
    public virtual void EKeyUp()
    {
        if (!IsPressed_E) return;
        if (!OnEKeyUp()) return;
        IsPressed_E = false;
    }
    // R skill
    /// <summary>This is invoked when RKey Click Down.</summary>
    public virtual void RKeyDown()
    {
        if (skillComponent.R_Level == 0) return;
        if (!OnRKeyDown()) return;
        IsPressed_R = true;
    }
    /// <summary>This is invoked when RKey Click Up.</summary>
    public virtual void RKeyUp()
    {
        if (!IsPressed_R) return;
        if (!OnRKeyUp()) return;
        IsPressed_R = false;
    }
    /// <summary>Return false to avoid skill use.</summary>
    public virtual bool OnQKeyDown(){return true;}
    /// <summary>Return false to avoid skill use.</summary>
    public virtual bool OnQKeyUp(){return true;}
    /// <summary>Return false to avoid skill use.</summary>
    public virtual bool OnWKeyDown(){return true;}
    /// <summary>Return false to avoid skill use.</summary>
    public virtual bool OnWKeyUp(){return true;}
    /// <summary>Return false to avoid skill use.</summary>
    public virtual bool OnEKeyDown(){return true;}
    /// <summary>Return false to avoid skill use.</summary>
    public virtual bool OnEKeyUp(){return true;}
    /// <summary>Return false to avoid skill use.</summary>
    public virtual bool OnRKeyDown(){return true;}
    /// <summary>Return false to avoid skill use.</summary>
    public virtual bool OnRKeyUp(){return true;}
    public void OnQKeyModifierUp()
    {
        // If Q Button is shown
        if (MainInfoUI.instance.Q.Level_Up_Button.gameObject.activeSelf)
        {
            MainInfoUI.instance.Q.Level_Up_Button.onClick.Invoke();
        }
    }
    public void OnWKeyModifierUp()
    {
        // If W Button is shown
        if (MainInfoUI.instance.W.Level_Up_Button.gameObject.activeSelf)
        {
            MainInfoUI.instance.W.Level_Up_Button.onClick.Invoke();
        }
    }
    public void OnEKeyModifierUp()
    {
        // If E Button is shown
        if (MainInfoUI.instance.E.Level_Up_Button.gameObject.activeSelf)
        {
            MainInfoUI.instance.E.Level_Up_Button.onClick.Invoke();
        }
    }
    public void OnRKeyModifierUp()
    {
        // If R Button is shown
        if (MainInfoUI.instance.R.Level_Up_Button.gameObject.activeSelf)
        {
            MainInfoUI.instance.R.Level_Up_Button.onClick.Invoke();
        }
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
        if (OptionPanel.Instance.gameObject.activeSelf)
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
    public virtual void OnTabKeyDown()
    {
        // Show UI
        CharacterInfoPanel.Instance.gameObject.SetActive(true);
        // Update Info
    }
    public virtual void OnTabKeyUp()
    {
        // Close UI
        CharacterInfoPanel.Instance.gameObject.SetActive(false);
    }
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
        Vector3 mousePos = InputComponent.instance.playerInput.Player.MousePosition.ReadValue<Vector2>();
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
        Vector3 mousePos = InputComponent.instance.playerInput.Player.MousePosition.ReadValue<Vector2>();
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
        if ( InputComponent.instance.playerInput.Player.Camera_Reset.IsPressed())
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
        if ( InputComponent.instance.playerInput.Player.Right_Mouse.IsPressed() && !EventSystem.current.IsPointerOverGameObject())
        {
            // Spawn Particle -> Spawn in OnRightMouseClick
            // Instantiate(Target_Particle,mouseProject + new Vector3(0,1f,0), Quaternion.identity);

            // Calculate mouse Project
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit,Mathf.Infinity,MouseTargetLayer))
            {
                Vector3 AgentDestination;
                // Land Layer -> Set Hit point as Destination
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
        if (Target == null) 
        {
            animator.SetTrigger("AttackStop");
            return;
        }
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, Attack_Range,MouseTargetLayer);
        // If Target is in range -> stop and attack
        if (hitColliders.Any(item => item.transform.root.name == Target.name))
        {
            // Not Move
            agent.destination = transform.position;
            agent.isStopped = true;
            // Face to Target
            Vector3 moveVelocity = Target.position - transform.position;
            moveVelocity.y = 0;
            transform.LookAt(transform.position + moveVelocity );
            // Normal Attack here
            NormalAttack();
        }
    }
    protected virtual void NormalAttack()
    {
        animator.Play("Attack");
    }
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
                skillComponent.Q_Level += 1;
                break;
            case "W":
                skillComponent.W_Level += 1;
                break;
            case "E":
                skillComponent.E_Level += 1;
                break;
            case "R":
                skillComponent.R_Level += 1;
                break;
        }
        // Check if still need to level up
        skillComponent.AddExp(0);
    }
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, Attack_Range);
    }
}

}