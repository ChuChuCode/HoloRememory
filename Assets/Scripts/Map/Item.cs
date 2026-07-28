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

        Apply(character);
        NetworkServer.Destroy(gameObject);
    }

    protected abstract void Apply(CharacterBase character);
}
}
