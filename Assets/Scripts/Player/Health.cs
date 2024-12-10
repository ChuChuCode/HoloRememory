using Mirror;
using UnityEditor.EditorTools;
using UnityEngine;
namespace HR.Object{

[RequireComponent (typeof (Outline))]
public class Health : NetworkBehaviour
{
    // Get exp when be killed
    public int exp;
    [SyncVar] public int maxHealth ;
    [SyncVar] public int currentHealth;
    [SyncVar] public bool isDead = false;
    public virtual void InitialHealth()
    {
        currentHealth = maxHealth;
    }
    // Get Damage and return exp
    public virtual int GetDamage(int damage)
    {
        currentHealth -= damage;
        
        if (currentHealth <= 0)
        {
            return exp;
        }
        return 0;
    }
    public virtual void Death(){}
    
}

}