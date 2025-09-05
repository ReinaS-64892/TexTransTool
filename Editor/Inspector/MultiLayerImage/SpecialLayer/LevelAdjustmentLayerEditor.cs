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

            var currentPosition = position;
            currentPosition.height = EditorGUIUtility.singleLineHeight;

            EditorGUI.LabelField(currentPosition, label, EditorStyles.boldLabel);
            currentPosition.y += EditorGUIUtility.singleLineHeight;

            var inputFloor = property.FindPropertyRelative(nameof(LevelAdjustmentLayer.Level.InputFloor));
            var inputCeiling = property.FindPropertyRelative(nameof(LevelAdjustmentLayer.Level.InputCeiling));
            var gamma = property.FindPropertyRelative(nameof(LevelAdjustmentLayer.Level.Gamma));
            var outputFloor = property.FindPropertyRelative(nameof(LevelAdjustmentLayer.Level.OutputFloor));
            var outputCeiling = property.FindPropertyRelative(nameof(LevelAdjustmentLayer.Level.OutputCeiling));
            
            EditorGUI.indentLevel++;

            EditorGUI.PropertyField(currentPosition, inputFloor, "LevelAdjustmentLayer:prop:InputFloor".Glc());
            currentPosition.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(currentPosition, inputCeiling, "LevelAdjustmentLayer:prop:InputCeiling".Glc());
            currentPosition.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(currentPosition, gamma, "LevelAdjustmentLayer:prop:Gamma".Glc());
            currentPosition.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(currentPosition, outputFloor, "LevelAdjustmentLayer:prop:OutputFloor".Glc());
            currentPosition.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(currentPosition, outputCeiling, "LevelAdjustmentLayer:prop:OutputCeiling".Glc());
            currentPosition.y += EditorGUIUtility.singleLineHeight;
            
            EditorGUI.indentLevel--;

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 6;
        }
    }
}