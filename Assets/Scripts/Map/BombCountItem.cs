using UnityEngine;
using HR.Object.Player;

namespace HR.Map{
public class BombCountItem : Item
{
    [SerializeField] int amount = 1;

    protected override void Apply(CharacterBase character)
    {
        character.AddBombCount(amount);
    }
}
}
