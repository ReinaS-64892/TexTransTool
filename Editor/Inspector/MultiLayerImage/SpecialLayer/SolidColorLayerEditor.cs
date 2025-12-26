#nullable enable

using UnityEditor;
using net.rs64.TexTransTool.MultiLayerImage;

namespace net.rs64.TexTransTool.Editor.MultiLayerImage
{
    [CustomEditor(typeof(SolidColorLayer), true)]
    [CanEditMultipleObjects]
    internal class SolidColorLayerEditor : AbstractLayerEditor
    {
        protected override void DrawInnerProperties()
        {
            var color = serializedObject.FindProperty(nameof(SolidColorLayer.Color));
            EditorGUILayout.PropertyField(color, "SolidColorLayer:prop:Color".Glc());
        }
    }
}