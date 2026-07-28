using System.Collections.Generic;
using UnityEngine;
using Mirror;
using HR.Object.Player;
using HR.Map;

namespace HR.Object.Skill{
public class ExplosionSegment : NetworkBehaviour
{
    [SerializeField] float lifeTime = 0.5f; // how long the visual effect plays before the object is removed
    [SerializeField] float hitWindow = 0.15f; // how long it can actually deal damage - much shorter, so rushing in for a dropped item isn't a death sentence
    [SerializeField] int damage = 1;

    HashSet<CharacterBase> hitTargets = new();
    Collider col;

    public override void OnStartServer()
    {
        col = GetComponent<Collider>();
        Invoke(nameof(StopHitting), hitWindow);
        Invoke(nameof(DestroySelf), lifeTime);
    }

    [ServerCallback]
    void OnTriggerEnter(Collider other)
    {
        CharacterBase character = other.GetComponentInParent<CharacterBase>();
        if (character != null && hitTargets.Add(character))
        {
            character.HealthDamage(damage);
        }

        // Unlike walls/destructibles, items never block the grid (no
        // GridCell), so the blast already passes straight through them -
        // this just also destroys the item caught in it, on top of that.
        Item item = other.GetComponentInParent<Item>();
        if (item != null)
        {
            NetworkServer.Destroy(item.gameObject);
        }
    }

    [Server]
    void StopHitting()
    {
        col.enabled = false;
    }

    [Server]
    void DestroySelf()
    {
        NetworkServer.Destroy(gameObject);
    }
}
}
