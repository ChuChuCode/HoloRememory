using System.Collections.Generic;
using UnityEngine;

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

    public IEnumerable<Vector2Int> GetAllSpawnCoords()
    {
        foreach (KeyValuePair<Vector2Int, GridCell> entry in cells)
        {
            if (entry.Value.type == CellType.Spawn) yield return entry.Key;
        }
    }

    List<GridCell> GetTeamSpawnCells(int teamId)
    {
        List<GridCell> spawnCells = new();
        foreach (GridCell cell in cells.Values)
        {
            if (cell.type == CellType.Spawn && cell.teamId == teamId)
            {
                spawnCells.Add(cell);
            }
        }
        return spawnCells;
    }

    // playerIndex is which player of that team this is (0, 1, 2, ...);
    // wraps around if there are fewer spawn cells than players. Used only
    // for the initial spawn, where round-robin avoids two players landing
    // on the same point.
    public Vector3 GetSpawnPosition(int teamId, int playerIndex)
    {
        List<GridCell> spawnCells = GetTeamSpawnCells(teamId);
        if (spawnCells.Count == 0)
        {
            Debug.LogWarning($"No spawn point registered for team {teamId}.");
            return Vector3.zero;
        }

        spawnCells.Sort((a, b) => a.spawnIndex.CompareTo(b.spawnIndex));
        return spawnCells[playerIndex % spawnCells.Count].transform.position;
    }

    // Used for mid-match respawns (MultiLife mode) - random, so camping one
    // spawn point can't guarantee a kill, and skips any spawn point that's
    // currently blocked (e.g. a bomb sitting on it) when possible.
    public Vector3 GetRandomSpawnPosition(int teamId)
    {
        List<GridCell> spawnCells = GetTeamSpawnCells(teamId);
        if (spawnCells.Count == 0)
        {
            Debug.LogWarning($"No spawn point registered for team {teamId}.");
            return Vector3.zero;
        }

        List<GridCell> clear = spawnCells.FindAll(cell => !IsOccupied(WorldToGrid(cell.transform.position)));
        List<GridCell> candidates = clear.Count > 0 ? clear : spawnCells;

        return candidates[Random.Range(0, candidates.Count)].transform.position;
    }
}
}
