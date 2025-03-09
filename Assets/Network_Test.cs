using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using HR.Network;

public class Network_Test : MonoBehaviour
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
    void OnGUI() 
    {
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.yellow;
        style.fontSize = 20;
        
        GUILayout.BeginArea(new Rect(10, 40 ,300, 9999),style);

        GUILayout.Label($"NetworkClient.isConnected: {NetworkClient.isConnected}",style);
        GUILayout.Label($"NetworkClient.active: {NetworkClient.active}",style);
        GUILayout.Label($"NetworkClient.ready: {NetworkClient.ready}",style);
        GUILayout.Label($"NetworkServer.active: {NetworkServer.active}",style);

        GUILayout.Space(20);

        foreach (var player in NetworkServer.connections)
        {
            GUILayout.Label($"Player {player.Value.connectionId} - {player.Value.identity.gameObject.name}",style);
        }

        GUILayout.EndArea();
    }
}
