using UnityEngine;
using HR.Object.Player;

namespace HR.Map{
// Negative-effect pickup - on contact, the server picks one of two equally
// likely debuffs and applies it to whoever grabbed it (see
// CharacterBase.ApplyRandomDebuff): forced auto-bombing, or reversed
// movement controls, both for debuffDuration seconds.
public class ChaosItem : Item
{
    protected override void Apply(CharacterBase character)
    {
        character.ApplyRandomDebuff();
    }
}
}
