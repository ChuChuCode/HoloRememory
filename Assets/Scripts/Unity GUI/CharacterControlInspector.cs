using UnityEditor;
using UnityEngine;
using HR.Object.Player;

namespace HR.GUI{
[CustomEditor(typeof(SubaruMovementController))]
public class CharacterControlInspector : Editor
{
    #region SerializedProperties
    SerializedProperty IsPressed_QProp;
    SerializedProperty IsPressed_WProp;
    SerializedProperty IsPressed_EProp;
    SerializedProperty IsPressed_RProp;
    #endregion
    GUIStyle SkillPress,SkillNoPress;

    void OnEnable()
    {
        // Setup the SerializedProperties.
        IsPressed_QProp = serializedObject.FindProperty("IsPressed_Q");
        IsPressed_WProp = serializedObject.FindProperty("IsPressed_W");
        IsPressed_EProp = serializedObject.FindProperty("IsPressed_E");
        IsPressed_RProp = serializedObject.FindProperty("IsPressed_R");

        SkillPress = new GUIStyle();
        SkillPress.fontSize = 18;
        SkillPress.fontStyle = FontStyle.Bold;
        SkillPress.normal.textColor = Color.green;

        SkillNoPress = new GUIStyle();
        SkillNoPress.fontSize = 18;
        SkillPress.fontStyle = FontStyle.Bold;
        SkillNoPress.normal.textColor = Color.white;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update ();
        CharacterBase characterBase = (CharacterBase) target;
        GUILayout.Label("Skill",EditorStyles.boldLabel,GUILayout.Width(300), GUILayout.Height(50));
        GUILayout.BeginHorizontal();
        if (IsPressed_QProp.boolValue)
        {
            GUILayout.Label("Q",SkillPress);
        }
        else
        {
            GUILayout.Label("Q",SkillNoPress);
        }
        if (IsPressed_WProp.boolValue)
        {
            GUILayout.Label("W",SkillPress);
        }
        else
        {
            GUILayout.Label("W",SkillNoPress);
        }
        if (IsPressed_EProp.boolValue)
        {
            GUILayout.Label("E",SkillPress);
        }
        else
        {
            GUILayout.Label("E",SkillNoPress);
        }
        if (IsPressed_RProp.boolValue)
        {
            GUILayout.Label("R",SkillPress);
        }
        else
        {
            GUILayout.Label("R",SkillNoPress);
        }
        GUILayout.EndHorizontal();
        base.OnInspectorGUI();
    }
}

}