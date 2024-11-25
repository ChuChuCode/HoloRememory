using Mirror;
public class Health : NetworkBehaviour
{
    [SyncVar] public int maxHealth ;
    [SyncVar] public int currentHealth;
    protected bool isDead;
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
