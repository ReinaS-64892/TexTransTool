using UnityEditor;
using net.rs64.TexTransTool.Editor;
using net.rs64.TexTransTool;

[CustomEditor(typeof(PhaseDefinition))]
internal class PhaseDefinitionEditor : TexTransGroupEditor
{
    public override void OnInspectorGUI()
    {
        var sTexTransPhase = serializedObject.FindProperty("TexTransPhase");
        EditorGUILayout.PropertyField(sTexTransPhase, sTexTransPhase.name.GetLC());
        base.OnInspectorGUI();
        serializedObject.ApplyModifiedProperties();
    }
}
