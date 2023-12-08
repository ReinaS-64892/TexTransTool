using UnityEngine;
using UnityEditor;
using net.rs64.TexTransTool.Build;
using net.rs64.TexTransTool.Editor;
using net.rs64.TexTransTool;

[CustomEditor(typeof(PhaseDefinition))]
internal class PhaseDefinitionEditor : AbstractTexTransGroupEditor
{
    public override void OnInspectorGUI()
    {
        var s_TexTransPhase = serializedObject.FindProperty("TexTransPhase");
        EditorGUILayout.PropertyField(s_TexTransPhase, s_TexTransPhase.name.GetLC());
        base.OnInspectorGUI();
        serializedObject.ApplyModifiedProperties();
    }
}