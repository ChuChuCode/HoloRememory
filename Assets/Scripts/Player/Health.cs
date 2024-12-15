using Mirror;
using UnityEngine;
namespace HR.Object{

[RequireComponent (typeof (Outline))]
public class Health : NetworkBehaviour
{
    /// <summary>Get exp when be killed.</summary>
    public int exp;
    [SyncVar] public int maxHealth ;
    [SyncVar] public int currentHealth;
    [SyncVar] public bool isDead = false;
    /// <summary>Set Health to maxHealth.</summary>
    public virtual void InitialHealth()
    {
        currentHealth = maxHealth;
    }
    /// <summary>Let gameobject get damage.</summary>
    public virtual void GetDamage(int damage)
    {
        currentHealth -= damage;
    }
    /// <summary>Do things when Dead.</summary>
    public virtual void Death(){}
    
}

}