using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using HR.UI;
using HR.Network.Game;
using HR.Global;
using System.Collections;
using UnityEngine.VFX;
using static HR.UI.Skill_Icon;
using Mirror;
using HR.Object.Spell;
using HR.Network;

namespace HR.Object.Player{
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(CharacterSkillBase))]
public abstract class CharacterBase: Health
{
    [Header("Timer")]
    float HealthRegenTimer = 5f;
    float ManaRegenTimer = 5f;
    [Header("Mana / Energy")]
    [SyncVar] public int maxMana;
    [SyncVar(hook = nameof(Set_Mana))] public int currentMana = 1;

    [Header("Skill Mana Cost")]
    [SerializeField] protected int Q_Mana_Cost = 0;
    [SerializeField] protected int W_Mana_Cost = 0;
    [SerializeField] protected int E_Mana_Cost = 0;
    [SerializeField] protected int R_Mana_Cost = 0;

    [Header("Animator")]
    [SerializeField] protected Animator animator;
    [SerializeField] protected NetworkAnimator networkAnimator;

    [Header("Image Sprite")]
    public Sprite CharacterImage;
    [SerializeField] Sprite Q_skill_Image;
    [SerializeField] Sprite W_skill_Image;
    [SerializeField] Sprite E_skill_Image;
    [SerializeField] Sprite R_skill_Image;

    [Header("Economy")]
    public int ownMoney = 0;

    [Header("Agent")]
    public NavMeshAgent agent;
    [Header("Skillbase")]
    protected CharacterSkillBase skillComponent;

    [Header("Network Parameter")]
    [SyncVar] public int ConnectionID;
    [SyncVar] public int PlayerIdNumber;
    [SyncVar] public ulong PlayerSteamID;
    [SyncVar] public int TeamID;
    [SyncVar] public int CharacterID;
    [SyncVar] public string PlayerName;

    [Space(20)]
    [Header("Button Pressed Zone")]
    [Space(20)]

    [Header("Attack Pressed")]
    [SerializeField] protected bool IsPressed_Attack = false;
    [SerializeField] protected float Attack_Range;

    [Header("Skill Pressed")]
    [SerializeField] protected bool IsPressed_Q = false;
    [SerializeField] protected bool IsPressed_W = false;
    [SerializeField] protected bool IsPressed_E = false;
    [SerializeField] protected bool IsPressed_R = false;

    [Header("Spell Pressed")]
    [SerializeField] protected bool IsPressed_D = false;
    [SerializeField] protected bool IsPressed_F = false;
    [SyncVar] public SpellBase[] Spells = new SpellBase[2];

    [Header("Item Pressed")]
    [SerializeField] protected bool IsPressed_1 = false;
    [SerializeField] protected bool IsPressed_2 = false;
    [SerializeField] protected bool IsPressed_3 = false;
    [SerializeField] protected bool IsPressed_4 = false;
    [SerializeField] protected bool IsPressed_5 = false;
    [SerializeField] protected bool IsPressed_6 = false;
    [SyncVar] public Equipment_ScriptableObject[] EquipmentSlots = new Equipment_ScriptableObject[6];

    [Header("Store Item Pressed")]
    public bool CanPurchase = false;

    [Header("Camera")]
    [Tooltip("Fix Camera on Character")]
    [SerializeField] protected GameObject Fixed_Cam;
    [Tooltip("Free Camera on Character")]
    public GameObject Free_CameParent;
    [Header("Move Target")]
    [Tooltip("Particle that show move target")]
    [SerializeField] protected ParticleSystem Target_Particle;
    public Vector3 mouseProject;
    // prevent cancel and walk at the same time
    private bool isSkillCanceled = false;
    [SerializeField] protected LayerMask MouseTargetLayer;
    protected Ray ray;
    [Header("Dead Time")]
    float DeadTime = 3f;
    [Header("Recall")]
    float RecallTime = 8f;
    [SerializeField] protected VisualEffect RecallEffect;
    [SerializeField] protected bool isRecall = false;
    
    [Header("Status")]
    public int attack;
    public int defense;
    public float attackSpeed;
    public float moveSpeed;
    [SerializeField] protected int DefaultAttack;
    [SerializeField] protected int DefaultDefense;
    [SerializeField] protected float DefaultAttackSpeed;
    [SerializeField] protected float DefaultMoveSpeed;
    [SerializeField] protected float AgentWalkSpeed; // 3.5f
    
    [Header("KDA")]
    [SyncVar(hook = nameof(KDAChange))] public int kill = -1;
    [SyncVar(hook = nameof(KDAChange))] public int death = -1;
    [SyncVar(hook = nameof(KDAChange))] public int assist = -1;
    [Header("Number of Minions and Towers Destroyed")]
    [SyncVar(hook = nameof(KDAChange))] public int minion = -1;
    [SyncVar(hook = nameof(KDAChange))] public int tower = -1;

    [Header("Character Info")]
    AnimatorStateInfo stateInfo;
    private Network_Manager manager;

