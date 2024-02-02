using UnityEngine;
using UnityEditor;
using net.rs64.TexTransTool.MultiLayerImage;
namespace net.rs64.TexTransTool.Editor.MultiLayerImage
{
    [CustomEditor(typeof(RasterLayer))]
    [CanEditMultipleObjects]
    internal class RasterLayerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            TextureTransformerEditor.DrawerWarning("MultiImageLayer".GetLocalize());
            base.OnInspectorGUI();

            var thisTarget = target as RasterLayer;
            if (targets.Length != 1) { return; }
            if (thisTarget.RasterTexture != null) { EditorGUI.DrawTextureTransparent(EditorGUILayout.GetControlRect(GUILayout.Height(400)), thisTarget.RasterTexture, ScaleMode.ScaleToFit); }
        }


    }
    [CustomEditor(typeof(RasterImportedLayer))]
    [CanEditMultipleObjects]
    internal class RasterImportedLayerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            TextureTransformerEditor.DrawerWarning("MultiImageLayer".GetLocalize());
            base.OnInspectorGUI();

            var thisTarget = target as RasterImportedLayer;
            if (targets.Length != 1) { return; }
            if (thisTarget.ImportedImage != null && thisTarget.ImportedImage.PreviewTexture != null)
            {
                EditorGUI.DrawTextureTransparent(EditorGUILayout.GetControlRect(GUILayout.Height(400)), thisTarget.ImportedImage.PreviewTexture, ScaleMode.ScaleToFit);
            }
        }


    }
}
