using UnityEngine;
using Mirror;
using HR.Object.Minion;

public class MinionSpawner : NetworkBehaviour
{
    [SerializeField] Minion Minion_Prefab;
    [SerializeField] string layerName;
    float time_interval = 5f;
    [SerializeField] Transform EnemyTarget;
    void Update()
    {
        if (Time.time % time_interval < 0.01)
        {
            print("Spawn");
            // Spawn Minion
            Minion temp_Minion = Instantiate(Minion_Prefab,transform.position,transform.rotation);
            // Set Target
            temp_Minion.FinalDestination = EnemyTarget;
            // Set Layer
            temp_Minion.gameObject.layer = LayerMask.NameToLayer(layerName);
            NetworkServer.Spawn(temp_Minion.gameObject);
        }
    }
}
