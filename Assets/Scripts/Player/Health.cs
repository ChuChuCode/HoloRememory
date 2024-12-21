using Mirror;
using UnityEngine;
namespace HR.Object{

[RequireComponent (typeof (Outline))]
public class Health : NetworkBehaviour
{
    /// <summary>Get exp when be killed.</summary>
    public int exp;
    public int coin;
    [SyncVar] public int maxHealth ;
    [SyncVar] public int currentHealth;
    [SyncVar] public bool isDead = false;
    /// <summary>Set Health to maxHealth.</summary>
    public virtual void InitialHealth()
    {
        currentHealth = maxHealth;
    }
    /// <summary>
    /// Let gameobject get damage. Return is dead or not.
    /// </summary>
    public virtual bool GetDamage(int damage)
    {
        int beforeHealth = currentHealth;
        currentHealth -= damage;
        return beforeHealth > 0 && currentHealth <= 0 ;
    }
    /// <summary>Do things when Dead.</summary>
    public virtual void Death(){}
    
}

}