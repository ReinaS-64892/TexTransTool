#nullable enable

using UnityEditor;
using net.rs64.TexTransTool.MultiLayerImage;
using UnityEngine;

namespace net.rs64.TexTransTool.Editor.MultiLayerImage
{
    [CustomEditor(typeof(LevelAdjustmentLayer), true)]
    [CanEditMultipleObjects]
    internal class LevelAdjustmentLayerEditor : AbstractLayerEditor
    {
        protected override void DrawInnerProperties()
        {
            var rgb = serializedObject.FindProperty(nameof(LevelAdjustmentLayer.RGB));
            EditorGUILayout.PropertyField(rgb, "LevelAdjustmentLayer:prop:RGB".Glc());
            var red = serializedObject.FindProperty(nameof(LevelAdjustmentLayer.Red));
            EditorGUILayout.PropertyField(red, "LevelAdjustmentLayer:prop:Red".Glc());
            var green = serializedObject.FindProperty(nameof(LevelAdjustmentLayer.Green));
            EditorGUILayout.PropertyField(green, "LevelAdjustmentLayer:prop:Green".Glc());
            var blue = serializedObject.FindProperty(nameof(LevelAdjustmentLayer.Blue));
            EditorGUILayout.PropertyField(blue, "LevelAdjustmentLayer:prop:Blue".Glc());
        }
    }

    [CustomPropertyDrawer(typeof(LevelAdjustmentLayer.Level))]
    internal class LevelDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            position.height = EditorGUIUtility.singleLineHeight;
            property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, label);
            position.y += EditorGUIUtility.singleLineHeight;

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;

                var inputFloor = property.FindPropertyRelative(nameof(LevelAdjustmentLayer.Level.InputFloor));
                var inputCeiling = property.FindPropertyRelative(nameof(LevelAdjustmentLayer.Level.InputCeiling));
                var gamma = property.FindPropertyRelative(nameof(LevelAdjustmentLayer.Level.Gamma));
                var outputFloor = property.FindPropertyRelative(nameof(LevelAdjustmentLayer.Level.OutputFloor));
                var outputCeiling = property.FindPropertyRelative(nameof(LevelAdjustmentLayer.Level.OutputCeiling));
                
                EditorGUI.PropertyField(position, inputFloor, "LevelAdjustmentLayer:prop:InputFloor".Glc());
                position.y += EditorGUIUtility.singleLineHeight;
                EditorGUI.PropertyField(position, inputCeiling, "LevelAdjustmentLayer:prop:InputCeiling".Glc());
                position.y += EditorGUIUtility.singleLineHeight;
                EditorGUI.PropertyField(position, gamma, "LevelAdjustmentLayer:prop:Gamma".Glc());
                position.y += EditorGUIUtility.singleLineHeight;
                EditorGUI.PropertyField(position, outputFloor, "LevelAdjustmentLayer:prop:OutputFloor".Glc());
                position.y += EditorGUIUtility.singleLineHeight;
                EditorGUI.PropertyField(position, outputCeiling, "LevelAdjustmentLayer:prop:OutputCeiling".Glc());
                position.y += EditorGUIUtility.singleLineHeight;
                
                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return property.isExpanded ? EditorGUIUtility.singleLineHeight * 6 : EditorGUIUtility.singleLineHeight;
        }
    }
}