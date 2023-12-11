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
            EditorGUI.DrawTextureTransparent(EditorGUILayout.GetControlRect(GUILayout.Height(400)), thisTarget.RasterTexture, ScaleMode.ScaleToFit);
        }


    }
}