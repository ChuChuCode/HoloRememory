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
using HR.Map;

namespace HR.Object.Player{
// [RequireComponent(typeof(NavMeshAgent))]
// [RequireComponent(typeof(CharacterSkillBase))]
[RequireComponent(typeof(Outline))]
[RequireComponent(typeof(Rigidbody))]
public abstract class CharacterBase: Health
{
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

    [Header("Lives (set from the active GameModeConfig at spawn)")]
    [SyncVar] public int lives = 1;
    [SerializeField] float respawnDelay = 3f;
    [SyncVar(hook = nameof(OnWaitingToRespawnChanged))] bool isWaitingToRespawn;

    [Header("Status")]
    public int attack;
    public int defense;
    public float attackSpeed;
    public float moveSpeed;
    [SerializeField] float maxMoveSpeed = 10f;
    // [SerializeField] protected int DefaultAttack;
    // [SerializeField] protected int DefaultDefense;
    // [SerializeField] protected float DefaultAttackSpeed;
    // [SerializeField] protected float DefaultMoveSpeed;
    // [SerializeField] protected float AgentWalkSpeed; // 3.5f
    public int bombAmount;
    public int bombPower = 1;
    [SerializeField] protected BombBase Bomb_Prefab;
    bool isHoldingBomb;
    float holdStartTime;
    [SerializeField] float autoBombHoldThreshold = 0.15f; // a quick tap released before this never auto-places, no matter how far it moved
    [SerializeField] float bombPlaceCooldown = 0.05f;
    float nextBombPlaceTime;
    [SerializeField] private Vector2 moveVector;
    [SerializeField] Rigidbody rd;

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
        // FFA has no friend/foe distinction, so there's no need to sort
        // players onto separate Team1/Team2 layers for mouse-targeting
        // exclusion anymore (that whole system was MOBA-era and unused
        // elsewhere in the codebase besides this block).

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

        InputComponent.instance.playerInput.Player.Bomb.started += _ => StartHoldingBomb();
        InputComponent.instance.playerInput.Player.Bomb.canceled += _ => isHoldingBomb = false;

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
        if (isDead) return;

        // Skill Reset
        if (MainInfoUI.instance != null)
        {
            SkillUpdate(false);
        }

        // Holding the bomb button: once held past autoBombHoldThreshold (so a
        // quick tap - even one that covers real distance at high speed -
        // never triggers this), keep dropping a bomb in whatever cell you're
        // standing in the moment it's free of one. This covers both moving
        // to a fresh cell AND standing still waiting for your own bomb to
        // clear (bombAmount refunds on explosion, but that alone doesn't
        // change what cell you're in, so cell-change alone can't catch it).
        if (isHoldingBomb && Time.time - holdStartTime >= autoBombHoldThreshold && Time.time >= nextBombPlaceTime)
        {
            Vector2Int currentCell = GridManager.Instance.WorldToGrid(transform.position);
            if (bombAmount > 0 && !GridManager.Instance.IsOccupied(currentCell))
            {
                PlaceBomb();
            }
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
    void StartHoldingBomb()
    {
        isHoldingBomb = true;
        holdStartTime = Time.time;
        // Always honor an actual button press immediately, even if it's
        // within the cooldown window from a previous one - the cooldown is
        // only meant to stop the auto-place check above from double-firing,
        // not to throttle deliberate rapid taps at different spots.
        PlaceBomb();
    }
    void PlaceBomb()
    {
        NormalAttack();
        nextBombPlaceTime = Time.time + bombPlaceCooldown;
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
    // Spend a life instead of permanently dying, if any remain (MultiLife
    // mode). OneLife/HealthBar modes are both configured with lives = 1, so
    // this always falls straight through to permanent elimination for them.
    // isWaitingToRespawn (separate from isDead) drives the same hide/disable
    // reaction on every peer without touching CheckGameOver's "who's still
    // alive" count - a player mid-respawn-wait still has lives left.
    protected override void OnHealthDepleted()
    {
        lives -= 1;
        if (lives > 0)
        {
            isWaitingToRespawn = true;
            StartCoroutine(RespawnAfterDelay());
            return;
        }
        base.OnHealthDepleted(); // sets isDead = true -> Death() via the hook
    }
    IEnumerator RespawnAfterDelay()
    {
        yield return new WaitForSeconds(respawnDelay);

        InitialHealth();
        transform.position = GridManager.Instance.GetRandomSpawnPosition(TeamID);
        isWaitingToRespawn = false;
    }
    void OnWaitingToRespawnChanged(bool oldValue, bool newValue)
    {
        SetPresence(!newValue);

        if (!isLocalPlayer) return;

        DeadScreen.instance.isDead(newValue);
        if (newValue) InputComponent.instance.playerInput.Player.Disable();
        else InputComponent.instance.playerInput.Player.Enable();
    }
    void SetPresence(bool active)
    {
        rd.isKinematic = !active;
        foreach (Collider col in GetComponentsInChildren<Collider>())
        {
            col.enabled = active;
        }
        foreach (Renderer rend in GetComponentsInChildren<Renderer>())
        {
            rend.enabled = active;
        }
    }
    // Elimination is permanent for the round - fires on every peer (server +
    // all clients) via the isDead SyncVar hook, so a dead character stops
    // blocking movement/bombs for everyone, not just their own client.
    protected override void Death()
    {
        Target = null;
        SetPresence(false);

        if (isServer)
        {
            Manager.CheckGameOver();
        }

        if (!isLocalPlayer) return;

        DeadScreen.instance.isDead(true);
        InputComponent.instance.playerInput.Player.Disable();
    }
    protected virtual void OnDestroy() 
    {
        // Reset all bindings
        InputComponent.instance.Reset();
        // Destroy(Free_CameParent);    
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
        if (MainInfoUI.instance != null) MainInfoUI.instance.updateInfo();
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
    public void AddBombPower(int power)
    {
        bombPower += power;
    }
    public void AddMoveSpeed(float amount)
    {
        moveSpeed = Mathf.Min(moveSpeed + amount, maxMoveSpeed);
    }
    [Command]
    void CmdSpawnBomb()
    {
        // Snap to the same cell-center convention GridManager/GridSpawnerEditor use,
        // instead of a separate ad-hoc formula that could land between cells.
        Vector2Int coord = GridManager.Instance.WorldToGrid(transform.position);
        Vector3 spawnPos = GridManager.Instance.GridToWorld(coord);
        if (GridManager.Instance.IsOccupied(coord))
        {
            // Client already optimistically spent a bomb in NormalAttack(); give it back.
            TargetRefundBomb(connectionToClient);
            return;
        }

        BombBase bomb = Instantiate(Bomb_Prefab, spawnPos, Quaternion.identity);
        bomb.SetOwner(this);
        bomb.SetPower(bombPower);
        NetworkServer.Spawn(bomb.gameObject);
    }
    [TargetRpc]
    void TargetRefundBomb(NetworkConnection conn)
    {
        bombAmount += 1;
    }
    // Update Status to Server
    [Command]
    public void Update_Status(int attack,int defense)
    {
        this.attack = attack;
        this.defense = defense;
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