using Mirror;
using UnityEngine;
using HR.Object.Player;

namespace HR.Map{
public abstract class Item : NetworkBehaviour
{
    [ServerCallback]
    void OnTriggerEnter(Collider other)
    {
        CharacterBase character = other.GetComponentInParent<CharacterBase>();
        if (character == null) return;
        if (!character.CanPickupItems) return; // e.g. riding a UFO mount

        // Solo Mode tutorial-trigger flag (see LevelEventTrigger.PickedUpItem) -
        // already server-side here ([ServerCallback]), no Command needed.
        character.TutorialPickedUpItem = true;
        Apply(character);
        NetworkServer.Destroy(gameObject);
    }

    protected abstract void Apply(CharacterBase character);
}
}
