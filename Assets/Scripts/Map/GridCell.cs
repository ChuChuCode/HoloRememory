using UnityEngine;

namespace HR.Map{
public enum CellType
{
    Wall,
    Destructible,
    Spawn,
    Bomb
}

[DisallowMultipleComponent]
public class GridCell : MonoBehaviour
{
    public CellType type;

    [Header("Spawn Only")]
    public int teamId;
    public int spawnIndex;

    Vector2Int coord;

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
