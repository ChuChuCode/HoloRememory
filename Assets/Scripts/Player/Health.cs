using HR.UI;
using Mirror;
using UnityEngine;
namespace HR.Object{

public abstract class Health : NetworkBehaviour
{
    #region Patameter
    [Tooltip("Get exp when be killed.")]
    public int exp;
    [Tooltip("Get coin when be killed.")]
    public int coin;
    [SyncVar] public int maxHealth ;
    [SyncVar(hook = nameof(Set_Health))] public int currentHealth = 1;
    [SyncVar] public bool isDead = false;
    public Transform Target;
    [SerializeField] protected Bar healthBar;
    #endregion
    protected virtual void Awake()
    {
        // Check exp setting
        if (exp == 0)
        {
            Debug.LogWarning($"Please set exp parameter for {GetType().Name}:{gameObject.name}.");
        }
        // Check coin setting
        if (coin == 0)
        {
            Debug.LogWarning($"Please set coin parameter for {GetType().Name}:{gameObject.name}.");
        }
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
        Selectable.instance.updateInfo(this);
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

    #region Other Method
    /// <summary>
    /// Get Distance from gameobject to Enemy edge(center distance - Enemy radius).
    /// </summary>
    /// <param name="enemy">Target to attack.</param>
    /// <returns>Distance from gameobject to Enemy edge.</returns>
    protected float Get_Target_Radius(Transform enemy)
    {
        float radius = 0;
        SphereCollider sphereCollider = enemy.GetComponentInChildren<SphereCollider>();
        if ( sphereCollider != null)
        {
            radius = sphereCollider.radius;
        }
        CapsuleCollider capsuleCollider = enemy.GetComponentInChildren<CapsuleCollider>();
        if ( capsuleCollider != null)
        {
            radius = capsuleCollider.radius;
        }
        
        return Vector3.Distance(enemy.transform.position,transform.position) - radius;
    }
    /// <summary>
    /// Check the nearest object in hitColliders array
    /// </summary>
    /// <param name="hitColliders">Array of colliders.</param>
    /// <returns>The nearest object from GameObject.</returns>
    protected Transform Search_Nearest(Collider[] hitColliders)
    {
        if (hitColliders.Length == 0) return null;
        Transform target = hitColliders[0].transform.root;
        float distance = Vector3.Distance(transform.position, hitColliders[0].transform.position);

        for (int i = 1; i < hitColliders.Length; i++)
        {
            float temp_distance = Vector3.Distance(transform.position, hitColliders[i].transform.position);
            if ( temp_distance < distance)
            {
                target = hitColliders[i].transform.root;
                distance = temp_distance;
            }
        }
        return target;
    }
    #endregion
}

}