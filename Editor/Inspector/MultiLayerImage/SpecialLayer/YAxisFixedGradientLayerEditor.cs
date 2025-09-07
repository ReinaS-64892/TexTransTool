#nullable enable

using UnityEditor;
using net.rs64.TexTransTool.MultiLayerImage;

namespace net.rs64.TexTransTool.Editor.MultiLayerImage
{
    [CustomEditor(typeof(YAxisFixedGradientLayer), true)]
    [CanEditMultipleObjects]
    internal class YAxisFixedGradientLayerEditor : AbstractLayerEditor
    {
        protected override void DrawInnerProperties()
        {
            var gradient = serializedObject.FindProperty(nameof(YAxisFixedGradientLayer.Gradient));
            EditorGUILayout.PropertyField(gradient, "YAxisFixedGradientLayer:prop:Gradient".Glc());
        }
    }
}