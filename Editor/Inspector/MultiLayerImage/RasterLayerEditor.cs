using UnityEngine;
using UnityEditor;
using net.rs64.TexTransTool.MultiLayerImage;
namespace net.rs64.TexTransTool.Editor.MultiLayerImage
{
    [CustomEditor(typeof(RasterLayer))]
    [CanEditMultipleObjects]
    public class RasterLayerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var Target = target as RasterLayer;
            if (targets.Length != 1) { return; }
            EditorGUI.DrawTextureTransparent(EditorGUILayout.GetControlRect(GUILayout.Height(400)), Target.RasterTexture, ScaleMode.ScaleToFit);
        }


    }
}