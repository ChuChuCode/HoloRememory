using System.Collections.Generic;
using UnityEngine;
using Mirror;
using HR.Object.Player;
using HR.Object.Minions;
using HR.Map;
using HR.Network;

namespace HR.Object.Skill{
// Miko's Blaze - spawned by BombBase.SpawnExplosionSegment alongside the
// normal ExplosionSegment on every blast tile, whenever the exploding bomb
// was armed via SetLeavesLingeringFire. It's spawned immediately (same
// frame as the explosion, since the bomb GameObject that would otherwise
// own a "wait then spawn" coroutine gets destroyed moments later) but stays
// dormant - no visual, no damage - until StartDelay has passed, so it reads
// as "fire left behind after the blast" instead of overlapping with the
// explosion itself. Once active, it repeatedly damages anyone standing in
// it on a per-target cooldown instead of a permanent "already hit" set -
// unlike ExplosionSegment, someone who walks in late still takes damage,
// and someone who stays the whole time gets hit more than once.
public class LingeringFire : NetworkBehaviour
{
    float lifeTime = 0.75f;
    float tickInterval = 0.25f;
    float startDelay = 0.5f;
    [SerializeField] int damage = 1;

    Vector2Int cell;
    bool isActive;
    MeshRenderer meshRenderer;
    Dictionary<CharacterBase, float> nextHitTime = new();
    Dictionary<Minion, float> nextHitTimeMinion = new();

    // Set right after Instantiate, before NetworkServer.Spawn - same timing
    // as BombBase's SetOwner/SetPower. startDelay should match the normal
    // ExplosionSegment's own visual/hit-window duration (BombBase's
    // explosionVisualDuration) so the fire only appears once the blast
    // itself has fully played out.
    public void Configure(float lifeTime, float tickInterval, float startDelay)
    {
        this.lifeTime = lifeTime;
        this.tickInterval = tickInterval;
        this.startDelay = startDelay;
    }

    // Runs on every peer (server AND clients) the instant this is
    // instantiated - hides the flat fire tile so it doesn't visually
    // appear before the delay is up. RpcActivate below is what actually
    // shows it once the delay elapses.
    void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null) meshRenderer.enabled = false;
    }

    public override void OnStartServer()
    {
        cell = GridManager.Instance.WorldToGrid(transform.position);
        Invoke(nameof(ServerActivate), startDelay);
    }

    [Server]
    void ServerActivate()
    {
        isActive = true;
        RpcActivate();
        Invoke(nameof(DestroySelf), lifeTime);
    }

    [ClientRpc]
    void RpcActivate()
    {
        if (meshRenderer != null) meshRenderer.enabled = true;
    }

    // Grid-based, same center-of-mass check as ExplosionSegment - a
    // per-target cooldown (not a permanent HashSet) is what lets this deal
    // damage more than once over its lifetime.
    [ServerCallback]
    void Update()
    {
        if (!isActive) return;

        Network_Manager manager = Network_Manager.singleton as Network_Manager;
        if (manager == null) return;

        foreach (CharacterBase character in manager.Player_List)
        {
            if (character.isDead) continue;
            if (GridManager.Instance.WorldToGrid(character.transform.position) != cell) continue;
            if (nextHitTime.TryGetValue(character, out float next) && Time.time < next) continue;
            character.HealthDamage(damage);
            nextHitTime[character] = Time.time + tickInterval;
        }

        foreach (Minion minion in manager.Minion_List)
        {
            if (minion.isDead) continue;
            if (GridManager.Instance.WorldToGrid(minion.transform.position) != cell) continue;
            if (nextHitTimeMinion.TryGetValue(minion, out float next) && Time.time < next) continue;
            minion.HealthDamage(damage);
            nextHitTimeMinion[minion] = Time.time + tickInterval;
        }
    }

    [Server]
    void DestroySelf()
    {
        NetworkServer.Destroy(gameObject);
    }
}
}
