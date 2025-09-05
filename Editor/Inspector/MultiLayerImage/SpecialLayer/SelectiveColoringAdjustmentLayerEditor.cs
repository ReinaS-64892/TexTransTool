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
            serializedObject.Update();

            EditorGUILayout.LabelField("SelectiveColoringAdjustmentLayer:label:CMYK".Glc(), EditorStyles.boldLabel);

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

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawCMYKProperty(GUIContent label, string propertyName)
        {
            var property = serializedObject.FindProperty(propertyName);
            var vector = property.vector4Value;
            
            // 境界を分かりやすくするためのボックス
            EditorGUILayout.BeginVertical("helpbox");
            
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

            var padding = 5;
                        
            EditorGUILayout.BeginHorizontal();
            vector.x = EditorGUILayout.Slider(vector.x, -1f, 1f);
            EditorGUILayout.Space(padding);
            vector.y = EditorGUILayout.Slider(vector.y, -1f, 1f);
            EditorGUILayout.Space(padding);
            vector.z = EditorGUILayout.Slider(vector.z, -1f, 1f);
            EditorGUILayout.Space(padding);
            vector.w = EditorGUILayout.Slider(vector.w, -1f, 1f);
            EditorGUILayout.EndHorizontal();
            
            property.vector4Value = vector;
                        
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(2);
        }
    }
}