using UnityEngine;
using UnityEditor;
using net.rs64.TexTransTool.TextureAtlas.IslandFineTuner;

namespace net.rs64.TexTransTool.TextureAtlas.Editor
{
    [CustomPropertyDrawer(typeof(SizeOffset))]
    internal class SizeOffsetToIslandDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.height = 18f;
            if (IIslandFineTunerDrawer.DrawTunerSelector(position, property)) { return; }
            position.y += 18;

            var sOffsetValue = property.FindPropertyRelative("OffsetValue");
            EditorGUI.PropertyField(position, sOffsetValue, "IIslandFineTuner:prop:OffsetValue".Glc());
            position.y += 18;

            var sIslandSelector = property.FindPropertyRelative("IslandSelector");
            EditorGUI.PropertyField(position, sIslandSelector, "IIslandFineTuner:prop:IslandSelector".Glc());
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label) * 3;
        }
    }
}