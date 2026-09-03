using System.Collections;
using UnityEngine;
using Mirror;
using HR.UI;

namespace HR.Object.Player{
// Base for every character's active skill. Gated by SP, which passively
// regenerates continuously (every server frame, see Update()) instead of
// being granted by outside actions - once there's enough for this skill's
// Cost, the owning player's Skill input triggers Activate() and Cost (not
// the whole bar) is spent. Per-character skills subclass this and override
// Activate().
//
// TODO(HUD icon): SkillData still has no icon field, only Description
// (shown via SkillEnergyUI.SetSkillInfo) - deferred until that art exists
// (see GameHUDManager/LocalPlayerHUD).
//
// TODO(SP items, not started): proposed items discussed with user -
//   1. Instant SP refill - trivial, AddSkillEnergy(amount) is already
//      public; an item pickup just calls
//      character.SkillComponent.AddSkillEnergy(N) server-side.
//   2. Temporary faster regen - needs a runtime regenRateMultiplier field
//      here (Update() would multiply the per-second rate by it), plus a
//      timer to revert it - same "temporary buff + timer" shape as
//      CharacterBase's mountSpeedModifier/isBombImmune.
public abstract class CharacterSkillBase : NetworkBehaviour
{
    [SerializeField] protected SkillData data;

    // Unlike bombAmount/bombPower (which the owning client mutates itself
    // via client-side prediction before the Command even runs), every write
    // to this happens server-side only (Update/CmdActivate) - with no
    // SyncVar, a remote (non-host) client's own copy of this field would
    // just stay 0 forever and never reflect its own SP.
    [SyncVar(hook = nameof(OnSkillEnergyChanged))]
    protected float skillEnergy = 0f;
    public float SkillEnergy => skillEnergy;
    public float MaxSkillEnergy => data != null ? data.MaxEnergy : 10f;
    public float Cost => data != null ? data.Cost : MaxSkillEnergy;
    public bool IsSkillReady => skillEnergy >= Cost;

    protected CharacterBase owner;
    protected virtual void Awake()
    {
        owner = GetComponent<CharacterBase>();
    }
    void Start()
    {
        // SyncVar hooks only fire on change - push the resting 0/Max state
        // once up front so the HUD bar isn't just blank until the first
        // regen tick.
        if (!isLocalPlayer) return;
        SkillEnergyUI.instance?.InitSkillEnergy(skillEnergy, MaxSkillEnergy);
        SkillEnergyUI.instance?.SetSkillInfo(
            data != null ? data.SkillName : "",
            data != null ? data.Description : "");
    }
    // Passive SP regen - continuous, not stepped: adds a per-second rate
    // (RegenAmount/RegenInterval) scaled by deltaTime every frame, instead
    // of a lump sum every few seconds, so the HUD bar fills smoothly rather
    // than jumping in visible steps. Runs regardless of isDead/respawn (SP
    // already persists across respawn, same as before this rework), and
    // just naturally stops growing once AddSkillEnergy's own clamp hits
    // MaxSkillEnergy - no need to gate that here.
    [ServerCallback]
    void Update()
    {
        if (data == null || data.RegenInterval <= 0f) return;
        AddSkillEnergy((data.RegenAmount / data.RegenInterval) * Time.deltaTime);
    }
    void OnSkillEnergyChanged(float oldValue, float newValue)
    {
        if (!isLocalPlayer) return;
        SkillEnergyUI.instance?.UpdateSkillEnergy(newValue);
    }

    // Server-only, clamped to MaxSkillEnergy. Called every frame by this
    // component's own Update() now for passive regen - kept public so an
    // item/future mechanic can still grant a burst of SP on top of that.
    [Server]
    public void AddSkillEnergy(float amount)
    {
        skillEnergy = Mathf.Min(skillEnergy + amount, MaxSkillEnergy);
    }

    // Called from CharacterBase's Skill input binding (owning client only).
    public void TryActivate()
    {
        if (!isOwned) return;
        CmdActivate();
    }
    [Command]
    void CmdActivate()
    {
        if (!IsSkillReady) return;
        // Match-start countdown / match-over lock (Network_Manager sets
        // this on owner, not on this component) - server-authoritative
        // check, doesn't rely on the client having honored it.
        if (owner != null && owner.isInputLocked) return;
        StartCoroutine(ActivateAfterWindUp());
    }
    // WindUpTime is 0 for every P0 skill, so this resolves immediately today.
    // Reserved so a future wind-up animation just has to set WindUpTime and
    // (TODO) trigger the animation here, without touching CmdActivate itself.
    IEnumerator ActivateAfterWindUp()
    {
        float windUpTime = data != null ? data.WindUpTime : 0f;
        if (windUpTime > 0f) yield return new WaitForSeconds(windUpTime);

        // SP is only spent if the skill actually happened - e.g. Korone's
        // Jump can be aimed at a blocked cell, and a wasted attempt like that
        // shouldn't cost anything. Spends just Cost, not the whole bar -
        // any leftover keeps counting toward the next activation.
        if (Activate()) skillEnergy = Mathf.Max(0f, skillEnergy - Cost);
    }
    // The actual skill effect - implemented per character. Runs server-side
    // (called from CmdActivate). Return true if it actually happened (energy
    // gets spent), false to no-op the attempt for free.
    protected abstract bool Activate();

}

}
