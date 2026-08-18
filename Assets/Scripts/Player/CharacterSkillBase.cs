using System.Collections;
using UnityEngine;
using Mirror;
using HR.UI;

namespace HR.Object.Player{
// Base for every character's active skill. Gated purely by Skill Energy -
// no cooldown timer. Energy comes from external actions (destroying a
// block, picking up an item, etc.) calling AddSkillEnergy(); once it's
// full, the owning player's Skill input triggers Activate() and it resets
// to 0. Per-character skills subclass this and override Activate().
//
// TODO(SP rework, not started): proposed redesign discussed with user -
//   1. Rename Skill Energy -> SP, and have it passively regenerate over
//      time (e.g. +X per second) instead of/alongside the current
//      action-triggered AddSkillEnergy() grants.
//   2. Let each character's skill cost a different amount of SP to fire,
//      rather than every skill requiring the bar to be 100% full - needs
//      a `Cost` field on SkillData (separate from MaxEnergy), and
//      IsSkillReady/Activate() below need to check/spend `Cost` instead
//      of MaxEnergy/resetting to 0.
//   3. SkillData needs a `Description` field (+ Cost from #2) so the HUD
//      can finally show skill description/icon - this was deferred
//      earlier for lack of that data (see GameHUDManager/LocalPlayerHUD).
// Scope is small - mostly SkillData.cs + this file for the mechanic, plus
// SkillEnergyUI.cs/prefab wiring to actually display description + cost.
public abstract class CharacterSkillBase : NetworkBehaviour
{
    [SerializeField] protected SkillData data;

    // Unlike bombAmount/bombPower (which the owning client mutates itself
    // via client-side prediction before the Command even runs), every write
    // to this happens server-side only (AddSkillEnergy/CmdActivate) - with
    // no SyncVar, a remote (non-host) client's own copy of this field would
    // just stay 0 forever and never reflect its own energy.
    [SyncVar(hook = nameof(OnSkillEnergyChanged))]
    protected float skillEnergy = 0f;
    public float SkillEnergy => skillEnergy;
    public float MaxSkillEnergy => data != null ? data.MaxEnergy : 100f;
    public bool IsSkillReady => skillEnergy >= MaxSkillEnergy;

    protected CharacterBase owner;
    protected virtual void Awake()
    {
        owner = GetComponent<CharacterBase>();
    }
    void Start()
    {
        // SyncVar hooks only fire on change - push the resting 0/Max state
        // once up front so the HUD bar isn't just blank until the first
        // AddSkillEnergy call.
        if (!isLocalPlayer) return;
        SkillEnergyUI.instance?.UpdateSkillEnergy(skillEnergy, MaxSkillEnergy);
    }
    void OnSkillEnergyChanged(float oldValue, float newValue)
    {
        if (!isLocalPlayer) return;
        SkillEnergyUI.instance?.UpdateSkillEnergy(newValue, MaxSkillEnergy);
    }

    // Called by whatever grants energy (destroying a block, an item, etc.) -
    // server-only, clamped to MaxSkillEnergy.
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
        StartCoroutine(ActivateAfterWindUp());
    }
    // WindUpTime is 0 for every P0 skill, so this resolves immediately today.
    // Reserved so a future wind-up animation just has to set WindUpTime and
    // (TODO) trigger the animation here, without touching CmdActivate itself.
    IEnumerator ActivateAfterWindUp()
    {
        float windUpTime = data != null ? data.WindUpTime : 0f;
        if (windUpTime > 0f) yield return new WaitForSeconds(windUpTime);

        // Energy is only spent if the skill actually happened - e.g. Korone's
        // Jump can be aimed at a blocked cell, and a wasted attempt like that
        // shouldn't burn the whole energy bar for nothing.
        if (Activate()) skillEnergy = 0f;
    }
    // The actual skill effect - implemented per character. Runs server-side
    // (called from CmdActivate). Return true if it actually happened (energy
    // gets spent), false to no-op the attempt for free.
    protected abstract bool Activate();

}

}
