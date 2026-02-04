using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using HR.UI;
using HR.Network.Game;
using HR.Global;
using System.Collections;
using UnityEngine.VFX;
using Mirror;
using HR.Network;
using static UnityEngine.InputSystem.InputAction;
using HR.Object.Skill;

namespace HR.Object.Player{
// [RequireComponent(typeof(NavMeshAgent))]
// [RequireComponent(typeof(CharacterSkillBase))]
[RequireComponent(typeof(Outline))]
[RequireComponent(typeof(Rigidbody))]
public abstract class CharacterBase: Health
{
    [Header("Timer")]
    float ManaRegenTimer = 5f;
    [Header("Mana / Energy")]
    [SyncVar] public int maxMana;
    [SyncVar(hook = nameof(Set_Mana))] public int currentMana = 1;

    [Header("Animator")]
    [SerializeField] protected Animator animator;
    [SerializeField] protected NetworkAnimator networkAnimator;

    [Header("Image Sprite")]
    public Sprite CharacterImage;

    [Header("Economy")]
    public int ownMoney = 0;

    // [Header("Agent")]
    // public NavMeshAgent agent;
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

    // [Header("Camera")]
    // [Tooltip("Fix Camera on Character")]
    // [SerializeField] protected GameObject Fixed_Cam;
    // [Tooltip("Free Camera on Character")]
    // public GameObject Free_CameParent;
    [Header("Move Target")]
    [Tooltip("Particle that show move target")]
    [SerializeField] protected ParticleSystem Target_Particle;
    public Vector3 mouseProject;
    [SerializeField] protected LayerMask MouseTargetLayer;
    [Header("Dead Time")]
    float DeadTime = 3f;
    
    [Header("Status")]
    public int attack;
    public int defense;
    public float attackSpeed;
    public float moveSpeed;
    // [SerializeField] protected int DefaultAttack;
    // [SerializeField] protected int DefaultDefense;
    // [SerializeField] protected float DefaultAttackSpeed;
    // [SerializeField] protected float DefaultMoveSpeed;
    // [SerializeField] protected float AgentWalkSpeed; // 3.5f
    public int bombAmount;
    [SerializeField] protected BombBase Bomb_Prefab;
    [SerializeField] private Vector2 moveVector;
    [SerializeField] Rigidbody rd;
    
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
        if (!TryGetComponent<Rigidbody>(out rd))
        {
            Debug.LogError("CharacterBase must have a NavMeshAgent Component.",rd);
        }
        // Outline Component Check
        if (!TryGetComponent<Outline>(out Outline _))
        {
            Debug.LogError("CharacterBase must have a Outline Component.");
        }
        // //  CharacterSkillBase Check
        // if (!TryGetComponent<CharacterSkillBase>(out skillComponent))
        // {
        //     Debug.LogError("CharacterBase must have a CharacterSkillBase Component.",skillComponent);
        // }
        networkAnimator = GetComponent<NetworkAnimator>();
        Manager.Player_List.Add(this);
        DontDestroyOnLoad(gameObject);
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
        // MainInfoUI.instance.Character_Image.sprite = CharacterImage;

        // Set Level and exp add 1 from -1 to 0
        // skillComponent.AddExp(1);

        // Set LocalPlayer for MiniMap, ShowPath, StorePanel
        GameController.Instance.LocalPlayer = this;
        // ShowPath.Instance.LocalPlayer = this;
        // MainInfoUI.instance.LocalPlayer = this;
        // LocalPlayerInfo.Instance.Update_KDA(this);
        // OptionPanel.Instance.LocalPlayer = this;
        // StatusController.Instance.characterBase = this;

        // Health Initial
        // InitialHealth();
        // Mana Initial
        // InitialMana();

        // Initial Info
        // attack = DefaultAttack;
        // defense = DefaultDefense;
        // Update_Status(DefaultAttack,DefaultDefense);
        // attackSpeed = DefaultAttackSpeed;
        // moveSpeed = DefaultMoveSpeed;

        // Set Animation Speed and Agent Walk Speed
        // agent.speed = AgentWalkSpeed * moveSpeed;
        // animator.SetFloat("AttackSpeed",attackSpeed);
        // animator.SetFloat("MoveSpeed",moveSpeed);

        // Fixed_Cam.SetActive(true);

        // Move
        InputComponent.instance.playerInput.Player.Move.performed += CharacterMove;
        InputComponent.instance.playerInput.Player.Move.canceled += OnMovementCancelled;

        // Option
        InputComponent.instance.playerInput.Player.Option.started += _ => OnEscKeyClick();

        // Tab for Player Info
        InputComponent.instance.playerInput.Player.Tab.started += _ => OnTabKeyDown();
        InputComponent.instance.playerInput.Player.Tab.canceled += _ => OnTabKeyUp();

        InputComponent.instance.playerInput.Player.Bomb.performed += _ => NormalAttack();

        // Animation keys
        // InputComponent.instance.playerInput.Player.Animation1.started += _ => OnAnimationKeyDown(1);
        // InputComponent.instance.playerInput.Player.Animation2.started += _ => OnAnimationKeyDown(2);
        // InputComponent.instance.playerInput.Player.Animation3.started += _ => OnAnimationKeyDown(3);
        // InputComponent.instance.playerInput.Player.Animation4.started += _ => OnAnimationKeyDown(4);
        // InputComponent.instance.playerInput.Player.Animation5.started += _ => OnAnimationKeyDown(5);
        // InputComponent.instance.playerInput.Player.Animation6.started += _ => OnAnimationKeyDown(6);

    }
    protected virtual void Update()
    {
        if (!isLocalPlayer) return;

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
            // agent.isStopped = true;
            // animator.SetBool("isDead",true);

            isDead = true;
            //****** Unregister control -> need to change to only skill
            InputComponent.instance.playerInput.Player.Disable();
            
            // Dead Time Start
            Death();
            return;
        }

