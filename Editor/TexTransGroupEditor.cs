#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Rs64.TexTransTool.Editor
{
    [CustomEditor(typeof(TexTransGroup), true)]
    public class TexTransGroupEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var thsitarget = target as TexTransGroup;
            EditorGUI.BeginDisabledGroup(thsitarget.IsApply);
            base.OnInspectorGUI();
            EditorGUI.EndDisabledGroup();
            TextureTransformerEditor.TextureTransformerEditorDrow(thsitarget);
        }
    }
}
#endif