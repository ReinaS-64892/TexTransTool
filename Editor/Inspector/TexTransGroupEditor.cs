#if UNITY_EDITOR
using UnityEditor;

namespace net.rs64.TexTransTool.Editor
{
    [CustomEditor(typeof(TexTransGroup))]
    public class TexTransGroupEditor : AbstractTexTransGroupEditor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("TexTransGroupは廃止された機能でTexTransParentGroupを使用してください。", MessageType.Warning);

            var TTList = serializedObject.FindProperty("TextureTransformers");
            EditorGUILayout.PropertyField(TTList, true);

            base.OnInspectorGUI();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif