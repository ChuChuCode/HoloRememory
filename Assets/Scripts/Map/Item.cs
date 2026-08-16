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

        Debug.Log($"[ItemDebug] {name} picked up by {character.name} at {transform.position}, t={Time.time}");
        Apply(character);
        NetworkServer.Destroy(gameObject);
    }

    // TEMP diagnostic - fires for ANY reason this item's GameObject goes
    // away (picked up, caught in a blast, or anything else), so the cause
    // of the "items vanish the instant they land" report can be pinned down
    // from the Console instead of guessed at. Remove once that's found.
    [ServerCallback]
    void OnDestroy()
    {
        Debug.Log($"[ItemDebug] {name} destroyed at {transform.position}, t={Time.time}");
    }

    protected abstract void Apply(CharacterBase character);
}
}
