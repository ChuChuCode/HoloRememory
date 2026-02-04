using HR.UI;
using Mirror;
using UnityEngine;
namespace HR.Object{

public abstract class Health : NetworkBehaviour
{
    #region Patameter
    [SyncVar] public int maxHealth ;
    [SyncVar(hook = nameof(Set_Health))] public int currentHealth = 1;
    [SyncVar] public bool isDead = false;
    public Transform Target;
    [SerializeField] protected Bar healthBar;
    #endregion
    protected virtual void Awake()
    {
        // Check setting
        if (maxHealth == 0)
        {
            Debug.LogWarning($"Please set maxHealth parameter for {GetType().Name}:{gameObject.name}.");
        }
    }
    #region Method for Health
    /// <summary>
    /// Set currentHealth to maxHealth.
    /// </summary>
    public virtual void InitialHealth()
    {
        if (isServer) currentHealth = maxHealth;
        else if (isClient) CmdSetlHealth(maxHealth);
    }
    /// <summary>
    /// Decrease health to currentHealth.
    /// </summary>
    /// <param name="damage">Decreased health.</param>
    /// <returns>Is gameobject dead or not.</returns>
    public virtual bool HealthDamage(int damage)
    {
        int beforeHealth = currentHealth;
        if (isServer) currentHealth -= damage;
        else if (isClient) CmdSetlHealth(currentHealth - damage);
        return beforeHealth > 0 && currentHealth <= 0 ;
    }
    /// <summary>
    /// Add health to currentHealth.
    /// </summary>
    /// <param name="health">Added health.</param>
    public virtual void HealthHeal(int health)
    {
        if (isServer) currentHealth += health;
        else if (isClient) CmdSetlHealth(currentHealth + health);
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
    }
    /// <summary>
    /// Change currentHealth from Client to Server.(Only set thing on Authority Object)
    /// </summary>
    /// <param name="NewHealth">Changed currentHealth.</param>
    [Command]
    public virtual void CmdSetlHealth(int NewHealth)
    {
        currentHealth = NewHealth;
    }
    /// <summary>
    /// Hook for currentHealth
    /// </summary>
    public virtual void Set_Health(int OldValue,int NewValue)
    {
        // print(gameObject.name + " : " + OldValue + " -> " + NewValue);
        // UI Update
        if (healthBar == null) return;
        healthBar.SetMaxValue(maxHealth);
        healthBar.SetValue(NewValue);
    }
    /// <summary>
    // Do things when Dead.
    // </summary>
    protected abstract void Death();
    #endregion
}

}