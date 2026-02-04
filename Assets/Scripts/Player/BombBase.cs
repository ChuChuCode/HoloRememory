using UnityEngine;
using Mirror;
using HR.Object.Player;
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
    [SerializeField] Transform SpawnPoint;

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
    [ServerCallback]
    public virtual void SpawnExplosion()
    {
        // To be override
        GameObject explosion = Instantiate(ExplosionPrefab, SpawnPoint.position, Quaternion.identity);
        NetworkServer.Spawn(explosion);

        // Side Explosion
        SideExoplosion(Vector2.up);
        SideExoplosion(Vector2.down);
        SideExoplosion(Vector2.left);
        SideExoplosion(Vector2.right);
        
        NetworkServer.Destroy(gameObject);
        // Add Bomb Count to CharacterBase

        if (Owned != null)
        {
            Owned.AddBombCount(1);
        }
    }
    private void SideExoplosion(Vector2 direction)
    {
        for (int i = 1; i <= BombPower; i++)
        {
            Vector3 spawnPos = transform.position + new Vector3(direction.x * i, 0f, direction.y * i)*0.25f;
            GameObject explosion = Instantiate(ExplosionPrefab, spawnPos, Quaternion.identity);
            NetworkServer.Spawn(explosion);
        }
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