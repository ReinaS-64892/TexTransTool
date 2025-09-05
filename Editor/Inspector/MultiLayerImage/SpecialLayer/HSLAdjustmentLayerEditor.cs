#nullable enable

using UnityEditor;
using net.rs64.TexTransTool.MultiLayerImage;

namespace net.rs64.TexTransTool.Editor.MultiLayerImage
{
    [CustomEditor(typeof(HSLAdjustmentLayer), true)]
    [CanEditMultipleObjects]
    internal class HSLAdjustmentLayerEditor : AbstractLayerEditor
    {
        protected override void DrawInnerProperties()
        {
            var hue = serializedObject.FindProperty(nameof(HSLAdjustmentLayer.Hue));
            EditorGUILayout.PropertyField(hue, "HSLAdjustmentLayer:prop:Hue".Glc());
            var saturation = serializedObject.FindProperty(nameof(HSLAdjustmentLayer.Saturation));
            EditorGUILayout.PropertyField(saturation, "HSLAdjustmentLayer:prop:Saturation".Glc());
            var lightness = serializedObject.FindProperty(nameof(HSLAdjustmentLayer.Lightness));
            EditorGUILayout.PropertyField(lightness, "HSLAdjustmentLayer:prop:Lightness".Glc());
        }
    }
}