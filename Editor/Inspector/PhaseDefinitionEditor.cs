using UnityEngine;
using UnityEditor;
using net.rs64.TexTransTool.Build;
using net.rs64.TexTransTool.Editor;

[CustomEditor(typeof(PhaseDefinition))]
public class PhaseDefinitionEditor : AbstractTexTransGroupEditor
{
    public override void OnInspectorGUI()
    {
        EditorGUILayout.PropertyField(serializedObject.FindProperty("TexTransPhase"));
        base.OnInspectorGUI();
        serializedObject.ApplyModifiedProperties();
    }
}