    public Network_Manager Manager
    {
        get
        {
            if (manager != null)
            {
                return manager;
            }
            return manager = Network_Manager.singleton as Network_Manager;
        }
    }
    protected override void Awake()
    {
        base.Awake();
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
        networkAnimator = GetComponent<NetworkAnimator>();
        Manager.Player_List.Add(this);
        DontDestroyOnLoad(gameObject);
        CanPurchase = true;
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
        // Set Layer
        int PlayerLayer = LayerMask.NameToLayer("Team" + TeamID.ToString());
        SetLayer(PlayerLayer);

        // Remove layer to mouse raycast only Enemy and Land
        MouseTargetLayer &= ~(1 <<gameObject.layer);
        if (LayerMask.LayerToName(gameObject.layer) == "Team1")
        {
            MouseTargetLayer &= ~(1 << LayerMask.NameToLayer("Team1Building"));
        }
        else
        {
            MouseTargetLayer &= ~(1 << LayerMask.NameToLayer("Team2Building"));
        }    

        if (!isLocalPlayer) return;

        // Set Skill UI and Spells
        MainInfoUI.instance.Character_Image.sprite = CharacterImage;
        MainInfoUI.instance.Q.Set_Skill_Icon(Q_skill_Image);
        MainInfoUI.instance.W.Set_Skill_Icon(W_skill_Image);
        MainInfoUI.instance.E.Set_Skill_Icon(E_skill_Image);
        MainInfoUI.instance.R.Set_Skill_Icon(R_skill_Image);
        MainInfoUI.instance.D.Set_Skill_Icon(Spells[0]?.Spell_Sprite);
        MainInfoUI.instance.F.Set_Skill_Icon(Spells[1]?.Spell_Sprite);

        // Set Level and exp add 1 from -1 to 0
        skillComponent.AddExp(1);

        // Set LocalPlayer for MiniMap, ShowPath, StorePanel
        GameController.Instance.LocalPlayer = this;
        ShowPath.Instance.LocalPlayer = this;
        StorePanel.Instance.LocalPlayer = this;
        MainInfoUI.instance.LocalPlayer = this;
        LocalPlayerInfo.Instance.Update_KDA(this);
        OptionPanel.Instance.LocalPlayer = this;
        StatusController.Instance.characterBase = this;
        Selectable.instance.LocalPlayer = this;

        // Set Spell Initial
        Spells[0].Set_Initial();
        Spells[1].Set_Initial();

        // Health Initial
        InitialHealth();
        // Mana Initial
        InitialMana();

        // Initial Info
        // attack = DefaultAttack;
        // defense = DefaultDefense;
        Update_Status(DefaultAttack,DefaultDefense);
        attackSpeed = DefaultAttackSpeed;
        moveSpeed = DefaultMoveSpeed;

        // Set Animation Speed and Agent Walk Speed
        agent.speed = AgentWalkSpeed * moveSpeed;
        animator.SetFloat("AttackSpeed",attackSpeed);
        animator.SetFloat("MoveSpeed",moveSpeed);

        Fixed_Cam.SetActive(true);
        // Set Recall time and IEnumerator
        RecallEffect.SetFloat("Duration",RecallTime);

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

        // D spell
        InputComponent.instance.playerInput.Player.D.started += _ => DKeyDown();
        InputComponent.instance.playerInput.Player.D.canceled += _ => DKeyUp();

        // F spell
        InputComponent.instance.playerInput.Player.F.started += _ => FKeyDown();
        InputComponent.instance.playerInput.Player.F.canceled += _ => FKeyUp();

        // A attack
        InputComponent.instance.playerInput.Player.A.started += _ => AKeyDown();
        InputComponent.instance.playerInput.Player.A.canceled += _ => AKeyUp();

        // S cancel walk
        InputComponent.instance.playerInput.Player.S.started += _ => SKeyDown();

        // Equipment Key
        InputComponent.instance.playerInput.Player.Equipment1.started += _ => UseEquipmentKeyDown(0);
        InputComponent.instance.playerInput.Player.Equipment2.started += _ => UseEquipmentKeyDown(1);
        InputComponent.instance.playerInput.Player.Equipment3.started += _ => UseEquipmentKeyDown(2);
        InputComponent.instance.playerInput.Player.Equipment4.started += _ => UseEquipmentKeyDown(3);
        InputComponent.instance.playerInput.Player.Equipment5.started += _ => UseEquipmentKeyDown(4);
        InputComponent.instance.playerInput.Player.Equipment6.started += _ => UseEquipmentKeyDown(5);

        InputComponent.instance.playerInput.Player.Equipment1.canceled += _ => UseEquipmentKeyUp(0);
        InputComponent.instance.playerInput.Player.Equipment2.canceled += _ => UseEquipmentKeyUp(1);
        InputComponent.instance.playerInput.Player.Equipment3.canceled += _ => UseEquipmentKeyUp(2);
        InputComponent.instance.playerInput.Player.Equipment4.canceled += _ => UseEquipmentKeyUp(3);
        InputComponent.instance.playerInput.Player.Equipment5.canceled += _ => UseEquipmentKeyUp(4);
        InputComponent.instance.playerInput.Player.Equipment6.canceled += _ => UseEquipmentKeyUp(5);

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

        // Animation keys
        InputComponent.instance.playerInput.Player.Animation1.started += _ => OnAnimationKeyDown(1);
        InputComponent.instance.playerInput.Player.Animation2.started += _ => OnAnimationKeyDown(2);
        InputComponent.instance.playerInput.Player.Animation3.started += _ => OnAnimationKeyDown(3);
        InputComponent.instance.playerInput.Player.Animation4.started += _ => OnAnimationKeyDown(4);
        InputComponent.instance.playerInput.Player.Animation5.started += _ => OnAnimationKeyDown(5);
        InputComponent.instance.playerInput.Player.Animation6.started += _ => OnAnimationKeyDown(6);

    }
    protected virtual void Update()
    {
        if (!isLocalPlayer) return;

        // Check Distance between character and Spawn Point
        if (CanPurchase)
        {
            float SpawnDistance = 0;
            if (gameObject.layer == LayerMask.NameToLayer("Team1"))
            {
                SpawnDistance = Vector3.Distance(transform.position, GameController.Instance.Team1_transform.position);
            }
            else if (gameObject.layer == LayerMask.NameToLayer("Team2"))
            {
                SpawnDistance = Vector3.Distance(transform.position, GameController.Instance.Team2_transform.position);
            }
            if (SpawnDistance > 10f)
            {
                CanPurchase = false;
            }
        }

        // Skill Reset
        if (MainInfoUI.instance != null)
        {
            SkillUpdate(false);
        }

        // Dead already and wait to respawn
        if (isDead) 
        {
            return;
        }
        // Dead now 
        if (currentHealth <= 0 && !isDead)
        {
            // Screen to black/white
            DeadScreen.instance.isDead(true);
            agent.isStopped = true;
            animator.SetBool("isDead",true);

            isDead = true;
            //****** Unregister control -> need to change to only skill
            InputComponent.instance.playerInput.Player.Disable();
            
            // Dead Time Start
            Death();
            return;
        }

        // Check Free Camera Reset -> Camera_Reset
        Camera_Reset();
        // Animation
        HandleMoveAnmation();
        // Passive skill
        Passive();
        // Auto Regeneration
        AutoRegen();
        // RayCast Mouse
        Get_Project_Mouse();
        // Auto Attack
        AutoAttack();
        // Move
        CharacterMove();
        // Normal Attack
        Attack();
    }
    protected virtual void SkillUpdate(bool isRespawn)
    {
        if (isRespawn) 
        {
            // Change all cool down to 0
        }
        else
        {
            // Update cool down per Update
        }
    }
    protected override void Death()
    {
        float dead_start_time = Time.time;
        Target = null;
        StartCoroutine(nameof(DeadCountDown),dead_start_time);
    }
    IEnumerator DeadCountDown(float dead_start_time)
    {
        while (Time.time - dead_start_time < DeadTime)
        {
            // Update UI wait time
            yield return null;
        }

        // Health Initial
        InitialHealth();
        // Mana Initial
        InitialMana();
        // Respawn
        if (gameObject.layer == LayerMask.NameToLayer("Team1"))
        {
            agent.Warp(GameController.Instance.Team1_transform.position);
        }
        else if (gameObject.layer == LayerMask.NameToLayer("Team2"))
        {
            agent.Warp(GameController.Instance.Team2_transform.position);
        }

        // Reset Skill Cooldown
        SkillUpdate(true);

        // Screen Control
        DeadScreen.instance.isDead(false);

        // Animation Control
        agent.isStopped = false;
        // animator.Play("Idle");
        animator.SetBool("isDead",false);

        // Register control
        InputComponent.instance.playerInput.Player.Enable();

        // Reset isDead
        isDead = false;

        // Reset CanPurchase
        CanPurchase = true;

        yield return null;
    }
        protected virtual void OnDestroy() 
    {
        // Reset all bindings
        InputComponent.instance.Reset();
        Destroy(Free_CameParent);    
    }
    public override void InitialHealth()
    {
        if (!isLocalPlayer) return;
        CmdSetlHealth(maxHealth);
    }
    [Command]
    public override void CmdSetlHealth(int NewHealth)
    {
        currentHealth = NewHealth;
    }
    public override void Set_Health(int OldValue, int NewValue)
    {
        base.Set_Health(OldValue, NewValue);
        if (!isLocalPlayer) return;
        MainInfoUI.instance.updateInfo();
    }
    /// <summary>
    /// Set currentMana to maxMana.
    /// </summary>
    public virtual void InitialMana()
    {
        if (isServer) currentMana = maxMana;
        else if (isClient) CmdSetlMana(maxMana);
    }
    /// <summary>
    /// Decrease health to currentMana.
    /// </summary>
    /// <param name="Cost">Decreased mana.</param>
    /// <returns>Is gameobject has enough mana or not.</returns>
    public virtual bool ManaReduced(int Cost)
    {
        if (currentMana < Cost) return false;
        if (isServer) currentMana -= Cost;
        else if (isClient) CmdSetlMana(currentMana - Cost);
        return true;
    }
    /// <summary>
    /// Add mana to currentMana.
    /// </summary>
    /// <param name="mana">Added mana.</param>
    public virtual void ManaRegen(int mana)
    {
        if (isServer) currentMana += mana;
        else if (isClient) CmdSetlMana(currentMana + mana);
        if (currentMana > maxMana)
        {
            currentMana = maxMana;
        }
    }
    /// <summary>
    /// Change currentMana from Client to Server.(Only set thing on Authority Object)
    /// </summary>
    /// <param name="NewMana">Changed currentMana.</param>
    [Command]
    public virtual void CmdSetlMana(int NewMana)
    {
        currentMana = NewMana;
    }
    public virtual void Set_Mana(int OldValue,int NewValue)
    {
        Selectable.instance.updateInfo(this);
        if (!isLocalPlayer) return;
        MainInfoUI.instance.updateInfo();
    }
    /// <summary>This is invoked when Mouse Right Click Down.</summary>
    public virtual void OnRightMouseClick()
    {
        if (IsPressed_Attack)
        {
            IsPressed_Attack = false;
        }
        // Check skill is Hovering
        if (IsPressed_Q)
        {
            IsPressed_Q = false;
            // Hide Q preview
            Hide_Q_UI();
            // Set flag to true to prevent walk
            isSkillCanceled = true;
        }
        else if (IsPressed_W)
        {
            IsPressed_W = false;
            // Hide W preview
            Hide_W_UI();
            // Set flag to true to prevent walk
            isSkillCanceled = true;
        }
        else if (IsPressed_E)
        {
            IsPressed_E = false;
            // Hide E preview
            Hide_E_UI();
            // Set flag to true to prevent walk
            isSkillCanceled = true;
        }
        else if (IsPressed_R)
        {
            IsPressed_R = false;
            // Hide R preview
            Hide_R_UI();
            // Set flag to true to prevent walk
            isSkillCanceled = true;
        }
        else if (IsPressed_R)
        {
            IsPressed_R = false;
            // Hide R preview
            Hide_R_UI();
            // Set flag to true to prevent walk
            isSkillCanceled = true;
        }
        else if (IsPressed_D)
        {
            IsPressed_D = false;
            // Cancel Spells
            Spells[0].Destroy_prefab();
            // Set flag to true to prevent walk
            isSkillCanceled = true;
        }
        else if (IsPressed_F)
        {
            IsPressed_F = false;
            // Hide R preview
            // Cancel Spells
            Spells[1].Destroy_prefab();
            // Set flag to true to prevent walk
            isSkillCanceled = true;
        }
        else if (IsPressed_1)
        {
            IsPressed_1 = false;
            isSkillCanceled = true;
            // Cancel Element 1
            if (EquipmentSlots[0] is Item_ScriptableObject)
            {
                Item_ScriptableObject tempItem = (Item_ScriptableObject)EquipmentSlots[0];
                tempItem.Destroy_prefab();
            }
        }
        else if (IsPressed_2)
        {
            IsPressed_2 = false;
            isSkillCanceled = true;
            // Cancel Element 2
            if (EquipmentSlots[1] is Item_ScriptableObject)
            {
                Item_ScriptableObject tempItem = (Item_ScriptableObject)EquipmentSlots[1];
                tempItem.Destroy_prefab();
            }
        }
        else if (IsPressed_3)
        {
            IsPressed_3 = false;
            isSkillCanceled = true;
            // Cancel Element 3
            if (EquipmentSlots[2] is Item_ScriptableObject)
            {
                Item_ScriptableObject tempItem = (Item_ScriptableObject)EquipmentSlots[2];
                tempItem.Destroy_prefab();
            }
        }
        else if (IsPressed_4)
        {
            IsPressed_4 = false;
            isSkillCanceled = true;
            // Cancel Element 4
            if (EquipmentSlots[3] is Item_ScriptableObject)
            {
                Item_ScriptableObject tempItem = (Item_ScriptableObject)EquipmentSlots[3];
                tempItem.Destroy_prefab();
            }
        }
        else if (IsPressed_5)
        {
            IsPressed_5 = false;
            isSkillCanceled = true;
            // Cancel Element 5
            if (EquipmentSlots[4] is Item_ScriptableObject)
            {
                Item_ScriptableObject tempItem = (Item_ScriptableObject)EquipmentSlots[4];
                tempItem.Destroy_prefab();
            }
        }
        else if (IsPressed_6)
        {
            IsPressed_6 = false;
            isSkillCanceled = true;
            // Cancel Element 6
            if (EquipmentSlots[5] is Item_ScriptableObject)
            {
                Item_ScriptableObject tempItem = (Item_ScriptableObject)EquipmentSlots[5];
                tempItem.Destroy_prefab();
            }
        }
        // Normal Walk
        else
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;
            isSkillCanceled = false;
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit,Mathf.Infinity,MouseTargetLayer))
            {
                // If Layer is Land -> Spawn Particle
                if (hit.transform.root.gameObject.layer == LayerMask.NameToLayer("Land"))
                {
                    // Local Spawn for LocalPlaer to See Walk Target
                    Instantiate(Target_Particle,mouseProject + new Vector3(0,0.01f,0), Quaternion.identity);
                }
            }
        }
    }
    /// <summary>This is invoked when Mouse Left Click Down.</summary>
    public virtual void OnLeftMouseClick()
    {
        if (IsPressed_Attack)
        {

            AKeyUp();
        }
        // Check skill is Previewing
        else if (IsPressed_Q)
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
        else if (IsPressed_D)
        {
            // Use D Spell
            DKeyUp();
        }
        else if (IsPressed_F)
        {
            // Use F Spell
            FKeyUp();
        }
        else if (IsPressed_1)
        {
            // Use Equipment 1
            UseEquipmentKeyUp(0);
        }
        else if (IsPressed_2)
        {
            UseEquipmentKeyUp(1);
        }
        else if (IsPressed_3)
        {
            UseEquipmentKeyUp(2);
        }
        else if (IsPressed_4)
        {
            UseEquipmentKeyUp(3);
        }
        else if (IsPressed_5)
        {
            UseEquipmentKeyUp(4);
        }
        else if (IsPressed_6)
        {
            UseEquipmentKeyUp(5);
        }
    }
    #region Skill Method
    // Q skill
    /// <summary>This is invoked when QKey Click Down.</summary>
    public void QKeyDown()
    {
        if (skillComponent.Q_Level == 0) return;
        if (currentMana < Q_Mana_Cost) return;
        if (!OnQKeyDown()) return;
        IsPressed_Q = true;
    }
    /// <summary>This is invoked when QKey Click Up.</summary>
    public void QKeyUp()
    {
        if (!IsPressed_Q) return;
        if (!OnQKeyUp()) return;
        Stop_Recall();
        IsPressed_Q = false;
    }
    // W skill
    /// <summary>This is invoked when WKey Click Down.</summary>
    public virtual void WKeyDown()
    {
        if (skillComponent.W_Level == 0) return;
        if (currentMana < W_Mana_Cost) return;
        if (!OnWKeyDown()) return;
        IsPressed_W = true;
    }
    /// <summary>This is invoked when WKey Click Up.</summary>
    public virtual void WKeyUp()
    {
        if (!IsPressed_W) return;
        if (!OnWKeyUp()) return;
        Stop_Recall();
        IsPressed_W = false;
    }
    // E skill
    /// <summary>This is invoked when EKey Click Down.</summary>
    public virtual void EKeyDown()
    {
        if (skillComponent.E_Level == 0) return;
        if (currentMana < E_Mana_Cost) return;
        if (!OnEKeyDown()) return;
        IsPressed_E = true;
    }
    /// <summary>This is invoked when EKey Click Up.</summary>
    public virtual void EKeyUp()
    {
        if (!IsPressed_E) return;
        if (!OnEKeyUp()) return;
        Stop_Recall();
        IsPressed_E = false;
        
    }
    // R skill
    /// <summary>This is invoked when RKey Click Down.</summary>
    public virtual void RKeyDown()
    {
        if (skillComponent.R_Level == 0) return;
        if (currentMana < R_Mana_Cost) return;
        if (!OnRKeyDown()) return;
        IsPressed_R = true;
    }
    /// <summary>This is invoked when RKey Click Up.</summary>
    public virtual void RKeyUp()
    {
        if (!IsPressed_R) return;
        if (!OnRKeyUp()) return;
        Stop_Recall();
        IsPressed_R = false;
    }
    // D Spell
    public virtual void DKeyDown()
    {
        if (Spells[0] != null)
        {
            IsPressed_D = true;
        }
        Stop_Recall();
        Spells[0].SpellKeyDown(this);
    }
    public virtual void DKeyUp()
    {
        if (!IsPressed_D) return;
        IsPressed_D = false;
        Spells[0].SpellKeyUp(this);
    }
    // F Spell
    public virtual void FKeyDown()    
    {
        if (Spells[1] != null)
        {
            IsPressed_F = true;
        }
        Stop_Recall();
        Spells[1].SpellKeyDown(this);
    }
    public virtual void FKeyUp()
    {
        if (!IsPressed_F) return;
        IsPressed_F = false;
        Spells[1].SpellKeyUp(this);
    }
    public virtual void AKeyDown()
    {
        IsPressed_Attack = true;
        // Attack Show Range
        
    }
    public virtual void AKeyUp()
    {
        if (!IsPressed_Attack) return;
        IsPressed_Attack = false;
        Stop_Recall();
        // Calculate mouse Project
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit,Mathf.Infinity,MouseTargetLayer))
        {
            // if mouseRaycast to Land Layer 
            if (hit.transform.root.gameObject.layer == LayerMask.NameToLayer("Land"))
            {
                // Layer Mask for enemy only
                LayerMask enemyLayer = MouseTargetLayer & ~(1 << LayerMask.NameToLayer("Land"));
                Vector3 moveVelocity;
                // Find hit.point near enemy
                Collider[] hitColliders = Physics.OverlapSphere(hit.point, Attack_Range, enemyLayer);
                Transform temp = Search_Nearest(hitColliders);
                if (temp != null)
                {
                    agent.isStopped = false;
                    Target = temp.transform.root;
                    agent.destination = Target.position;
                    moveVelocity = Target.position - transform.position;
                    // Rotate Immediately
                    agent.velocity = moveVelocity.normalized * agent.speed;
                    return;
                }
                // No Target near hit.point -> Find enemy near character(player)
                hitColliders = Physics.OverlapSphere(transform.position, Attack_Range, enemyLayer);
                temp = Search_Nearest(hitColliders);
                if (temp != null)
                {
                    agent.isStopped = false;
                    Target = temp.transform.root;
                    agent.destination = Target.position;
                    moveVelocity = Target.position - transform.position;
                    // Rotate Immediately
                    agent.velocity = moveVelocity.normalized * agent.speed;
                    return;
                }
                // No enemy near player -> walk to hit.point
                agent.isStopped = false;
                agent.destination = hit.point;
                Target = null;
                moveVelocity = hit.point - transform.position;
                // Rotate Immediately
                agent.velocity = moveVelocity.normalized * agent.speed;
                Instantiate(Target_Particle,hit.point + new Vector3(0,0.01f,0), Quaternion.identity);
                return;
            }
            // Enemy Layer -> Set Enemy Transform and Calculate in CharacterMove()
            else
            {
                agent.isStopped = false;
                Target = hit.transform.root;
                agent.destination = hit.transform.root.position;
                Vector3 moveVelocity = hit.transform.root.position - transform.position;
                // Rotate Immediately
                agent.velocity = moveVelocity.normalized * agent.speed;
            }
        }
    }
    public virtual void SKeyDown()
    {
        Target = null;
        agent.destination = transform.position;
        agent.isStopped = true;
        animator.SetBool("isAttack",false);
        animator.SetBool("isMove",false);
    }

    /// <summary>Return false to avoid skill use.</summary>
    public abstract bool OnQKeyDown();
    /// <summary>Return false to avoid skill use.</summary>
    public virtual bool OnQKeyUp()
    {
        return ManaReduced(Q_Mana_Cost);
    }
    /// <summary>Return false to avoid skill use.</summary>
    public abstract bool OnWKeyDown();
    /// <summary>Return false to avoid skill use.</summary>
    public virtual bool OnWKeyUp()
    {
        return ManaReduced(W_Mana_Cost);
    }
    /// <summary>Return false to avoid skill use.</summary>
    public abstract bool OnEKeyDown();
    /// <summary>Return false to avoid skill use.</summary>
    public virtual bool OnEKeyUp()
    {
        return ManaReduced(E_Mana_Cost);
    }
    /// <summary>Return false to avoid skill use.</summary>
    public abstract bool OnRKeyDown();
    /// <summary>Return false to avoid skill use.</summary>
    public virtual bool OnRKeyUp()
    {
        return ManaReduced(R_Mana_Cost);
    }
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
        // If Store Panel Show -> Hide
        if (StorePanel.Instance.gameObject.activeSelf)
        {
            StorePanel.Instance.gameObject.SetActive(false);
            return;
        }
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
    {
        // Show/Hide UI
        if (StorePanel.Instance.gameObject.activeSelf)
        {
            StorePanel.Instance.gameObject.SetActive(false);
        }
        else
        {
            StorePanel.Instance.gameObject.SetActive(true);
        }
    }
    public virtual void OnBKeyClick()
    {
        if (isRecall) return;
        isRecall = true;
        // Wait 8 seconds and warp back to spawn point (Just like dead)
        StartCoroutine(nameof(WaitRecall));
        // Particle Show
        RecallEffect.gameObject.SetActive(true);
    }
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
    public virtual void OnAnimationKeyDown(int AnimationID)
    {
        Target = null;
        agent.destination = transform.position;
        agent.isStopped = true;
        animator.SetBool("isAttack",false);
        animator.SetBool("isMove",false);
        animator.SetFloat("AnimationID",AnimationID);
        networkAnimator.SetTrigger("Animation");
    }

    #region Skill UI
    /// <summary>Hide Q skill preview.</summary>
    protected abstract void Hide_Q_UI();
    /// <summary>Hide W skill preview.</summary>
    protected abstract void Hide_W_UI();
    /// <summary>Hide E skill preview.</summary>
    protected abstract void Hide_E_UI();
    /// <summary>Hide R skill preview.</summary>
    protected abstract void Hide_R_UI();
    #endregion
    // Passive Skill
    /// <summary>This method relate to Passive Skill.</summary>
    protected abstract void Passive();
    /// <summary>This is invoked when Mouse Move. Now use "Get_Project_Mouse" to Update Project Point.</summary>
    // public virtual void OnMousePositionInput()
    // {
    //     Vector3 mousePos = InputComponent.instance.playerInput.Player.MousePosition.ReadValue<Vector2>();
    //     RaycastHit hit;
    //     ray = Camera.main.ScreenPointToRay(mousePos);
    //     if (Physics.Raycast(ray, out hit))
    //     {
    //         mouseProject = hit.point;
    //     }
    // }
    /// <summary>This method calculate the project point from camera to scene object in Land Layer.</summary>
    /// <summary>Regeneration Check.</summary>
    protected void AutoRegen()
    {
        if (currentHealth != maxHealth)
        {
            HealthRegenTimer -= Time.deltaTime;
            if (HealthRegenTimer <= 0)
            {
                HealthRegenTimer = 5f;
                HealthHeal(10);
            }
        }
        else
        {
            HealthRegenTimer = 5f;
        }
        if (currentMana != maxMana)
        {
            ManaRegenTimer -= Time.deltaTime;
            if (ManaRegenTimer <= 0)
            {
                ManaRegenTimer = 5f;
                ManaRegen(5);
            }
        }
        else
        {
            ManaRegenTimer = 5f;
        }
    }
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
        if (isSkillCanceled && !InputComponent.instance.playerInput.Player.Right_Mouse.IsPressed())
        {
            isSkillCanceled = false;
        }
        // Move
        // !EventSystem.current.IsPointerOverGameObject() to prevent on UI hover
        if ( InputComponent.instance.playerInput.Player.Right_Mouse.IsPressed() && !EventSystem.current.IsPointerOverGameObject())
        {
            // Check if a skill was canceled
            if (isSkillCanceled)
            {
                // Reset the flag
                return;
            }
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
                agent.isStopped = false;
                // Rotate Immediately
                agent.velocity = moveVelocity.normalized * agent.speed;
                // Walk goal
                agent.destination = AgentDestination;
                moveVelocity.y = 0;
                // transform.LookAt(transform.position + moveVelocity);
            }
            Stop_Recall();
        }
        // If has target -> Update Target Position
        // if (Target != null)
        // {
        //     agent.destination = Target.position;
        // }
    }
    protected void AutoAttack()
    {
        if(!OptionPanel.Instance.Get_AutoAttack()) return;
        // Attack only stop
        if (agent.velocity.magnitude > 0) return;
        LayerMask enemyLayer = MouseTargetLayer & ~(1 << LayerMask.NameToLayer("Land"));
        Collider[]hitColliders = Physics.OverlapSphere(transform.position, Attack_Range, enemyLayer);
        Transform temp = Search_Nearest(hitColliders);
        if (temp != null)
        {
            agent.isStopped = false;
            Target = temp.transform.root;
            agent.destination = Target.position;
            Vector3 moveVelocity = Target.position - transform.position;
            // Rotate Immediately
            agent.velocity = moveVelocity.normalized * agent.speed;
            return;
        }
    }
    protected void Attack()
    {
        // Check if Enemy reach attack range
        if (Target == null) 
        {
            animator.SetBool("isAttack",false);
            return;
        }
        float distance = Get_Target_Radius(Target);
        // If Target is in range -> stop and attack
        if ( distance < Attack_Range)
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
        animator.SetBool("isAttack",true);
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
    public void Skill_Up(Skill_Name skill)
    {
        switch (skill)
        {
            case Skill_Name.Q:
                skillComponent.Q_Level += 1;
                break;
            case Skill_Name.W:
                skillComponent.W_Level += 1;
                break;
            case Skill_Name.E:
                skillComponent.E_Level += 1;
                break;
            case Skill_Name.R:
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
    protected IEnumerator WaitRecall()
    {
        agent.destination = transform.position;
        // agent.isStopped = true;
        // Play Recall Animation
        // animator.Play("Recall");
        yield return new WaitForSeconds(RecallTime);
        if (gameObject.layer == LayerMask.NameToLayer("Team1"))
        {
            agent.Warp(GameController.Instance.Team1_transform.position);
        }
        else if (gameObject.layer == LayerMask.NameToLayer("Team2"))
        {
            agent.Warp(GameController.Instance.Team2_transform.position);
        }
        isRecall = false;
        // Particle Hide
        RecallEffect.gameObject.SetActive(false);
        OnRecall();
    }
    protected void Stop_Recall()
    {
        if (isRecall)
        {
            isRecall = false;
            StopCoroutine(nameof(WaitRecall));
            // Particle Hide
            RecallEffect.gameObject.SetActive(false);
        }
    }
    protected virtual void OnRecall(){}
    /// <summary> Add or Spend Money </summary>
    public void AddMoney(int money)
    {
        MoneyChange(money);
    }
    public void SpendMoney(int money)
    {
        MoneyChange(-money);
    }
    void MoneyChange(int money)
    {
        ownMoney += money;
        MainInfoUI.instance.updateInfo();
        StorePanel.Instance.Update_Money(this);
    }
    /// <summary> Buy or Add Equipment </summary>
    public void AddEquipItem(Equipment_ScriptableObject equipment, int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= EquipmentSlots.Length) return;

        // Initial Equipment
        EquipmentSlots[slotIndex] = equipment;
        UpdateStats();
        CharacterInfoPanel.Instance.UpdateUI();
    }
    /// <summary> Sell or Delete Equipment </summary>
    public void DeleteEquipItem(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= EquipmentSlots.Length) return;

        Equipment_ScriptableObject equipment = EquipmentSlots[slotIndex];
        if (equipment != null)
        {
            // Delete Equipment
            EquipmentSlots[slotIndex] = null;
            UpdateStats();
        }
        CharacterInfoPanel.Instance.UpdateUI();
    }
    /// <summary> Use Equipment </summary>
    public void UseEquipmentKeyDown(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= EquipmentSlots.Length) return;

        Equipment_ScriptableObject equipment = EquipmentSlots[slotIndex];
        if (equipment != null)
        {
            switch (slotIndex)
            {
                case 0:
                    IsPressed_1 = true;
                    break;
                case 1:
                    IsPressed_2 = true;
                    break;
                case 2:
                    IsPressed_3 = true;
                    break;
                case 3:
                    IsPressed_4 = true;
                    break;
                case 4:
                    IsPressed_5 = true;
                    break;
                case 5:
                    IsPressed_6 = true;
                    break;
            }
            // Check if the equipment is a potion
            if (equipment is Potion_ScriptableObject)
            {
                // Clear the equipment slot
                DeleteEquipItem(slotIndex);
            }
            equipment.ItemKeyDown(this);
            // Update UI
            MainInfoUI.instance.Update_Equipment();
        }
    }
    public void UseEquipmentKeyUp(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= EquipmentSlots.Length) return;
        Equipment_ScriptableObject equipment = EquipmentSlots[slotIndex];
        if (equipment != null)
        {
            switch (slotIndex)
            {
                case 0:
                    if (!IsPressed_1) return;
                    IsPressed_1 = false;
                    break;
                case 1:
                    if (!IsPressed_2) return;
                    IsPressed_2 = false;
                    break;
                case 2:
                    if (!IsPressed_3) return;
                    IsPressed_3 = false;
                    break;
                case 3:
                    if (!IsPressed_4) return;
                    IsPressed_4 = false;
                    break;
                case 4:
                    if (!IsPressed_5) return;
                    IsPressed_5 = false;
                    break;
                case 5:
                    if (!IsPressed_6) return;
                    IsPressed_6 = false;
                    break;
            }
            Stop_Recall();
            equipment.ItemKeyUp(this);
            // Update UI
            MainInfoUI.instance.Update_Equipment();
        }
    }
    /// <summary> Update Stats when change Equipment </summary>
    public void UpdateStats()
    {
        // Reset
        // attack = DefaultAttack;
        // defense = DefaultDefense;
        Update_Status(DefaultAttack,DefaultDefense);
        attackSpeed = DefaultAttackSpeed;
        moveSpeed = DefaultMoveSpeed;
        // Update with equipment
        foreach (Equipment_ScriptableObject equipment in EquipmentSlots)
        {
            if (equipment != null)
            {
                // Calculate Stats
                equipment.CharacterInfoChange(this);
            }
        }
        // Apply to Animation and Agent Move Speed
        agent.speed = AgentWalkSpeed * moveSpeed;
        animator.SetFloat("AttackSpeed",attackSpeed);
        animator.SetFloat("MoveSpeed",moveSpeed);
    }
    // Update Status to Server
    [Command]
    public void Update_Status(int attack,int defense)
    {
        this.attack = attack;
        this.defense = defense;
    }
    // Hook -> change UI
    void KDAChange(int oldValue, int newValue)
    {
        // CharacterInfoPanel.Instance.UpdateUI();
        if (!isOwned) return;
        LocalPlayerInfo.Instance.Update_KDA(this);
    }
    /// <summary>
    /// Set Kill, Death, Assist, Minion, Tower
    /// </summary>
    /// <param name="kill"></param>
    /// <param name="death"></param>
    /// <param name="assist"></param>
    /// <param name="minion"></param>
    /// <param name="tower"></param>
    [ServerCallback]
    public void SetKDA(int kill, int death, int assist, int minion, int tower)
    {
        this.kill = kill;
        this.death = death;
        this.assist = assist;
        this.minion = minion;
        this.tower = tower;
    }
    [ServerCallback]
    public void AddKDA(string KDAMT)
    {
        switch (KDAMT)
        {
            case "kill":
                kill += 1;
                break;
            case "death":
                death += 1;
                break;
            case "assist":
                assist += 1;
                break;
            case "minion":
                minion += 1;
                break;
            case "tower":
                tower += 1;
                break;
            default:
                break;
        }
    }
    public void SetLayer(int PlayerLayer)
    {
        // Set Layer to all child
        Transform[] children = gameObject.GetComponentsInChildren<Transform>(includeInactive: true);
        foreach(Transform child in children)
        {
            child.gameObject.layer = PlayerLayer;
        }
    }
    protected void HandleMoveAnmation()
    {
        // If stand Animation => stop move and rotate
        stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if(stateInfo.IsTag("stand"))
        {
            agent.isStopped = true;
            animator.SetBool("isMove",false);
            return;
        }
        else
        {
            agent.isStopped = false;
        }
        bool isRun = agent.velocity.magnitude > 0;
        // Run when idle
        if (isRun)
        {
            animator.SetBool("isMove",true);
        }
        // idle when run
        else
        {
            animator.SetBool("isMove",false);
        }
    }
}

}