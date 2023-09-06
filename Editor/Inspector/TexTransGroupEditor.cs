using UnityEditor;

namespace net.rs64.TexTransTool.Editor
{
    [CustomEditor(typeof(TexTransGroup))]
    public class TexTransGroupEditor : AbstractTexTransGroupEditor
    {
        public override void OnInspectorGUI()
        {
            var TTList = serializedObject.FindProperty("TextureTransformers");
            EditorGUILayout.PropertyField(TTList, true);

            base.OnInspectorGUI();

            serializedObject.ApplyModifiedProperties();
        }
    }
}