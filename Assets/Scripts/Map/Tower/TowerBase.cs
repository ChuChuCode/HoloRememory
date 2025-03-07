using UnityEngine;
using HR.UI;
using Mirror;

namespace HR.Object.Map{
public class TowerBase : Health
{
    [Header("Gizmos")]
    [HideInInspector]public bool ShowGizmos;
    [SerializeField] protected CapsuleCollider Next_Tower_Collider;
    [SerializeField] [Range(0.0f, 10.0f)] protected float attack_radius = 5f;
    protected virtual void Start()
    {
        if (!isServer) return;
        InitialHealth();
    }
    [Server]
    public override void CmdSetlHealth(int NewHealth)
    {
        currentHealth = NewHealth;
    }
    public override bool GetDamage(int damage)
    {
        bool isdead = base.GetDamage(damage);
        // Update UI
        Selectable.instance.updateInfo(this);
        return isdead;
    }

    public override void Death()
    {
        GetComponent<Collider>().enabled = false;
        if (Next_Tower_Collider != null)
        {
            Next_Tower_Collider.enabled = true;
        }
    }
    void OnDrawGizmosSelected()
    {
        if (!ShowGizmos) return;
        // Draw a yellow sphere at the transform's position
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, attack_radius);
    }
}

}