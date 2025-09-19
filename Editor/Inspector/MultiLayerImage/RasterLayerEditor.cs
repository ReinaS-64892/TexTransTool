#nullable enable

using UnityEditor;
using net.rs64.TexTransTool.MultiLayerImage;

namespace net.rs64.TexTransTool.Editor.MultiLayerImage
{
    [CustomEditor(typeof(RasterLayer), true)]
    [CanEditMultipleObjects]
    internal class RasterLayerEditor : AbstractLayerEditor
    {
        protected override void DrawInnerProperties()
        {
            var importedImage = serializedObject.FindProperty(nameof(RasterLayer.RasterTexture));
            EditorGUILayout.PropertyField(importedImage, "RasterLayer:prop:RasterTexture".Glc());
        }
    }
}