        // Check Free Camera Reset -> Camera_Reset
        // Camera_Reset();
        // Animation
        // HandleMoveAnmation();
        // Passive skill
        Passive();
        // Auto Regeneration
        // AutoRegen();
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
        // if (gameObject.layer == LayerMask.NameToLayer("Team1"))
        // {
        //     agent.Warp(GameController.Instance.Team1_transform.position);
        // }
        // else if (gameObject.layer == LayerMask.NameToLayer("Team2"))
        // {
        //     agent.Warp(GameController.Instance.Team2_transform.position);
        // }

        // Reset Skill Cooldown
        SkillUpdate(true);

        // Screen Control
        DeadScreen.instance.isDead(false);

        // Animation Control
        // agent.isStopped = false;
        // animator.Play("Idle");
        // animator.SetBool("isDead",false);

        // Register control
        InputComponent.instance.playerInput.Player.Enable();

        // Reset isDead
        isDead = false;

        yield return null;
    }
    protected virtual void OnDestroy() 
    {
        // Reset all bindings
        InputComponent.instance.Reset();
        // Destroy(Free_CameParent);    
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
        if (!isLocalPlayer) return;
        MainInfoUI.instance.updateInfo();
    }
    // Camera Change
    /// <summary>This is invoked when YKey Click Down.</summary>
    // public virtual void OnYKeyClick()
    // {
    //     // Fixed cam active -> Fixed cam deactive and free cam active
    //     if (Fixed_Cam.activeSelf)
    //     {
    //         Fixed_Cam.SetActive(false);
    //         Free_CameParent.SetActive(true);
    //         // set position to gameobject
    //         Free_CameParent.transform.position = gameObject.transform.position;
    //     }
    //     else
    //     {
    //         Fixed_Cam.SetActive(true);
    //         Free_CameParent.SetActive(false);
    //     }
    // } 
    // Camera Reset
    /// <summary>This is invoked when SpaceKey Click Down.</summary>
    // public virtual void OnSpaceKeyClick()
    // {
    //     if (!Free_CameParent.activeSelf) return;
    //     Free_CameParent.transform.position = gameObject.transform.position;
    // }
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
    // public virtual void OnAnimationKeyDown(int AnimationID)
    // {
    //     Target = null;
    //     agent.destination = transform.position;
    //     agent.isStopped = true;
    //     animator.SetBool("isAttack",false);
    //     animator.SetBool("isMove",false);
    //     animator.SetFloat("AnimationID",AnimationID);
    //     networkAnimator.SetTrigger("Animation");
    // }
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
    // protected void Camera_Reset()
    // {
    //     // Camera Reset
    //     if ( InputComponent.instance.playerInput.Player.Camera_Reset.IsPressed())
    //     {
    //         if (Free_CameParent.activeSelf)
    //         {
    //             Free_CameParent.transform.position = gameObject.transform.position;
    //         }
    //     }
    // }
    protected void CharacterMove(CallbackContext callback)
    {
        moveVector = callback.ReadValue<Vector2>();
    }
    protected void OnMovementCancelled(CallbackContext callback)
    {
        moveVector = Vector2.zero;
    }
    void FixedUpdate()
    {
        rd.velocity = new Vector3(moveVector.x, 0, moveVector.y) * moveSpeed;
    }
    protected virtual void NormalAttack()
    {
        if (bombAmount == 0) return;
        bombAmount -= 1;
        CmdSpawnBomb();
        
    }
    /// Minimap Method
    // public void Set_Destination(Vector3 position,bool SpawnParticle)
    // {
    //     // Spawn Particle
    //     if (SpawnParticle) Instantiate(Target_Particle,position + new Vector3(0,0.01f,0), Quaternion.identity);
    //     Vector3 moveVelocity = position - transform.position;
    //     // Rotate Immediately
    //     agent.velocity = moveVelocity.normalized * agent.speed;
    //     // Walk goal
    //     agent.destination = position;
    //     moveVelocity.y = 0;
    //     // transform.LookAt(transform.position + moveVelocity);
    // }
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
    }
    public void AddBombCount(int count)
    {
        bombAmount += count;
    }
    [Command]
    void CmdSpawnBomb()
    {
        // Calcuate Spawn Position, eq (0,0,0)~(1,0,1)) are all (0.5,0,0.5)
        Vector3 spawnPos = new Vector3(
            Mathf.Floor(transform.position.x) + 0.5f,
            0f,
            Mathf.Floor(transform.position.z) + 0.5f
        );
        BombBase bomb = Instantiate(Bomb_Prefab, spawnPos, Quaternion.identity);
        bomb.SetOwner(this);
        NetworkServer.Spawn(bomb.gameObject);
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
        // LocalPlayerInfo.Instance.Update_KDA(this);
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
    // protected void HandleMoveAnmation()
    // {
    //     // If stand Animation => stop move and rotate
    //     stateInfo = animator.GetCurrentAnimatorStateInfo(0);
    //     if(stateInfo.IsTag("stand"))
    //     {
    //         agent.isStopped = true;
    //         animator.SetBool("isMove",false);
    //         return;
    //     }
    //     else
    //     {
    //         agent.isStopped = false;
    //     }
    //     bool isRun = agent.velocity.magnitude > 0;
    //     // Run when idle
    //     if (isRun)
    //     {
    //         animator.SetBool("isMove",true);
    //     }
    //     // idle when run
    //     else
    //     {
    //         animator.SetBool("isMove",false);
    //     }
    // }
}

}