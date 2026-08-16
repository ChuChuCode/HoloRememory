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
[RequireComponent(typeof(CharacterSkillBase))]
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
    public CharacterSkillBase SkillComponent => skillComponent;

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

    [Header("Move Target")]
    [Tooltip("Particle that show move target")]
    [SerializeField] protected ParticleSystem Target_Particle;
    public Vector3 mouseProject;

    [Header("Lives (set from the active GameModeConfig at spawn)")]
    [SyncVar] public int lives = 1;
    [SerializeField] float respawnDelay = 3f;
    [SyncVar(hook = nameof(OnWaitingToRespawnChanged))] bool isWaitingToRespawn;

    [Header("Status")]
    public float moveSpeed;
    [SerializeField] float maxMoveSpeed = 10f;
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
    // Last non-zero move direction, snapped to a grid cardinal axis - lets
    // skills (e.g. Korone's Jump) know which way to act without a separate
    // facing/rotation system.
    Vector2Int facingDir = Vector2Int.down;
    public Vector2Int FacingDir => facingDir;

    [Header("Debuff")]
    [SerializeField] float debuffDuration = 10f;
    [Tooltip("The character's own visible body mesh - flashed while a debuff is active so everyone (not just the affected player) can see it. Not the GridHighlight or Outline - just the colored body.")]
    [SerializeField] Renderer bodyRenderer;
    [SerializeField] Color debuffFlashColor = Color.white;
    [SerializeField] float debuffFlashInterval = 0.15f;
    bool forcedAutoBomb;
    bool controlsReversed;
    Coroutine forceAutoBombRoutine;
    Coroutine reverseControlsRoutine;
    Coroutine debuffFlashRoutine;
    Color bodyOriginalColor;
    bool bodyColorCaptured;
    bool isSkillLocked;

    // A mount absorbs the next bomb hit instead of costing a life - see
    // HealthDamage/Dismount. currentMount is the server's own authoritative
    // copy (used for the CanPickupItems gate and to decide what Dismount
    // does); mountSpeedModifier is a client-local mirror of just the one
    // value FixedUpdate actually needs, pushed over by TargetApplyMount
    // since movement is client-authoritative.
    [Header("Mount")]
    [SerializeField] float mountStaggerDuration = 0.5f;
    [Tooltip("Placeholder stand-in for a real mount model/sit animation - a plain shape tinted per-mount via MountData.VisualColor. Shared across all characters.")]
    [SerializeField] GameObject mountVisualPrefab;
    [Tooltip("Peak height of the placeholder hop played on mounting/getting knocked off - same arc shape as Korone's Jump, just standing in for a real animation.")]
    [SerializeField] float mountHopHeight = 0.4f;
    [Tooltip("How much higher the character sits once mounted - placeholder for actually raising the model onto a mount, since there's no sit animation yet.")]
    [SerializeField] float mountRideHeight = 0.3f;
    MountData currentMount;
    float mountSpeedModifier;
    bool isBombImmune;
    GameObject mountVisualInstance;
    // Client-local mirror of "is currently mounted" - the elevated ride
    // height has nothing solid under it, so gravity has to stay off for the
    // whole ride, not just the brief hop/stagger window (see SetSkillLock).
    bool isMountedLocally;
    public bool CanPickupItems => currentMount == null || currentMount.CanPickupItems;

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
        // CharacterSkillBase Check
        if (!TryGetComponent<CharacterSkillBase>(out skillComponent))
        {
            Debug.LogError("CharacterBase must have a CharacterSkillBase Component.");
        }
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

        // Skill - gated entirely by skillComponent's own energy check, not
        // by anything here.
        InputComponent.instance.playerInput.Player.Skill.started += _ => skillComponent.TryActivate();
    }
    protected virtual void Update()
    {
        if (!isLocalPlayer) return;
        if (isDead) return;
        // Otherwise this polled loop (unlike the disabled Input actions)
        // would keep running during the respawn wait - e.g. still
        // auto-placing bombs if the button was held when this life was lost.
        if (isWaitingToRespawn) return;

        // Holding the bomb button: once held past autoBombHoldThreshold (so a
        // quick tap - even one that covers real distance at high speed -
        // never triggers this), keep dropping a bomb in whatever cell you're
        // standing in the moment it's free of one. This covers both moving
        // to a fresh cell AND standing still waiting for your own bomb to
        // clear (bombAmount refunds on explosion, but that alone doesn't
        // change what cell you're in, so cell-change alone can't catch it).
        // forcedAutoBomb (the Chaos debuff) skips the hold threshold - it
        // should start spamming immediately, not wait for a real hold.
        bool wantsAutoPlace = forcedAutoBomb || (isHoldingBomb && Time.time - holdStartTime >= autoBombHoldThreshold);
        if (wantsAutoPlace && Time.time >= nextBombPlaceTime)
        {
            Vector2Int currentCell = GridManager.Instance.WorldToGrid(transform.position);
            if (bombAmount > 0 && !GridManager.Instance.IsOccupied(currentCell))
            {
                PlaceBomb();
            }
        }

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
    // A mount absorbs the hit instead of it reaching health/lives at all -
    // ExplosionSegment is the only damage source in this game, and it always
    // goes through here. isBombImmune additionally covers the brief window
    // right after mounting/dismounting, so getting knocked off doesn't
    // immediately chain into a second, un-absorbed hit from the same blast.
    public override bool HealthDamage(int damage)
    {
        if (isBombImmune) return false;
        if (currentMount != null)
        {
            Dismount();
            return false;
        }
        return base.HealthDamage(damage);
    }
    // Spend a life instead of permanently dying, if any remain (MultiLife
    // mode). OneLife/HealthBar modes are both configured with lives = 1, so
    // this always falls straight through to permanent elimination for them.
    // isWaitingToRespawn (separate from isDead) drives the same hide/disable
    // reaction on every peer without touching CheckGameOver's "who's still
    // alive" count - a player mid-respawn-wait still has lives left.
    protected override void OnHealthDepleted()
    {
        // Debuffs run on a plain timer on the owning client with no death
        // check of their own - without this, dying mid-debuff would still
        // leave forcedAutoBomb/controlsReversed active into the next life
        // (or forever, for a permanent death) until their original duration
        // happened to run out.
        TargetClearDebuffs(connectionToClient);
        RpcStopDebuffFlash();
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
        transform.position = GridManager.Instance.GetRandomSpawnPosition();
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
    // Called on every client the instant this character's owner
    // disconnects mid-match - kept separate from Death()/OnHealthDepleted
    // since a disconnect isn't triggered by taking damage. Empty for now;
    // Network_Manager calls this (server-side) before the disconnecting
    // connection's object actually gets torn down.
    //
    // TODO(disconnect self-destruct): waiting on real animation/timing
    // before building this out. Planned behavior:
    //   1. Character should NOT vanish immediately - stay visible/in place.
    //   2. Some UI should update to show this player as "disconnected"
    //      (exact UI element still undecided - no in-match player-roster
    //      HUD exists yet to hang this off of).
    //   3. Play a "self-destruct" animation on the character.
    //   4. Only after the animation finishes should the object actually be
    //      removed - this needs Network_Manager.OnServerDisconnect to stop
    //      relying on Mirror's default base.OnServerDisconnect() (which
    //      destroys the connection's object immediately) and instead defer
    //      the NetworkServer.Destroy() call until the animation completes.
    [ClientRpc]
    public void RpcOnDisconnect()
    {
        OnDisconnect();
    }
    protected virtual void OnDisconnect()
    {
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
    public virtual void OnEscKeyClick()
    {
        // OptionPanel (old MOBA UI) isn't placed anywhere in the current
        // scene and isn't null-safe enough to partially activate -
        // LeaveGamePanel is the minimal replacement actually used here.
        if (LeaveGamePanel.Instance == null) return;

        // Show/Hide UI
        LeaveGamePanel.Instance.SetVisible(!LeaveGamePanel.Instance.IsVisible);
    }
    public virtual void OnTabKeyDown()
    {
        // CharacterInfoPanel (old MOBA UI) isn't placed anywhere in the
        // current scene yet - guard so this doesn't throw once Tab's own
        // input binding actually fires.
        if (CharacterInfoPanel.Instance == null) return;
        CharacterInfoPanel.Instance.gameObject.SetActive(true);
    }
    public virtual void OnTabKeyUp()
    {
        if (CharacterInfoPanel.Instance == null) return;
        CharacterInfoPanel.Instance.gameObject.SetActive(false);
    }
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
    protected void CharacterMove(CallbackContext callback)
    {
        moveVector = callback.ReadValue<Vector2>();
        if (controlsReversed) moveVector = -moveVector;
        if (moveVector != Vector2.zero)
        {
            facingDir = Mathf.Abs(moveVector.x) > Mathf.Abs(moveVector.y)
                ? new Vector2Int(moveVector.x > 0 ? 1 : -1, 0)
                : new Vector2Int(0, moveVector.y > 0 ? 1 : -1);
        }
    }
    protected void OnMovementCancelled(CallbackContext callback)
    {
        moveVector = Vector2.zero;
    }
    void FixedUpdate()
    {
        // Same guards as Update() - without these, this keeps running for
        // dead/respawn-waiting characters (whose Rigidbody just got set
        // isKinematic = true by SetPresence), and Unity rejects setting
        // velocity on a kinematic body.
        if (!isLocalPlayer) return;
        if (isDead) return;
        if (isWaitingToRespawn) return;
        if (isSkillLocked) return;
        float effectiveSpeed = Mathf.Max(0f, moveSpeed + mountSpeedModifier);
        rd.velocity = new Vector3(moveVector.x, 0, moveVector.y) * effectiveSpeed;

        // Face the direction actually being moved in - keeps whatever
        // direction it was last facing while standing still, same as
        // facingDir (used by Jump/Bomb Push) already does. Direct transform
        // assignment, not physics, so the Rigidbody's frozen rotation
        // constraints don't fight it; replicated to other clients via the
        // same NetworkTransform that already syncs rotation.
        if (moveVector != Vector2.zero)
        {
            transform.rotation = Quaternion.LookRotation(new Vector3(moveVector.x, 0, moveVector.y));
        }
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
    // bombAmount/bombPower/moveSpeed aren't SyncVars (see the comment on
    // bombAmount's declaration) - these are only ever called server-side
    // (Item.Apply, via OnTriggerEnter's [ServerCallback]), so without this
    // TargetRpc a non-host client's own copy would never actually change.
    // For bombPower this was a silent gap (the server's own copy is what
    // CmdSpawnBomb reads, so the blast itself was already correct) - but for
    // bombAmount specifically it was gameplay-breaking: NormalAttack's
    // bombAmount==0 check runs client-side, so a remote player picking up a
    // BombCount item would still be unable to place the extra bomb at all.
    [Server]
    public void AddBombCount(int count)
    {
        bombAmount += count;
        TargetSyncBombAmount(connectionToClient, bombAmount);
    }
    [Server]
    public void AddBombPower(int power)
    {
        bombPower += power;
        TargetSyncBombPower(connectionToClient, bombPower);
    }
    [Server]
    public void AddMoveSpeed(float amount)
    {
        moveSpeed = Mathf.Min(moveSpeed + amount, maxMoveSpeed);
        TargetSyncMoveSpeed(connectionToClient, moveSpeed);
    }
    [TargetRpc]
    void TargetSyncBombAmount(NetworkConnection target, int amount)
    {
        bombAmount = amount;
    }
    [TargetRpc]
    void TargetSyncBombPower(NetworkConnection target, int power)
    {
        bombPower = power;
    }
    [TargetRpc]
    void TargetSyncMoveSpeed(NetworkConnection target, float speed)
    {
        moveSpeed = speed;
    }
    // Chaos item debuff - server picks one of two equally likely effects and
    // pushes it to the owning client only (both movement input and the
    // auto-place loop only ever run on isLocalPlayer, so nothing needs to
    // reach anyone else).
    [Server]
    public void ApplyRandomDebuff()
    {
        if (Random.value < 0.5f) TargetForceAutoBomb(connectionToClient, debuffDuration);
        else TargetReverseControls(connectionToClient, debuffDuration);
        // Everyone should see this, not just the affected player - the
        // actual gameplay effect above is TargetRpc'd only to them, but the
        // flash itself is just a visual tell.
        RpcStartDebuffFlash(debuffDuration);
    }
    [ClientRpc]
    void RpcStartDebuffFlash(float duration)
    {
        if (debuffFlashRoutine != null) StopCoroutine(debuffFlashRoutine);
        debuffFlashRoutine = StartCoroutine(DebuffFlashRoutine(duration));
    }
    [ClientRpc]
    void RpcStopDebuffFlash()
    {
        if (debuffFlashRoutine != null)
        {
            StopCoroutine(debuffFlashRoutine);
            debuffFlashRoutine = null;
        }
        RestoreBodyColor();
    }
    IEnumerator DebuffFlashRoutine(float duration)
    {
        if (bodyRenderer == null) yield break;
        if (!bodyColorCaptured)
        {
            bodyOriginalColor = bodyRenderer.sharedMaterial.GetColor("_BaseColor");
            bodyColorCaptured = true;
        }
        MaterialPropertyBlock block = new MaterialPropertyBlock();
        bool flashOn = false;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            flashOn = !flashOn;
            block.SetColor("_BaseColor", flashOn ? debuffFlashColor : bodyOriginalColor);
            bodyRenderer.SetPropertyBlock(block);
            yield return new WaitForSeconds(debuffFlashInterval);
            elapsed += debuffFlashInterval;
        }
        RestoreBodyColor();
        debuffFlashRoutine = null;
    }
    void RestoreBodyColor()
    {
        if (bodyRenderer == null || !bodyColorCaptured) return;
        MaterialPropertyBlock block = new MaterialPropertyBlock();
        block.SetColor("_BaseColor", bodyOriginalColor);
        bodyRenderer.SetPropertyBlock(block);
    }
    [TargetRpc]
    void TargetForceAutoBomb(NetworkConnection target, float duration)
    {
        if (forceAutoBombRoutine != null) StopCoroutine(forceAutoBombRoutine);
        forceAutoBombRoutine = StartCoroutine(ForceAutoBombRoutine(duration));
    }
    IEnumerator ForceAutoBombRoutine(float duration)
    {
        forcedAutoBomb = true;
        yield return new WaitForSeconds(duration);
        forcedAutoBomb = false;
        forceAutoBombRoutine = null;
    }
    [TargetRpc]
    void TargetReverseControls(NetworkConnection target, float duration)
    {
        if (reverseControlsRoutine != null) StopCoroutine(reverseControlsRoutine);
        reverseControlsRoutine = StartCoroutine(ReverseControlsRoutine(duration));
    }
    IEnumerator ReverseControlsRoutine(float duration)
    {
        controlsReversed = true;
        yield return new WaitForSeconds(duration);
        controlsReversed = false;
        reverseControlsRoutine = null;
    }
    // Called the instant health hits 0 (see OnHealthDepleted) - stops
    // whichever debuff coroutine is running on the owning client and resets
    // both flags immediately, instead of letting them expire on their own.
    [TargetRpc]
    void TargetClearDebuffs(NetworkConnection target)
    {
        if (forceAutoBombRoutine != null)
        {
            StopCoroutine(forceAutoBombRoutine);
            forceAutoBombRoutine = null;
        }
        if (reverseControlsRoutine != null)
        {
            StopCoroutine(reverseControlsRoutine);
            reverseControlsRoutine = null;
        }
        forcedAutoBomb = false;
        controlsReversed = false;
    }
    // Mount item pickup - grants one absorbed hit (see HealthDamage) instead
    // of a straight stat buff. Replaces whatever mount is already active, if
    // any (matches picking up a second mount before the first was knocked
    // off - no need to be dismounted first).
    [Server]
    public void Mount(MountData mount)
    {
        currentMount = mount;
        TargetApplyMount(connectionToClient, mount.MoveSpeedModifier, mountStaggerDuration, true);
        RpcShowMountVisual(mount.VisualColor);
        RestartBombImmune(mountStaggerDuration);
    }
    [Server]
    void Dismount()
    {
        currentMount = null;
        TargetApplyMount(connectionToClient, 0f, mountStaggerDuration, false);
        RpcHideMountVisual();
        RestartBombImmune(mountStaggerDuration);
    }
    Coroutine bombImmuneRoutine;
    // Swapping mounts calls this again before the previous window ends -
    // without stopping that older one first, it would still turn
    // isBombImmune back off early once its own (now stale) duration
    // elapses, cutting the new window short.
    [Server]
    void RestartBombImmune(float duration)
    {
        if (bombImmuneRoutine != null) StopCoroutine(bombImmuneRoutine);
        bombImmuneRoutine = StartCoroutine(BombImmuneRoutine(duration));
    }
    // Placeholder only - a plain tinted shape parented under the character,
    // sent to every client (not just the owner) since riding a mount is
    // something everyone should see. TODO(mount visuals): once a real model
    // exists per mount, along with a sit animation, replace this instantiate
    // with swapping the character's own animator state instead.
    [ClientRpc]
    void RpcShowMountVisual(Color color)
    {
        if (mountVisualPrefab == null)
        {
            Debug.LogError($"{name}: mountVisualPrefab isn't assigned - mount pickups won't show anything.", this);
            return;
        }
        if (mountVisualInstance == null)
        {
            mountVisualInstance = Instantiate(mountVisualPrefab, transform);
            // The character's own root rises by mountRideHeight while mounted
            // (see HopRoutine) - since this is parented under that same
            // transform, it would rise right along with it and end up
            // hovering in the air instead of sitting near the ground. Shift
            // it back down by the same amount so it stays put underfoot.
            mountVisualInstance.transform.localPosition -= new Vector3(0, mountRideHeight, 0);
            // GridHighlight is a direct child of this same root (every
            // character prefab has one) - same rising-with-the-parent issue,
            // so it needs the same counter-offset. Only ever done once per
            // mount (guarded by mountVisualInstance == null, same as above) -
            // swapping mounts while already riding one calls this again, and
            // applying the offset a second time would push it down further
            // each time instead of leaving it where it already correctly is.
            OffsetGridHighlight(-mountRideHeight);
        }
        Renderer rend = mountVisualInstance.GetComponentInChildren<Renderer>();
        if (rend != null)
        {
            MaterialPropertyBlock block = new MaterialPropertyBlock();
            block.SetColor("_BaseColor", color);
            rend.SetPropertyBlock(block);
        }
    }
    [ClientRpc]
    void RpcHideMountVisual()
    {
        if (mountVisualInstance != null) Destroy(mountVisualInstance);
        OffsetGridHighlight(mountRideHeight);
    }
    Transform gridHighlight;
    void OffsetGridHighlight(float deltaY)
    {
        if (gridHighlight == null) gridHighlight = transform.Find("GridHighlight");
        if (gridHighlight == null) return;
        gridHighlight.localPosition += new Vector3(0, deltaY, 0);
    }
    // Movement is client-authoritative, so both the speed modifier and the
    // stagger's movement lock have to be applied here on the owning client,
    // not just server-side - isBombImmune is the only part of this that can
    // stay server-only, since ExplosionSegment's damage check already runs
    // there.
    [TargetRpc]
    void TargetApplyMount(NetworkConnection target, float speedModifier, float staggerDuration, bool mounting)
    {
        mountSpeedModifier = speedModifier;
        bool wasMountedAlready = isMountedLocally;
        isMountedLocally = mounting;

        // HopRoutine's end height is relative to wherever it starts, not an
        // absolute ground height - picking up a second mount while already
        // riding one (swap, no need to dismount first - see Mount()) would
        // otherwise stack another +mountRideHeight on top of the current
        // (already elevated) position each time, climbing higher forever.
        // Already at ride height in that case, so just skip the hop - the
        // color/stat swap (RpcShowMountVisual/mountSpeedModifier) is enough.
        if (mounting && wasMountedAlready) return;

        // Mounting ends the hop mountRideHeight HIGHER than it started
        // (character now sits on top of the mount); dismounting is the
        // reverse, back down to normal ground height.
        StartCoroutine(MountStaggerLockRoutine(staggerDuration, mounting ? mountRideHeight : -mountRideHeight));
    }
    // Same lock skills use (Korone's Jump, Watame's Bomb Push) - covers both
    // just-mounted and just-knocked-off, since both are "briefly can't act".
    // Plays a small hop for the same duration as a placeholder for a real
    // "climb on"/"fall off" animation - timed with RpcShowMountVisual/
    // RpcHideMountVisual, which fire in the same frame Mount()/Dismount() do.
    IEnumerator MountStaggerLockRoutine(float duration, float endYOffset)
    {
        SetSkillLock(true);
        yield return StartCoroutine(HopRoutine(duration, endYOffset));
        SetSkillLock(false);
    }
    // Runs on the owning client - direct transform writes, same as Korone's
    // Jump, replicate out to every other client via this character's own
    // client-authoritative NetworkTransform. endYOffset is where the hop
    // settles relative to where it started - 0 for Korone/Watame's skills
    // (return to the same spot), +/-mountRideHeight for mounting/dismounting
    // (settle higher/lower - the mount visual, parented under this same
    // transform, rises and falls right along with it).
    IEnumerator HopRoutine(float duration, float endYOffset = 0f)
    {
        Vector3 basePos = transform.position;
        Vector3 targetPos = basePos + new Vector3(0, endYOffset, 0);
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            Vector3 flat = Vector3.Lerp(basePos, targetPos, t);
            float arc = mountHopHeight * Mathf.Sin(t * Mathf.PI); // 0 at both ends, peaks at the midpoint
            transform.position = new Vector3(flat.x, flat.y + arc, flat.z);
            yield return null;
        }
        transform.position = targetPos;
    }
    IEnumerator BombImmuneRoutine(float duration)
    {
        isBombImmune = true;
        yield return new WaitForSeconds(duration);
        isBombImmune = false;
        bombImmuneRoutine = null;
    }
    // Called by skills that move this character's own transform (Korone's
    // Jump) or otherwise need it to hold still for a moment (Watame's Bomb
    // Push) - without this, FixedUpdate's normal WASD handling would fight
    // whatever the skill is doing to the Rigidbody/transform.
    public void SetSkillLock(bool locked)
    {
        isSkillLocked = locked;
        // FixedUpdate skips its usual "velocity.y = 0 every tick" reset
        // while locked, so gravity would otherwise accumulate downward
        // velocity unopposed for the whole locked window - once the floor's
        // collider catches it, that fights (and was winning against) any
        // manual transform lift a hop tries to do, like mounting settling
        // higher than ground level. Unlocking only turns gravity back on if
        // NOT still mounted - the elevated ride height has nothing solid
        // under it, so gravity has to stay off for the whole ride, not just
        // this brief hop, or it drifts back down afterward.
        rd.useGravity = locked ? false : !isMountedLocally;
        if (locked) rd.velocity = Vector3.zero;
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
}

}