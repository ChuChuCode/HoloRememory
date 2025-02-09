using UnityEditor;
using UnityEngine;
using HR.Object.Map;

namespace HR.GUI{
[CustomEditor(typeof(TowerBase),true)]
public class TowerOnInspector : Editor
{
    public override void OnInspectorGUI()
    {
        TowerBase towerBehaviour = (TowerBase) target;
        GUILayout.Label ("Show/Hide Gizmos", EditorStyles.boldLabel,GUILayout.Height(30));
        if (GUILayout.Button("Show/Hide Gizmos"))
        {
            towerBehaviour.ShowGizmos = !towerBehaviour.ShowGizmos;
        }
        base.OnInspectorGUI();
    }
}

}