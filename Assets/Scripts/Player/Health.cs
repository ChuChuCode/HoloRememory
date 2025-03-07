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
    [SyncVar(hook = nameof(Set_Health))] public int currentHealth;
    [SyncVar] public bool isDead = false;
    /// <summary>Set Health to maxHealth.</summary>
    public virtual void InitialHealth()
    {
        CmdSetlHealth(maxHealth);
    }
    /// <summary>
    /// Let gameobject get damage. Return is dead or not.
    /// </summary>
    public virtual bool GetDamage(int damage)
    {
        int beforeHealth = currentHealth;
        // for local
        currentHealth -= damage;
        CmdSetlHealth(currentHealth - damage);
        return beforeHealth > 0 && currentHealth <= 0 ;
    }
    public virtual void Heal(int health)
    {
        // for local
        currentHealth += health;
        CmdSetlHealth(currentHealth + health);
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
    }
    /// <summary>Do things when Dead.</summary>
    public virtual void Death(){}
    [Command]
    public virtual void CmdSetlHealth(int NewHealth)
    {
        currentHealth = NewHealth;
    }
    public virtual void Set_Health(int OldValue,int NewValue)
    {
        print(gameObject.name + " : " + OldValue + " -> " + NewValue);
    }
}

}