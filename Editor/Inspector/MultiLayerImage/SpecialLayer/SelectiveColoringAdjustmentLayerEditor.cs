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
        private static readonly GUILayoutOption[] EqualWidthOptions = { GUILayout.ExpandWidth(true), GUILayout.MinWidth(0) };

        private static GUIStyle? _centeredLabelStyle;
        private static GUIStyle CenteredLabelStyle => _centeredLabelStyle ??= new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter };

        private static readonly GUILayoutOption[] SliderOptions = { GUILayout.MinWidth(30), GUILayout.ExpandWidth(true) };
        private static readonly GUILayoutOption[] FloatFieldOptions = { GUILayout.Width(38), GUILayout.ExpandWidth(false) };

        private static float DrawCompactSliderWithField(float value, float min = -1f, float max = 1f)
        {
            EditorGUILayout.BeginHorizontal(EqualWidthOptions);
            value = GUILayout.HorizontalSlider(value, min, max, SliderOptions);
            value = Mathf.Clamp(EditorGUILayout.FloatField(value, FloatFieldOptions), min, max);
            EditorGUILayout.EndHorizontal();
            return value;
        }

        protected override void DrawInnerProperties()
        {
            DrawCMYKHeader();
            
            EditorGUILayout.Space();

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

        private void DrawCMYKHeader()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("SelectiveColoringAdjustmentLayer:label:CyansCMYK".Glc(), CenteredLabelStyle, EqualWidthOptions);
            EditorGUILayout.LabelField("SelectiveColoringAdjustmentLayer:label:MagentasCMYK".Glc(), CenteredLabelStyle, EqualWidthOptions);
            EditorGUILayout.LabelField("SelectiveColoringAdjustmentLayer:label:YellowsCMYK".Glc(), CenteredLabelStyle, EqualWidthOptions);
            EditorGUILayout.LabelField("SelectiveColoringAdjustmentLayer:label:BlacksCMYK".Glc(), CenteredLabelStyle, EqualWidthOptions);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawCMYKProperty(GUIContent label, string propertyName)
        {
            var property = serializedObject.FindProperty(propertyName);
            var vector = property.vector4Value;
            
            EditorGUILayout.BeginVertical("helpbox");
            
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            vector.x = DrawCompactSliderWithField(vector.x); // Cyan
            vector.y = DrawCompactSliderWithField(vector.y); // Magenta
            vector.z = DrawCompactSliderWithField(vector.z); // Yellow
            vector.w = DrawCompactSliderWithField(vector.w); // Black
            EditorGUILayout.EndHorizontal();
            
            property.vector4Value = vector;
                        
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(2);
        }
    }
}