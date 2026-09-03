using System.Collections;
using Mirror;
using UnityEngine;
using HR.Map;
using HR.Network;
using HR.Object.Player;

namespace HR.Object.Minions{
// PVE mob: wanders the grid one cell at a time (same discrete-hop shape as
// Botan's SSRB chase - Assets/Scripts/Player/Botan/SSRBBomb.cs - just an
// undirected pick instead of chasing a target), and takes damage from bomb
// blasts via ExplosionSegment's Minion_List check. Deliberately NOT a
// CharacterBase - it doesn't belong in Player_List (would corrupt
// HUD/game-over/spawn-point logic that assumes every entry there is an
// actual player).
//
// Not registered as a GridCell, same reasoning as players (see
// GridManager.IsPlayerAt's comment - a mover shouldn't block itself or
// others from walking/blasting through it): a bomb blast passes straight
// through a minion's cell rather than being stopped by it, exactly like it
// already does for players - ExplosionSegment.Update() is what actually
// damages it, not the blast-walk in BombBase.SideExoplosion.
public class Minion : Health
{
    // Seconds to slide across one cell - hops chain back to back with no
    // pause in between (see WanderRoutine), so this doubles as the walking
    // speed, not a rest period.
    [SerializeField] float moveInterval = 0.6f;
    [SerializeField] int contactDamage = 1;
    // Repeat-hit cooldown while a player stands on the same cell as this
    // minion, so contact doesn't shred health every single frame.
    [SerializeField] float contactCooldown = 1f;

    Vector2Int currentCoord;
    int lives;
    float nextContactTime;
    Network_Manager manager;

    Network_Manager Manager
    {
        get
        {
            if (manager != null) return manager;
            return manager = Network_Manager.singleton as Network_Manager;
        }
    }

    protected override void Awake()
    {
        base.Awake();
        Manager.Minion_List.Add(this);
    }

    public override void OnStartServer()
    {
        currentCoord = GridManager.Instance.WorldToGrid(transform.position);

        // Same mode-driven split players already get (see
        // Network_Manager.OnServerSceneChanged's Game-scene branch):
        // overrideHealth true (OneLife/MultiLife) forces one-shot death;
        // false (HealthBar) keeps whatever maxHealth this prefab was
        // configured with, giving it an actual multi-hit health bar.
        GameModeConfig modeConfig = GameSettings.Instance != null ? GameSettings.Instance.CurrentConfig() : null;
        if (modeConfig != null)
        {
            if (modeConfig.overrideHealth) maxHealth = modeConfig.maxHealth;
            lives = modeConfig.lives;
        }
        else
        {
            lives = 1;
        }
        InitialHealth();

        StartCoroutine(WanderRoutine());
    }

    // Physics-based, unlike bomb blast damage (ExplosionSegment, which is
    // grid-cell comparison) - per spec, contact should reflect an actual
    // collision, not "happens to round to the same logical cell." Needs a
    // Collider on this prefab with isTrigger = true.
    [ServerCallback]
    void OnTriggerStay(Collider other)
    {
        if (isDead) return;
        if (Time.time < nextContactTime) return;

        CharacterBase character = other.GetComponentInParent<CharacterBase>();
        if (character == null || character.isDead) return;

        character.HealthDamage(contactDamage);
        nextContactTime = Time.time + contactCooldown;
    }

    static readonly Vector2Int[] Directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

    // No pause between hops - chains straight into the next slide so the
    // minion reads as continuously walking instead of stopping dead between
    // each cell. moveInterval is purely "how long one hop's slide takes"
    // now, not a rest period.
    IEnumerator WanderRoutine()
    {
        while (!isDead)
        {
            Vector2Int dir = Directions[Random.Range(0, Directions.Length)];
            Vector2Int next = currentCoord + dir;
            if (GridManager.Instance.IsOccupied(next))
            {
                // Boxed in on all sides - wait a frame rather than spinning
                // the loop with no yield at all, which would hang Unity.
                yield return null;
                continue;
            }

            currentCoord = next;
            yield return SlideVisual(GridManager.Instance.GridToWorld(next), moveInterval);
        }
    }

    IEnumerator SlideVisual(Vector3 destination, float duration)
    {
        Vector3 start = transform.position;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(start, destination, elapsed / duration);
            yield return null;
        }
        transform.position = destination; // guarantee an exact landing despite frame-time drift
    }

    // Instant, in-place revive when a life remains - unlike CharacterBase's
    // delayed respawn at a new random point, per spec: a mob just keeps
    // wandering from wherever it died, no downtime.
    protected override void OnHealthDepleted()
    {
        lives -= 1;
        if (lives > 0)
        {
            InitialHealth();
            return;
        }
        base.OnHealthDepleted(); // isDead = true -> Death() via the hook
    }

    protected override void Death()
    {
        if (isServer) NetworkServer.Destroy(gameObject);
    }

    void OnDestroy()
    {
        if (manager != null) manager.Minion_List.Remove(this);
    }
}
}
