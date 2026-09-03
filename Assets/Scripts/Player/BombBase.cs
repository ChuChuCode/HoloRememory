using UnityEngine;
using Mirror;
using HR.Object.Player;
using HR.Map;
using System.Collections;
using System.Collections.Generic;

namespace HR.Object.Skill{
public class BombBase : NetworkBehaviour
{
    [SerializeField] protected CharacterBase Owned;
    [SerializeField] protected Rigidbody Rd;
    [SerializeField] protected int AttackDamage;
    // SyncVar for the same reason as BombPower below - Rui's Hawk Eye reads
    // this (via FuseProgress) on the caster's own client, which isn't
    // necessarily the server, and needs the REAL fuse time (SetFuseTime can
    // shorten it, e.g. Botan's SSRB) rather than whatever the prefab default is.
    [SyncVar] [SerializeField] protected float BombTime;
    // SyncVar so a remote (non-host) client's own copy correctly reflects
    // the actual placing player's Fire Power - needed for Rui's Hawk Eye to
    // compute the right blast range for bombs it doesn't own. Nothing
    // client-side previously read this at all (SpawnExplosion is
    // [ServerCallback], so it never ran on clients anyway), so this only
    // adds correctness, nothing relies on the old plain-field behavior.
    [SyncVar] [SerializeField] protected int BombPower;
    private float Timer ;
    [SerializeField] GameObject ExplosionPrefab;

    [SerializeField] Collider bombCollider;
    // Skill bombs that don't come out of the owner's normal bomb count (e.g.
    // Botan's SSRB) never decremented it on spawn, so refunding one here on
    // explosion would hand out a free bonus bomb. True by default since
    // every normal bomb DOES come out of that count.
    protected bool countsTowardBombLimit = true;
    public void SetCountsTowardBombLimit(bool value)
    {
        countsTowardBombLimit = value;
    }

    HashSet<NetworkIdentity> playersOnBomb = new();
    // Cached so anything that moves this bomb after it's placed (Watame's
    // Bomb Push, SSRB's own chase hops) can re-register it without a
    // GetComponent lookup every time.
    protected GridCell gridCell;

    // Set on every peer (not just the server) - Rui's Hawk Eye runs entirely
    // on the caster's own client and needs this to compute FuseProgress()
    // for bombs it doesn't own.
    float spawnTime;

    protected virtual void Start()
    {
        spawnTime = Time.time;
        if (!isServer) return;
        Timer = BombTime;
        Invoke("SpawnExplosion", BombTime);
    }

    // 0 = just placed, 1 = about to explode - used by Rui's Hawk Eye to
    // color-code the highlight per bomb instead of a single flat color.
    public float FuseProgress()
    {
        if (BombTime <= 0f) return 1f;
        return Mathf.Clamp01((Time.time - spawnTime) / BombTime);
    }

    // void Update()
    // {
    //     if (!isServer) return;
    //     if (Timer > 0)
    //     {
    //         Timer -= Time.deltaTime;
    //     }
    //     else
    //     {
    //         // Spawn Explosion
    //         SpawnExplosion();
    //         NetworkServer.Destroy(gameObject);
    //     }
    // }

    public override void OnStartServer()
    {
        bombCollider.isTrigger = true; // ⭐ 關鍵
        // Register immediately (Spawn-time, not next-frame Start()) so a
        // rapid second CmdSpawnBomb can't sneak into the same cell first.
        gridCell = gameObject.AddComponent<GridCell>();
        gridCell.type = CellType.Bomb;
    }
    [ServerCallback]
    void OnTriggerEnter(Collider other)
    {
        var netId = other.GetComponentInParent<NetworkIdentity>();
        if (netId == null) return;

        playersOnBomb.Add(netId);
    }

    [ServerCallback]
    void OnTriggerExit(Collider other)
    {
        var netId = other.GetComponentInParent<NetworkIdentity>();
        if (netId == null) return;

        playersOnBomb.Remove(netId);

        // ⭐ 只有「最後一個人離開」才變成牆
        if (playersOnBomb.Count == 0)
        {
            bombCollider.isTrigger = false;
        }
    }
    public void SetOwner(CharacterBase owner)
    {
        Owned = owner;
    }
    public void SetPower(int power)
    {
        BombPower = power;
    }
    // Called before Start() runs (right after Instantiate, from
    // CmdSpawnBomb) so the shortened fuse is what Start() actually reads
    // into Timer/Invoke - e.g. Botan's quick-fuse skill.
    public void SetFuseTime(float time)
    {
        BombTime = time;
    }

    // Visually slides this bomb's transform to a world position over
    // `duration` seconds instead of snapping - shared by anything that
    // needs a bomb to look like it's actually traveling (SSRB's chase hops,
    // Watame's Bomb Push).
    protected IEnumerator SlideVisual(Vector3 destination, float duration)
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

    // Server-only, fire-and-forget: physically relocate this bomb to a new
    // grid cell over `duration` seconds, keeping GridManager registration
    // correct throughout. Used by Watame's Bomb Push.
    [Server]
    public void MoveToCell(Vector2Int destination, float duration)
    {
        StartCoroutine(MoveToCellRoutine(destination, duration));
    }
    protected IEnumerator MoveToCellRoutine(Vector2Int destination, float duration)
    {
        GridManager.Instance.Unregister(gridCell.Coord, gridCell);
        gridCell.UpdateCoord(destination);
        GridManager.Instance.Register(destination, gridCell);
        yield return SlideVisual(GridManager.Instance.GridToWorld(destination), duration);
    }

