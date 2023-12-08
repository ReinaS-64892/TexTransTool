using UnityEngine;
using UnityEditor;
using net.rs64.TexTransTool.MultiLayerImage;
namespace net.rs64.TexTransTool.Editor.MultiLayerImage
{
    [CustomEditor(typeof(MultiLayerImageCanvas))]
    internal class MultiLayerImageCanvasEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            TextureTransformerEditor.DrawerWarning("MultiImageLayer".GetLocalize());

            var s_Target = serializedObject;

            TextureBlenderEditor.DrawerRelativeTextureSelector(s_Target.FindProperty("TextureSelector"));
            EditorGUI.BeginDisabledGroup(true);
            var s_TexSize = s_Target.FindProperty("TextureSize");
            EditorGUILayout.PropertyField(s_TexSize, s_TexSize.displayName.GetLC());
            EditorGUI.EndDisabledGroup();

            var Target = target as MultiLayerImageCanvas;
            PreviewContext.instance.DrawApplyAndRevert(Target);

            s_Target.ApplyModifiedProperties();
        }
    }
}