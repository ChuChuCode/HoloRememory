using UnityEngine;
using HR.Object.Player;

namespace HR.Map{
public class MountItem : Item
{
    [SerializeField] MountData mount;

    protected override void Apply(CharacterBase character)
    {
        if (mount == null)
        {
            Debug.LogError($"{name}: MountItem has no MountData assigned.", this);
            return;
        }
        character.Mount(mount);
    }
}
}
