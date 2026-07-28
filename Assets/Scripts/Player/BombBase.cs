using UnityEngine;
using Mirror;
using HR.Object.Player;
using HR.Map;
using System.Collections.Generic;

namespace HR.Object.Skill{
public class BombBase : NetworkBehaviour
{
    [SerializeField] protected CharacterBase Owned;
    [SerializeField] protected Rigidbody Rd;
    [SerializeField] protected int AttackDamage;
    [SerializeField] protected float BombTime;
    [SerializeField] protected int BombPower;
    private float Timer ;
    [SerializeField] GameObject ExplosionPrefab;

    [SerializeField] Collider bombCollider;

    HashSet<NetworkIdentity> playersOnBomb = new();

    void Start()
    {
        if (!isServer) return;
        Timer = BombTime;
        Invoke("SpawnExplosion", BombTime);
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
        gameObject.AddComponent<GridCell>().type = CellType.Bomb;
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
    bool hasExploded = false;

    [ServerCallback]
    public virtual void SpawnExplosion()
    {
        // Guards against chain-reaction re-entrancy: two bombs close enough
        // to catch each other in both directions would otherwise call back
        // into an explosion that's still mid-execution.
        if (hasExploded) return;
        hasExploded = true;

        // To be override
        Vector2Int origin = GridManager.Instance.WorldToGrid(transform.position);
        SpawnExplosionSegment(origin);

        // Side Explosion
        SideExoplosion(origin, Vector2Int.up);
        SideExoplosion(origin, Vector2Int.down);
        SideExoplosion(origin, Vector2Int.left);
        SideExoplosion(origin, Vector2Int.right);

        NetworkServer.Destroy(gameObject);
        // Add Bomb Count to CharacterBase

        if (Owned != null)
        {
            Owned.AddBombCount(1);
        }
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