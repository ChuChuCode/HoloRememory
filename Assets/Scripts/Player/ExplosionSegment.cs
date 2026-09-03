using System.Collections.Generic;
using UnityEngine;
using Mirror;
using HR.Object.Player;
using HR.Object.Minions;
using HR.Map;
using HR.Network;

namespace HR.Object.Skill{
public class ExplosionSegment : NetworkBehaviour
{
    [SerializeField] float lifeTime = 0.5f; // how long the visual effect plays before the object is removed
    [SerializeField] float hitWindow = 0.15f; // how long it can actually deal damage - much shorter, so rushing in for a dropped item isn't a death sentence
    [SerializeField] int damage = 1;

    HashSet<CharacterBase> hitTargets = new();
    HashSet<Minion> hitMinions = new();
    Collider col;
    Vector2Int cell;
    bool isHitting;

    public override void OnStartServer()
    {
        col = GetComponent<Collider>();
        cell = GridManager.Instance.WorldToGrid(transform.position);
        isHitting = true;
        Invoke(nameof(StopHitting), hitWindow);
        Invoke(nameof(DestroySelf), lifeTime);
    }

    // Grid-based, not the physics collider - a character only takes damage
    // when their own logical cell (the same center-of-mass check used
    // everywhere else: Jump, Bomb Push, movement) actually matches this
    // blast tile, instead of dying just from a collider edge brushing it.
    [ServerCallback]
    void Update()
    {
        if (!isHitting) return;
        Network_Manager manager = Network_Manager.singleton as Network_Manager;
        if (manager == null) return;

        foreach (CharacterBase character in manager.Player_List)
        {
            if (character.isDead || hitTargets.Contains(character)) continue;
            if (GridManager.Instance.WorldToGrid(character.transform.position) != cell) continue;
            hitTargets.Add(character);
            character.HealthDamage(damage);
        }

        foreach (Minion minion in manager.Minion_List)
        {
            if (minion.isDead || hitMinions.Contains(minion)) continue;
            if (GridManager.Instance.WorldToGrid(minion.transform.position) != cell) continue;
            hitMinions.Add(minion);
            minion.HealthDamage(damage);
        }
    }

    [ServerCallback]
    void OnTriggerEnter(Collider other)
    {
        // Unlike walls/destructibles, items never block the grid (no
        // GridCell), so the blast already passes straight through them -
        // this just also destroys the item caught in it, on top of that.
        // Still physics-based (unlike player damage above) since a loose
        // item's exact position isn't grid-snapped the way a character's
        // hit detection needs to be.
        Item item = other.GetComponentInParent<Item>();
        if (item != null)
        {
            Debug.Log($"[ItemDebug] {item.name} caught in blast at {item.transform.position}, t={Time.time}"); // TEMP diagnostic, remove once the vanish-on-land bug is found
            NetworkServer.Destroy(item.gameObject);
        }
    }

    [Server]
    void StopHitting()
    {
        isHitting = false;
        col.enabled = false;
    }

    [Server]
    void DestroySelf()
    {
        NetworkServer.Destroy(gameObject);
    }
}
}
