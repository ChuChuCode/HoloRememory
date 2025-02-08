using UnityEngine;
using HR.Object.Skill;
using HR.UI;

namespace HR.Object.Map{
public class TowerBase : Health
{
    [SerializeField] CapsuleCollider Next_Tower_Collider;
    void Start()
    {
        InitialHealth();
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
}

}