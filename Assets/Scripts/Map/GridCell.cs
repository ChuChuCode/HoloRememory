using UnityEngine;

namespace HR.Map{
public enum CellType
{
    Wall,
    Destructible,
    Spawn,
    Bomb,
    // Level-designed marker for where MinionSpawner places a mob at match
    // start - same non-blocking role as Spawn (see GridManager.IsOccupied),
    // just a separate type so it isn't mixed into the player spawn pool.
    MinionSpawn
}

[DisallowMultipleComponent]
public class GridCell : MonoBehaviour
{
    public CellType type;

    [Header("Spawn Only")]
    public int teamId;
    public int spawnIndex;

    Vector2Int coord;
    // The authoritative logical cell - updated synchronously the instant a
    // move is decided (see UpdateCoord), even if the visual transform is
    // still mid-slide toward it. Anything that needs "which cell is this
    // object really in" (e.g. a bomb's own explosion origin) should read
    // this instead of re-deriving it from transform.position, which can
    // still be sitting somewhere between cells at that exact moment.
    public Vector2Int Coord => coord;

    // For a GridCell that physically moves after being registered (e.g.
    // Botan's SSRB chasing across tiles) - the mover must call this AND
    // re-register with GridManager itself on every hop, or this cell's
    // eventual OnDisable/Unregister call will still target its stale
    // spawn-time coord instead of wherever it ended up.
    public void UpdateCoord(Vector2Int newCoord)
    {
        coord = newCoord;
    }

    // OnEnable/OnDisable (not Awake/OnDestroy) because Mirror's
    // NetworkServer.Destroy on a scene-placed NetworkIdentity only calls
    // SetActive(false) - it never actually destroys the GameObject. Awake
    // only fires once, so OnDestroy would never come to unregister it,
    // leaving the cell permanently marked as occupied.
    void OnEnable()
    {
        if (GridManager.Instance == null) return;
        coord = GridManager.Instance.WorldToGrid(transform.position);
        GridManager.Instance.Register(coord, this);
    }

    void OnDisable()
    {
        if (GridManager.Instance == null) return;
        GridManager.Instance.Unregister(coord, this);
    }
}
}
