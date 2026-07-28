using UnityEngine;
using HR.Object.Player;

namespace HR.Map{
public class SpeedItem : Item
{
    [SerializeField] float amount = 0.5f;

    protected override void Apply(CharacterBase character)
    {
        character.AddMoveSpeed(amount);
    }
}
}
