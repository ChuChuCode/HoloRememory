using Mirror;
public class Health : NetworkBehaviour
{
    [SyncVar] public int maxHealth ;
    [SyncVar] public int currentHealth;
    protected bool isDead;
    public virtual void InitialHealth(){}
    public virtual void GetDamage(int damage){}
    public virtual void Death(){}
}