    bool hasExploded = false;

    [ServerCallback]
    public virtual void SpawnExplosion()
    {
        // Guards against chain-reaction re-entrancy: two bombs close enough
        // to catch each other in both directions would otherwise call back
        // into an explosion that's still mid-execution.
        if (hasExploded) return;
        hasExploded = true;

        // gridCell.Coord (not WorldToGrid(transform.position)) - if this
        // bomb is mid-slide (pushed, or an SSRB still chasing) when its
        // fuse runs out, the visual transform can still be sitting between
        // cells, but the logical cell was already updated the instant the
        // move was decided. Exploding from the visual position would have
        // rounded back to wherever it started.
        Vector2Int origin = gridCell.Coord;
        SpawnExplosionSegment(origin);

        // Side Explosion
        SideExoplosion(origin, Vector2Int.up);
        SideExoplosion(origin, Vector2Int.down);
        SideExoplosion(origin, Vector2Int.left);
        SideExoplosion(origin, Vector2Int.right);

        NetworkServer.Destroy(gameObject);

        // Frees up the slot this bomb was using - NOT a capacity increase
        // (that's AddBombCount, only from picking up a BombCount item).
        if (Owned != null && countsTowardBombLimit)
        {
            Owned.RefundBomb();
        }
    }
    // Dry-run version of SpawnExplosion's walk, for anything that needs to
    // know "which cells would this hit" without actually detonating - Rui's
    // Hawk Eye uses this to preview blast range. Mirrors SideExoplosion's
    // exact stopping rules (walls block entirely and aren't included;
    // destructibles/bombs ARE included since they'd be hit, but nothing
    // past them is) without side effects.
    public List<Vector2Int> GetBlastCells()
    {
        // gridCell only exists on whichever peer actually ran
        // OnStartServer (the server/host) - AddComponent calls made there
        // don't replicate to remote clients. A remote client calling this
        // (Rui's Hawk Eye runs entirely on the caster's own client) falls
        // back to the bomb's own networked transform position instead.
        Vector2Int origin = gridCell != null ? gridCell.Coord : GridManager.Instance.WorldToGrid(transform.position);
        List<Vector2Int> cells = new List<Vector2Int> { origin };
        cells.AddRange(GetBlastCellsInDirection(origin, Vector2Int.up));
        cells.AddRange(GetBlastCellsInDirection(origin, Vector2Int.down));
        cells.AddRange(GetBlastCellsInDirection(origin, Vector2Int.left));
        cells.AddRange(GetBlastCellsInDirection(origin, Vector2Int.right));
        return cells;
    }
    List<Vector2Int> GetBlastCellsInDirection(Vector2Int origin, Vector2Int direction)
    {
        List<Vector2Int> cells = new List<Vector2Int>();
        for (int i = 1; i <= BombPower; i++)
        {
            Vector2Int coord = origin + direction * i;
            if (GridManager.Instance.TryGetCell(coord, out GridCell cell))
            {
                if (cell.type == CellType.Wall) break;
                cells.Add(coord);
                if (cell.type == CellType.Destructible || cell.type == CellType.Bomb) break;
            }
            else
            {
                cells.Add(coord);
            }
        }
        return cells;
    }
    // Walk the grid step by step in one direction, stopping at walls and
    // breaking (then stopping past) the first destructible block found.
    private void SideExoplosion(Vector2Int origin, Vector2Int direction)
    {
        for (int i = 1; i <= BombPower; i++)
        {
            Vector2Int coord = origin + direction * i;

            if (GridManager.Instance.TryGetCell(coord, out GridCell cell))
            {
                if (cell.type == CellType.Wall) return;

                if (cell.type == CellType.Destructible)
                {
                    // No explosion segment here - the block vanishing is
                    // feedback enough, and this cell (plus any item it drops)
                    // is immediately safe to walk into instead of making
                    // players wait out a hit window for a tile with nothing
                    // left to explode.
                    cell.GetComponent<DestructibleBlock>().Break();
                    return;
                }

                if (cell.type == CellType.Bomb)
                {
                    // Chain reaction: detonate it now instead of waiting for its own timer.
                    cell.GetComponent<BombBase>().SpawnExplosion();
                    return;
                }
            }

            SpawnExplosionSegment(coord);
        }
    }
    private void SpawnExplosionSegment(Vector2Int coord)
    {
        GameObject explosion = Instantiate(ExplosionPrefab, GridManager.Instance.GridToWorld(coord), Quaternion.identity);
        NetworkServer.Spawn(explosion);
    }
    // [ServerCallback]
    // void OnTriggerEnter(Collider other) 
    // {
    //     if (other.transform.root == Target)
    //     {
    //         Health health = other.transform.root.GetComponent<Health>();
    //         if (health is CharacterBase )
    //         {
    //             CharacterBase character = health as CharacterBase;
    //             bool isdead = character.HealthDamage(AttackDamage);
    //             if (isdead)
    //             {
    //                 TriggerCharacterBaseDead(character);
    //             }
    //         }
    //         // Destory Ball
    //         NetworkServer.Destroy(gameObject);
    //     }
    // }
    // protected virtual void TriggerCharacterBaseDead(CharacterBase characterBase)
    // {
    //     characterBase.AddKDA("death");
    // }
}

}