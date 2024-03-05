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
            if (ITextureFineTuningDrawer.DrawTuningSelector(position, property)) { return; }
            position.y += 18;

            var sSelect = property.FindPropertyRelative("Select");
            var sPropertyNames = property.FindPropertyRelative("PropertyNames");
            var sLinear = property.FindPropertyRelative("Linear");
            EditorGUI.PropertyField(position, sSelect, "TextureFineTuning:prop:Linear".Glc());
            position.y += 18;
            EditorGUI.PropertyField(position, sPropertyNames, "TextureFineTuning:prop:TargetPropertyName".Glc());
            position.y += 18;
            EditorGUI.PropertyField(position, sLinear, "TextureFineTuning:prop:Select".Glc());
            position.y += 18;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label) * 4;
        }
    }
}
