using Mirror;
using UnityEngine;
using HR.Object.Player;

namespace HR.Map{
[RequireComponent(typeof(GridCell))]
public class DestructibleBlock : NetworkBehaviour
{
    [SerializeField] GameObject[] itemPrefabs;
    [Range(0f, 1f)] [SerializeField] float dropChance = 0.3f;
    [Header("Mount")]
    [SerializeField] GameObject[] mountPrefabs;
    [Tooltip("Rolled separately from (and before) the normal item drop above - a mount absorbs a whole hit, so it needs to be much rarer than a regular power-up.")]
    [Range(0f, 1f)] [SerializeField] float mountDropChance = 0.05f;
    [Tooltip("Skill Energy awarded to whoever destroyed this block - the v1 energy source per the roadmap (\"Destroy Block = +5\") until item pickups/survival time/kills are added.")]
    [SerializeField] float skillEnergyReward = 5f;

    [Server]
    public void Break(CharacterBase destroyer)
    {
        destroyer?.SkillComponent?.AddSkillEnergy(skillEnergyReward);

        if (mountPrefabs != null && mountPrefabs.Length > 0 && Random.value < mountDropChance)
        {
            DropItem(mountPrefabs[Random.Range(0, mountPrefabs.Length)]);
        }
        else if (itemPrefabs != null && itemPrefabs.Length > 0 && Random.value < dropChance)
        {
            DropItem(itemPrefabs[Random.Range(0, itemPrefabs.Length)]);
        }

        // Don't rely solely on NetworkServer.Destroy's scene-object handling
        // (SetActive(false), synced to clients as a separate message) -
        // disable collision/visuals here immediately, on the server's own
        // copy, so nothing can linger even for a moment.
        foreach (Collider col in GetComponentsInChildren<Collider>())
        {
            col.enabled = false;
        }
        foreach (Renderer rend in GetComponentsInChildren<Renderer>())
        {
            rend.enabled = false;
        }

        NetworkServer.Destroy(gameObject);
    }

    void DropItem(GameObject prefab)
    {
        // Destructible blocks can be scaled tall (obstacleLayers), so
        // transform.position may sit well above ground level - drop the
        // item at true floor height (matches where GridManager spawns
        // explosion segments), or its collider never overlaps a blast.
        // +0.25 so the placeholder sphere (0.25 radius) sits on top of
        // the floor instead of clipping half into it.
        Vector2Int coord = GridManager.Instance.WorldToGrid(transform.position);
        Vector3 dropPosition = GridManager.Instance.GridToWorld(coord) + Vector3.up * 0.25f;

        GameObject item = Instantiate(prefab, dropPosition, Quaternion.identity);
        NetworkServer.Spawn(item);
    }
}
}
