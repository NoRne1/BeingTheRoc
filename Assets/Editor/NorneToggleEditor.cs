using UnityEngine;
using UnityEditor;
using UnityEditor.UI;

[CustomEditor(typeof(NorneToggle))]
public class NorneToggleEditor : ToggleEditor
{
    SerializedProperty toggleType;

    protected override void OnEnable()
    {
        base.OnEnable();
        toggleType = serializedObject.FindProperty("toggleType");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.Update();
        EditorGUILayout.PropertyField(toggleType);
        serializedObject.ApplyModifiedProperties();
    }
}
