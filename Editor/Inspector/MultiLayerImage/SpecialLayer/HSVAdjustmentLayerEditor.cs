#nullable enable

using UnityEditor;
using net.rs64.TexTransTool.MultiLayerImage;

namespace net.rs64.TexTransTool.Editor.MultiLayerImage
{
    [CustomEditor(typeof(HSVAdjustmentLayer), true)]
    [CanEditMultipleObjects]
    internal class HSVAdjustmentLayerEditor : AbstractLayerEditor
    {
        protected override void DrawInnerProperties()
        {
            var hue = serializedObject.FindProperty(nameof(HSVAdjustmentLayer.Hue));
            EditorGUILayout.PropertyField(hue, "HSVAdjustmentLayer:prop:Hue".Glc());
            var saturation = serializedObject.FindProperty(nameof(HSVAdjustmentLayer.Saturation));
            EditorGUILayout.PropertyField(saturation, "HSVAdjustmentLayer:prop:Saturation".Glc());
            var value = serializedObject.FindProperty(nameof(HSVAdjustmentLayer.Value));
            EditorGUILayout.PropertyField(value, "HSVAdjustmentLayer:prop:Value".Glc());
        }
    }
}