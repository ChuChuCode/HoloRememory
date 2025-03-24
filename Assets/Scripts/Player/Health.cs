using HR.UI;
using Mirror;
using UnityEngine;
namespace HR.Object{

public class Health : NetworkBehaviour
{
    /// <summary>Get exp when be killed.</summary>
    public int exp;
    public int coin;
    [SyncVar] public int maxHealth ;
    [SyncVar(hook = nameof(Set_Health))] public int currentHealth;
    [SyncVar] public bool isDead = false;
    public Transform Target;
    void Start()
    {
           
    }
        /// <summary>Set Health to maxHealth.</summary>
        public virtual void InitialHealth()
    {
        if (isServer) currentHealth = maxHealth;
        else if (isClient) CmdSetlHealth(maxHealth);
    }
    /// <summary>
    /// Let gameobject get damage. Return is dead or not.
    /// </summary>
    public virtual bool GetDamage(int damage)
    {
        int beforeHealth = currentHealth;
        if (isServer) currentHealth -= damage;
        else if (isClient) CmdSetlHealth(currentHealth - damage);
        return beforeHealth > 0 && currentHealth <= 0 ;
    }
    public virtual void Heal(int health)
    {
        if (isServer) currentHealth += health;
        else if (isClient) CmdSetlHealth(currentHealth + health);
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
        Selectable.instance.updateInfo(this);
        // print(gameObject.name + " : " + OldValue + " -> " + NewValue);
    }
    /// <summary>
    /// Get Distance from gameobject to Enemy edge(center distance - Enemy radius)
    /// </summary>
    /// <param name="enemy"></param>
    /// <returns></returns>
    /// 
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
}

}