using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HR.Object.Map;
using HR.Network;
using HR.Global;
using HR.UI;
using IO.Swagger.Model;

public class MainTowerBehaviour : TowerBase
{
    private Network_Manager manager;

    public Network_Manager Manager
    {
        get
        {
            if (manager != null)
            {
                return manager;
            }
            return manager = Network_Manager.singleton as Network_Manager;
        }
    }
    void Update()
    {
        if (!isDead && currentHealth <= 0)
        {
            isDead = true;
            Death();
        }
    }
    protected override void Death()
    {
        base.Death();
        StartCoroutine(nameof(EndGame));
    }
    IEnumerator EndGame()
    {
        // Show Result UI
        StatusController.Instance.gameObject.SetActive(true);
        // Show Win/Lose
        StatusController.Instance.Show_Result( LayerMask.LayerToName(gameObject.layer) );
        // Set Lose Team to Network_Manager
        Manager.LoseTeam = gameObject.layer;
        yield return new WaitForSeconds(5f);
        Manager.ChangeScene("Result_Scene");
    }
}
