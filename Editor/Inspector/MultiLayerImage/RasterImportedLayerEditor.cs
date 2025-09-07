#nullable enable

using UnityEditor;
using net.rs64.TexTransTool.MultiLayerImage;

namespace net.rs64.TexTransTool.Editor.MultiLayerImage
{
    [CustomEditor(typeof(RasterImportedLayer), true)]
    [CanEditMultipleObjects]
    internal class RasterImportedLayerEditor : AbstractLayerEditor
    {
        protected override void DrawInnerProperties()
        {
            var importedImage = serializedObject.FindProperty(nameof(RasterImportedLayer.ImportedImage));
            EditorGUILayout.PropertyField(importedImage, "RasterImportedLayer:prop:ImportedImage".Glc());
        }
    }
}