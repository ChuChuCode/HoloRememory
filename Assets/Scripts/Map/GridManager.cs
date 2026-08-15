using System.Collections.Generic;
using UnityEngine;
using HR.Network;
using HR.Object.Player;

namespace HR.Map{
public class GridManager : MonoBehaviour
{
    static GridManager instance;
    public static GridManager Instance
    {
        get
        {
            if (instance != null)
            {
                return instance;
            }
            return instance = FindObjectOfType<GridManager>();
        }
    }

    [SerializeField] float cellSize = 1f;

    Dictionary<Vector2Int, GridCell> cells = new();

    // Cell "n" is centered at world n*cellSize (matches how GridSpawnerEditor
    // actually places floor/wall tiles) - so bucket to the NEAREST cell
    // center, not the floor of the raw position.
    public Vector2Int WorldToGrid(Vector3 worldPosition)
    {
        return new Vector2Int(
            Mathf.RoundToInt(worldPosition.x / cellSize),
            Mathf.RoundToInt(worldPosition.z / cellSize));
    }

    public Vector3 GridToWorld(Vector2Int coord)
    {
        return new Vector3(coord.x * cellSize, 0f, coord.y * cellSize);
    }

    public void Register(Vector2Int coord, GridCell cell)
    {
        cells[coord] = cell;
    }

    public void Unregister(Vector2Int coord, GridCell cell)
    {
        if (cells.TryGetValue(coord, out GridCell existing) && existing == cell)
        {
            cells.Remove(coord);
        }
    }

    public bool TryGetCell(Vector2Int coord, out GridCell cell)
    {
        return cells.TryGetValue(coord, out cell);
    }

    // Spawn markers don't block anything - only Wall/Destructible/Bomb do.
    public bool IsOccupied(Vector2Int coord)
    {
        return TryGetCell(coord, out GridCell cell) && cell.type != CellType.Spawn;
    }

    // Characters aren't registered as cells at all (by design - players
    // don't block each other's movement or bomb placement), so this is a
    // live position check, not a dictionary lookup. Deliberately separate
    // from IsOccupied() rather than merged into it - a caster's own cell
    // would otherwise always read as "occupied" by themselves, breaking
    // normal bomb placement. Callers that also care about players (e.g.
    // Watame's Bomb Push stopping on contact) combine both checks
    // themselves.
    public bool IsPlayerAt(Vector2Int coord)
    {
        Network_Manager manager = Network_Manager.singleton as Network_Manager;
        if (manager == null) return false;

        foreach (CharacterBase player in manager.Player_List)
        {
            if (player.isDead) continue;
            if (WorldToGrid(player.transform.position) == coord) return true;
        }
        return false;
    }

    // Random walkable floor coordinate, sampled from the rectangular area
    // spanned by every registered cell - plain floor tiles are never
    // registered at all, so this is the closest available stand-in for
    // "the playable map area". Retries until it lands on something that
    // isn't Wall/Destructible/Bomb, giving up after a bounded number of
    // attempts (e.g. an unusually cramped or oddly-shaped map).
    public bool TryGetRandomClearCoord(out Vector2Int coord)
    {
        coord = default;
        if (cells.Count == 0) return false;

        int minX = int.MaxValue, maxX = int.MinValue, minZ = int.MaxValue, maxZ = int.MinValue;
        foreach (Vector2Int key in cells.Keys)
        {
            minX = Mathf.Min(minX, key.x);
            maxX = Mathf.Max(maxX, key.x);
            minZ = Mathf.Min(minZ, key.y);
            maxZ = Mathf.Max(maxZ, key.y);
        }

        for (int attempt = 0; attempt < 30; attempt++)
        {
            Vector2Int candidate = new Vector2Int(Random.Range(minX, maxX + 1), Random.Range(minZ, maxZ + 1));
            if (!IsOccupied(candidate))
            {
                coord = candidate;
                return true;
            }
        }
        return false;
    }

    public IEnumerable<Vector2Int> GetAllSpawnCoords()
    {
        foreach (KeyValuePair<Vector2Int, GridCell> entry in cells)
        {
            if (entry.Value.type == CellType.Spawn) yield return entry.Key;
        }
    }

    // Initial match spawn - random from the WHOLE map's spawn point pool
    // (ignores team/color entirely, since a color is no longer a strict
    // side of the map). usedCoords tracks what's already been handed out
    // this spawn pass so two players don't land on the same point; falls
    // back to reusing a point only if there are more players than points.
    public Vector3 GetRandomUnusedSpawnPosition(HashSet<Vector2Int> usedCoords)
    {
        List<GridCell> allSpawnCells = new();
        foreach (GridCell cell in cells.Values)
        {
            if (cell.type == CellType.Spawn) allSpawnCells.Add(cell);
        }
        if (allSpawnCells.Count == 0)
        {
            Debug.LogWarning("No spawn points registered on this map.");
            return Vector3.zero;
        }

        List<GridCell> unused = allSpawnCells.FindAll(cell => !usedCoords.Contains(WorldToGrid(cell.transform.position)));
        List<GridCell> candidates = unused.Count > 0 ? unused : allSpawnCells;

        GridCell chosen = candidates[Random.Range(0, candidates.Count)];
        Vector2Int coord = WorldToGrid(chosen.transform.position);
        usedCoords.Add(coord);
        // Snap X/Z to the exact cell center - hand-placed spawn markers can
        // be slightly off. Keep the marker's own Y so any intentional
        // height (e.g. clear of the floor mesh) isn't flattened to 0.
        Vector3 snapped = GridToWorld(coord);
        return new Vector3(snapped.x, chosen.transform.position.y, snapped.z);
    }

    // Used for mid-match respawns (MultiLife mode) - truly random from the
    // same whole-map pool as the initial spawn (no team scoping - camping
    // one point can't guarantee a kill). Only avoids Wall/Destructible/Bomb
    // (via IsOccupied) when possible; landing near/on another player is
    // fine, that just means an immediate fight.
    public Vector3 GetRandomSpawnPosition()
    {
        List<GridCell> allSpawnCells = new();
        foreach (GridCell cell in cells.Values)
        {
            if (cell.type == CellType.Spawn) allSpawnCells.Add(cell);
        }
        if (allSpawnCells.Count == 0)
        {
            Debug.LogWarning("No spawn points registered on this map.");
            return Vector3.zero;
        }

        List<GridCell> clear = allSpawnCells.FindAll(cell => !IsOccupied(WorldToGrid(cell.transform.position)));
        List<GridCell> candidates = clear.Count > 0 ? clear : allSpawnCells;

        GridCell chosen = candidates[Random.Range(0, candidates.Count)];
        Vector2Int coord = WorldToGrid(chosen.transform.position);
        // Snap X/Z to the exact cell center - hand-placed spawn markers can
        // be slightly off. Keep the marker's own Y so any intentional
        // height isn't flattened to 0.
        Vector3 snapped = GridToWorld(coord);
        return new Vector3(snapped.x, chosen.transform.position.y, snapped.z);
    }
}
}
