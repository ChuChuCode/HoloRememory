using UnityEngine;
using HR.Map;

namespace HR.Object.Player{
// Debug visual: snaps this object (a flat colored quad, parented under a
// character) to whichever grid cell that character currently occupies -
// the cell center, not the character's exact continuous position, so it's
// clear which discrete cell actually counts while testing anything
// grid-based (Jump landing, Bomb Push distance, bomb placement, SSRB's
// chase). Being parented also means it inherits SetPresence's existing
// renderer-disable-on-death behavior for free - no extra code needed to
// hide it when the character is dead/waiting to respawn.
public class GridCellHighlight : MonoBehaviour
{
    CharacterBase owner;

    void Awake()
    {
        owner = GetComponentInParent<CharacterBase>();
    }

    // Not Awake() - isLocalPlayer isn't guaranteed to be set correctly yet
    // that early in a spawned networked object's lifecycle (same reason
    // CharacterBase itself gates its own Start() on isLocalPlayer rather
    // than checking in Awake()).
    void Start()
    {
        // Debug aid for the local player's own alignment only - without
        // this every client would render every other player's highlight
        // too, cluttering the view with boxes nobody asked to see.
        if (owner != null && !owner.isLocalPlayer)
        {
            enabled = false;
            GetComponent<MeshRenderer>().enabled = false;
        }
    }

    void Update()
    {
        if (owner == null || GridManager.Instance == null) return;
        Vector3 cellCenter = GridManager.Instance.GridToWorld(GridManager.Instance.WorldToGrid(owner.transform.position));
        transform.position = new Vector3(cellCenter.x, transform.position.y, cellCenter.z);
        // Characters spawn facing the map center (Network_Manager uses
        // Quaternion.LookRotation toward the origin), which would otherwise
        // rotate this along with its parent - force it back to flat/
        // axis-aligned every frame so it stays square with the grid
        // regardless of which way the character is facing.
        transform.rotation = Quaternion.identity;
    }
}
}
