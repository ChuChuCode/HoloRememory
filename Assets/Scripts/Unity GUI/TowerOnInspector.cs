using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TowerBehaviour))]
public class TowerOnInspector : Editor
{
    public override void OnInspectorGUI()
    {
        TowerBehaviour towerBehaviour = (TowerBehaviour) target;
        GUILayout.Label ("Show/Hide Gizmos", EditorStyles.boldLabel,GUILayout.Height(30));
        if (GUILayout.Button("Show/Hide Gizmos"))
        {
            towerBehaviour.ShowGizmos = !towerBehaviour.ShowGizmos;
        }
        base.OnInspectorGUI();
    }
}
