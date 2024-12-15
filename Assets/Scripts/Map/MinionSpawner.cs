using UnityEngine;
using Mirror;
using HR.Object.Minion;
using HR.UI;
using System.Collections;

public class MinionSpawner : NetworkBehaviour
{
    [SerializeField] Minion Minion_Prefab;
    [SerializeField] string layerName;
    [SerializeField] Transform EnemyTarget;
    [SerializeField] int first_SpawnTime = 10;
    [SerializeField] int spawntime_interval = 30;
    [SerializeField] int SpawnNumber = 6;
    int next_SpawnTime;

    void Start()
    {
        next_SpawnTime = first_SpawnTime;
    }
    void Update()
    {
        if (GameDuration.Instance.timer > next_SpawnTime)
        {
            next_SpawnTime += spawntime_interval;
            
            StartCoroutine(nameof(SpawnOnce));
        }
    }
    IEnumerator SpawnOnce()
    {   
        for (int i = 0 ; i < SpawnNumber ; i++)
        {
            // Spawn Minion
            Minion temp_Minion = Instantiate(Minion_Prefab,transform.position,transform.rotation);
            // Set Target
            temp_Minion.MainDestination = EnemyTarget;
            // Set Layer for all child
            // Set Layer to all
            Transform[] children = temp_Minion.GetComponentsInChildren<Transform>(includeInactive: true);
            foreach(Transform child in children)
            {
                child.gameObject.layer = LayerMask.NameToLayer(layerName);
            }

            // Update Enemy Layer
            temp_Minion.Update_Enemy_Layer(LayerMask.NameToLayer(layerName));
            NetworkServer.Spawn(temp_Minion.gameObject);
            yield return new WaitForSeconds(1f);
        }
    }
}
