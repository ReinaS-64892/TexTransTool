using UnityEngine;
using UnityEditor;
using net.rs64.TexTransTool.TextureAtlas.IslandSizePriorityTuner;

namespace net.rs64.TexTransTool.TextureAtlas.Editor
{
    [CustomPropertyDrawer(typeof(SetFromIslandSelector))]
    internal class SetFromIslandSelectorDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.height = 18f;

            var sPriorityValue = property.FindPropertyRelative(nameof(SetFromIslandSelector.PriorityValue));
            var sIslandSelector = property.FindPropertyRelative(nameof(SetFromIslandSelector.IslandSelector));

            EditorGUI.PropertyField(position, sPriorityValue, "IslandSizePriorityTuner:prop:PriorityValue".GlcV());
            position.y += 18;
            EditorGUI.PropertyField(position, sIslandSelector, "IslandSizePriorityTuner:prop:IslandSelector".GlcV());
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label) * 2;
        }


        public static void DrawNow(SerializedProperty sfIs)
        {
            var sPriorityValue = sfIs.FindPropertyRelative(nameof(SetFromIslandSelector.PriorityValue));
            var sIslandSelector = sfIs.FindPropertyRelative(nameof(SetFromIslandSelector.IslandSelector));

            EditorGUILayout.PropertyField(sPriorityValue, "IslandSizePriorityTuner:prop:PriorityValue".GlcV());
            EditorGUILayout.PropertyField(sIslandSelector, "IslandSizePriorityTuner:prop:IslandSelector".GlcV());
        }
    }
}
