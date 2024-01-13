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

            var sTarget = serializedObject;

            TextureBlenderEditor.DrawerRelativeTextureSelector(sTarget.FindProperty("TextureSelector"));
            EditorGUI.BeginDisabledGroup(true);
            var sTexSize = sTarget.FindProperty("TextureSize");
            EditorGUILayout.PropertyField(sTexSize, sTexSize.displayName.GetLC());
            EditorGUI.EndDisabledGroup();

            var thisTarget = target as MultiLayerImageCanvas;
            PreviewContext.instance.DrawApplyAndRevert(thisTarget);

            sTarget.ApplyModifiedProperties();
        }
    }
}
