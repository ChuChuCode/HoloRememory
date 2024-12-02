using UnityEngine;
using Mirror;
using HR.Object.Minion;
using HR.UI;

public class MinionSpawner : NetworkBehaviour
{
    [SerializeField] Minion Minion_Prefab;
    [SerializeField] string layerName;
    int time_interval = 30;
    [SerializeField] Transform EnemyTarget;
    [SerializeField] int SpawnTime = 10;
    int next_SpawnTime;
    void Start()
    {
        next_SpawnTime = SpawnTime;
    }
    void Update()
    {
        if (GameDuration.Instance.timer > next_SpawnTime)
        {
            next_SpawnTime += time_interval;
            // Spawn Minion
            Minion temp_Minion = Instantiate(Minion_Prefab,transform.position,transform.rotation);
            // Set Target
            temp_Minion.FinalDestination = EnemyTarget;
            // Set Layer
            temp_Minion.gameObject.layer = LayerMask.NameToLayer(layerName);
            NetworkServer.Spawn(temp_Minion.gameObject);
        }
    }
    void SpawnOnce()
    {

    }
}
