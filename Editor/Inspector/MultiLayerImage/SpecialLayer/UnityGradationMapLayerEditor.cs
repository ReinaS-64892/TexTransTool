#nullable enable

using UnityEditor;
using net.rs64.TexTransTool.MultiLayerImage;

namespace net.rs64.TexTransTool.Editor.MultiLayerImage
{
    [CustomEditor(typeof(UnityGradationMapLayer), true)]
    [CanEditMultipleObjects]
    internal class UnityGradationMapLayerEditor : AbstractLayerEditor
    {
        protected override void DrawInnerProperties()
        {
            var gradation = serializedObject.FindProperty(nameof(UnityGradationMapLayer.Gradation));
            EditorGUILayout.PropertyField(gradation, "UnityGradationMapLayer:prop:Gradation".Glc());
        }
    }
}