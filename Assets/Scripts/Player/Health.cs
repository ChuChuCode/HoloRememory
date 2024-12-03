using Mirror;
using UnityEngine;
namespace HR.Object{

[RequireComponent (typeof (Outline))]
public class Health : NetworkBehaviour
{
    [SyncVar] public int maxHealth ;
    [SyncVar] public int currentHealth;
    [SyncVar] public bool isDead = false;
    public virtual void InitialHealth()
    {
        currentHealth = maxHealth;
    }
    public virtual void GetDamage(int damage)
    {
        currentHealth -= damage;
    }
    public virtual void Death(){}
    
}

}