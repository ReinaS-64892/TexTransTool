#nullable enable

using UnityEditor;
using net.rs64.TexTransTool.MultiLayerImage;
using UnityEngine;

namespace net.rs64.TexTransTool.Editor.MultiLayerImage
{
    [CustomEditor(typeof(SelectiveColoringAdjustmentLayer), true)]
    [CanEditMultipleObjects]
    internal class SelectiveColoringAdjustmentLayerEditor : AbstractLayerEditor
    {
        protected override void DrawInnerProperties()
        {
            DrawCMYKProperty("SelectiveColoringAdjustmentLayer:label:RedsCMYK".Glc(), nameof(SelectiveColoringAdjustmentLayer.RedsCMYK));
            DrawCMYKProperty("SelectiveColoringAdjustmentLayer:label:YellowsCMYK".Glc(), nameof(SelectiveColoringAdjustmentLayer.YellowsCMYK));
            DrawCMYKProperty("SelectiveColoringAdjustmentLayer:label:GreensCMYK".Glc(), nameof(SelectiveColoringAdjustmentLayer.GreensCMYK));
            DrawCMYKProperty("SelectiveColoringAdjustmentLayer:label:CyansCMYK".Glc(), nameof(SelectiveColoringAdjustmentLayer.CyansCMYK));
            DrawCMYKProperty("SelectiveColoringAdjustmentLayer:label:BluesCMYK".Glc(), nameof(SelectiveColoringAdjustmentLayer.BluesCMYK));
            DrawCMYKProperty("SelectiveColoringAdjustmentLayer:label:MagentasCMYK".Glc(), nameof(SelectiveColoringAdjustmentLayer.MagentasCMYK));
            DrawCMYKProperty("SelectiveColoringAdjustmentLayer:label:WhitesCMYK".Glc(), nameof(SelectiveColoringAdjustmentLayer.WhitesCMYK));
            DrawCMYKProperty("SelectiveColoringAdjustmentLayer:label:NeutralsCMYK".Glc(), nameof(SelectiveColoringAdjustmentLayer.NeutralsCMYK));
            DrawCMYKProperty("SelectiveColoringAdjustmentLayer:label:BlacksCMYK".Glc(), nameof(SelectiveColoringAdjustmentLayer.BlacksCMYK));
            
            EditorGUILayout.Space();

            var isAbsolute = serializedObject.FindProperty(nameof(SelectiveColoringAdjustmentLayer.IsAbsolute));
            EditorGUILayout.PropertyField(isAbsolute, "SelectiveColoringAdjustmentLayer:prop:IsAbsolute".Glc());
        }

        private void DrawCMYKProperty(GUIContent label, string propertyName)
        {
            var property = serializedObject.FindProperty(propertyName);
            property.isExpanded = EditorGUILayout.Foldout(property.isExpanded, label);
            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;
                var vector = property.vector4Value;
                vector.x = EditorGUILayout.Slider("SelectiveColoringAdjustmentLayer:label:CyansCMYK".Glc(), vector.x, -1, 1);
                vector.y = EditorGUILayout.Slider("SelectiveColoringAdjustmentLayer:label:MagentasCMYK".Glc(), vector.y, -1, 1);
                vector.z = EditorGUILayout.Slider("SelectiveColoringAdjustmentLayer:label:YellowsCMYK".Glc(), vector.z, -1, 1);
                vector.w = EditorGUILayout.Slider("SelectiveColoringAdjustmentLayer:label:BlacksCMYK".Glc(), vector.w, -1, 1);
                property.vector4Value = vector;
                EditorGUI.indentLevel--;
            }
        }
    }
}