using UnityEditor;
using UnityEngine;

public class SetTeamInspector : EditorWindow
{
    [MenuItem("Window/Set Team")]
    public static void ShowWindow()
    {
        GetWindow<SetTeamInspector>("Set Team");
    }
    void OnGUI()
    {
        GUILayout.Label("Set Team",EditorStyles.boldLabel,GUILayout.Width(300), GUILayout.Height(50));
        GUILayout.BeginHorizontal();
            if (GUILayout.Button("Team 1",GUILayout.Width(100), GUILayout.Height(50)))
            {
                foreach(GameObject obj in Selection.gameObjects)
                {
                    // Set Layer to all
                    Transform[] children = obj.GetComponentsInChildren<Transform>(includeInactive: true);
                    foreach(Transform child in children)
                    {
                        child.gameObject.layer =  LayerMask.NameToLayer("Team1");
                    }
                }
            }
            if (GUILayout.Button("Team 2",GUILayout.Width(100), GUILayout.Height(50)))
            {
                foreach(GameObject obj in Selection.gameObjects)
                {
                    // Set Layer to all
                    Transform[] children = obj.GetComponentsInChildren<Transform>(includeInactive: true);
                    foreach(Transform child in children)
                    {
                        child.gameObject.layer =  LayerMask.NameToLayer("Team2");
                    }
                }
            }
        GUILayout.EndHorizontal();
    }
}
