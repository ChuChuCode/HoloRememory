using Mirror;
namespace HR.Object{
public class Health : NetworkBehaviour
{
    [SyncVar] public int maxHealth ;
    [SyncVar] public int currentHealth;
    [SyncVar] public bool isDead;
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