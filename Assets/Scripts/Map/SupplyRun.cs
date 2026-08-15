using System.Collections;
using UnityEngine;
using Mirror;

namespace HR.Map{
// Periodically flies this (single, reused) supply marker across the map -
// always the same fixed direction, idle position kept off-camera - and
// drops a few items that fall from the sky down to random clear cells.
// Keeps the item economy alive after all the destructible blocks near
// spawn are gone (items otherwise only ever came from breaking blocks).
// Item positions are decoupled from the flight path itself - the plane is
// a purely decorative cue that "a supply run is happening".
public class SupplyRun : NetworkBehaviour
{
    [SerializeField] Transform endPoint;
    [SerializeField] float flightDuration = 6f;
    [SerializeField] float interval = 35f;
    [SerializeField] GameObject[] itemPrefabs;
    [SerializeField] int minItems = 3;
    [SerializeField] int maxItems = 5;
    [SerializeField] float dropHeight = 8f;
    [SerializeField] float fallDuration = 0.8f;

    Vector3 idlePosition;

    // Errors (not warnings) and refuses to run at all when misconfigured -
    // this loop only fires every `interval` seconds, so a soft warning
    // would mean waiting through a full interval in Play mode before
    // noticing nothing happened.
    bool IsConfigValid()
    {
        bool valid = true;
        if (endPoint == null)
        {
            Debug.LogError($"{name}: SupplyRun has no End Point assigned - disabling this supply run.", this);
            valid = false;
        }
        if (itemPrefabs == null || itemPrefabs.Length == 0)
        {
            Debug.LogError($"{name}: SupplyRun has no Item Prefabs assigned - disabling this supply run.", this);
            valid = false;
        }
        return valid;
    }

    void Start()
    {
        idlePosition = transform.position;
        if (!isServer) return;
        if (!IsConfigValid()) return;
        StartCoroutine(RunLoop());
    }

    IEnumerator RunLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(interval);
            yield return FlyAndDrop();
        }
    }

    IEnumerator FlyAndDrop()
    {
        DropItems();

        float elapsed = 0f;
        while (elapsed < flightDuration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(idlePosition, endPoint.position, elapsed / flightDuration);
            yield return null;
        }
        transform.position = idlePosition; // reset for next run, off-camera again
    }

    void DropItems()
    {
        if (itemPrefabs == null || itemPrefabs.Length == 0) return;

        int count = Random.Range(minItems, maxItems + 1);
        for (int i = 0; i < count; i++)
        {
            if (!GridManager.Instance.TryGetRandomClearCoord(out Vector2Int coord)) continue;

            // Same +0.25 floor-clearance convention DestructibleBlock uses
            // for its own item drops.
            Vector3 groundPos = GridManager.Instance.GridToWorld(coord) + Vector3.up * 0.25f;
            Vector3 skyPos = groundPos + Vector3.up * dropHeight;

            GameObject prefab = itemPrefabs[Random.Range(0, itemPrefabs.Length)];
            GameObject item = Instantiate(prefab, skyPos, Quaternion.identity);
            NetworkServer.Spawn(item);
            StartCoroutine(FallToGround(item.transform, skyPos, groundPos));
        }
    }

    // Runs on the SupplyRun object, not the item itself, since the item
    // might get picked up (destroyed) mid-fall - the null check covers
    // that. The item's own NetworkTransform (server-authoritative, same as
    // Bomb/SSRB) is what replicates this to every client, not this
    // coroutine directly.
    IEnumerator FallToGround(Transform item, Vector3 start, Vector3 end)
    {
        float elapsed = 0f;
        while (elapsed < fallDuration && item != null)
        {
            elapsed += Time.deltaTime;
            item.position = Vector3.Lerp(start, end, elapsed / fallDuration);
            yield return null;
        }
        if (item != null) item.position = end;
    }
}
}
