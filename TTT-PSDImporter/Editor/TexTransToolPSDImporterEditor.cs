using UnityEditor;
using UnityEditor.AssetImporters;

namespace net.rs64.TexTransTool.PSDImporter
{
    [CustomEditor(typeof(TexTransToolPSDImporter))]
    class TexTransToolPSDImporterEditor : ScriptedImporterEditor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("TexTransToolPSDImporter" + " " + "Common:ExperimentalWarning".GetLocalize(), MessageType.Warning);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ImportMode"));
            base.ApplyRevertGUI();
        }
        public override bool HasPreviewGUI() { return false; }
    }







}
