using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using HR.UI;
using HR.Object.Player;
using System;


namespace HR.Object{
[RequireComponent(typeof(NavMeshAgent))]
public abstract class MinionBase : Health
{
    #region Parameter
    [Header("State Strings")]
    const string WALK = "Walk";
    const string CHASE = "Chase";
    const string ATTACK = "Attack";
    const string DEAD = "Dead";
    [Header("Current State")]
    [SerializeField] protected string current_State = "";
    [SerializeField] protected Dictionary<string, Action> CharacterState;
    [Header("Agent")]
    public NavMeshAgent agent;
    [SerializeField] protected Animator animator;
    [Tooltip("Follow Player or go to a fixed destination")]
    public Transform MainDestination;
    [SerializeField] protected LayerMask Layer_Enemy;
    [Header("Attack")]
    [SerializeField] protected float Search_radius = 8f;
    [SerializeField] protected float attack_radius = 5f;
    protected float timer = 1f;
    [SerializeField] protected float deadTime = 1f;
    #endregion
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
    }
    /// <summary>
    /// Add initial current_State then call base.Start().
    /// </summary>
    protected virtual void Start()
    {
        // Initial State Dictionary
        CharacterState = new Dictionary<string, Action>{};
        // Add State to Dictionary
        Add_State();
        InitialHealth();
        if (current_State == "")
        {
            Debug.LogError("You should initial your current_State parameter.");
        }
    }
    /// <summary>Add State to CharacterState Dictionary.</summary>
    protected abstract void Add_State();
    protected virtual void Update()
    {
        if (CharacterState.ContainsKey(current_State))
        {
            CharacterState[current_State]?.Invoke(); 
        }
    }
    protected override void Death()
    {
        // Set Health to 0
        currentHealth = 0;
        Destroy(gameObject);
    }
    public void Update_Enemy_Layer(int layer)
    {
        Layer_Enemy &= ~(1 << layer);
        if (LayerMask.LayerToName(layer) == "Team1")
        {
            Layer_Enemy &= ~(1 << LayerMask.NameToLayer("Team1Building"));
        }
        else
        {
            Layer_Enemy &= ~(1 << LayerMask.NameToLayer("Team2Building"));
        }    
    }
    protected void Exp_Detect_Surround()
    {
        List<CharacterSkillBase> tempCharacterSkill = new List<CharacterSkillBase>();
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 10f, Layer_Enemy);
        // Check is CharacterSkillBase
        foreach (Collider collider in hitColliders)
        {
            CharacterSkillBase tempSkill = collider.transform.root.GetComponent<CharacterSkillBase>();
            if (tempSkill != null)
            {
                tempCharacterSkill.Add(tempSkill);
            }
        }
        foreach (CharacterSkillBase tempSkill in tempCharacterSkill)
        {
            // Check character around
            int exp_new = (tempCharacterSkill.Count == 1) ? exp : (int)(exp*0.7);
            tempSkill.AddExp(exp_new);
        }
    }
    void OnDrawGizmosSelected()
    {
        // Draw a yellow sphere at the transform's position
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, Search_radius);
    }
}

}