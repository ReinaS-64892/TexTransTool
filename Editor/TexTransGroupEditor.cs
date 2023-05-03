using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

namespace Rs64.TexTransTool.Editor
{
    [CustomEditor(typeof(TexTransGroup), true)]
    public class TexTransGroupEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            TextureTransformerEditor.TextureTransformerEditorDrow(target as TexTransGroup);
        }
    }
}
#endif