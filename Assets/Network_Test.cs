using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Network_Test : MonoBehaviour
{
    void OnGUI() 
    {
        GUILayout.BeginArea(new Rect(10, 40 ,300, 9999));

        GUILayout.Label($"NetworkClient.isConnected: {NetworkClient.isConnected}");
        GUILayout.Label($"NetworkClient.active: {NetworkClient.active}");
        GUILayout.Label($"NetworkClient.ready: {NetworkClient.ready}");
        GUILayout.Label($"NetworkServer.active: {NetworkServer.active}");

        GUILayout.EndArea();
    }
}
