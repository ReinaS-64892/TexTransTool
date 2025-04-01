using UnityEngine;
using UnityEditor;
using net.rs64.TexTransTool.TextureAtlas.FineTuning;
using ColorSpace = net.rs64.TexTransTool.TextureAtlas.FineTuning.ColorSpace;

namespace net.rs64.TexTransTool.TextureAtlas.Editor
{
    [CustomPropertyDrawer(typeof(ColorSpace))]
    internal class ColorSpaceTuningDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.height = 18f;

            var sSelect = property.FindPropertyRelative("Select");
            var sPropertyNames = property.FindPropertyRelative("PropertyNameList");
            var sLinear = property.FindPropertyRelative("AsLinear");
            EditorGUI.PropertyField(position, sLinear, "TextureFineTuning:prop:AsLinear".Glc());
            position.y += 18;

            position.height = EditorGUI.GetPropertyHeight(sPropertyNames);
            EditorGUI.PropertyField(position, sPropertyNames, "TextureFineTuning:prop:TargetPropertyName".Glc());
            position.y += position.height;
            position.height = 18f;

            EditorGUI.PropertyField(position, sSelect, "TextureFineTuning:prop:Select".Glc());
            position.y += 18;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label) * 2 + EditorGUI.GetPropertyHeight(property.FindPropertyRelative("PropertyNameList"));
        }
    }
}
