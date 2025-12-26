#nullable enable

using UnityEditor;
using net.rs64.TexTransTool.MultiLayerImage;

namespace net.rs64.TexTransTool.Editor.MultiLayerImage
{
    [CustomEditor(typeof(ColorizeLayer), true)]
    [CanEditMultipleObjects]
    internal class ColorizeLayerEditor : AbstractLayerEditor
    {
        protected override void DrawInnerProperties()
        {
            var color = serializedObject.FindProperty(nameof(ColorizeLayer.Color));
            EditorGUILayout.PropertyField(color, "ColorizeLayer:prop:Color".Glc());
        }
    }
}