using UnityEngine;
using UnityEditor;
using net.rs64.TexTransTool.TextureAtlas.FineTuning;

namespace net.rs64.TexTransTool.TextureAtlas.Editor
{
    [CustomPropertyDrawer(typeof(ReferenceCopy))]
    internal class ReferenceCopyTuningDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.height = 18f;
            if (ITextureFineTuningDrawer.DrawTuningSelector(position, property)) { return; }
            position.y += 18;

            var sSourcePropertyName = property.FindPropertyRelative("SourcePropertyName");
            var sTargetPropertyName = property.FindPropertyRelative("TargetPropertyName");
            EditorGUI.PropertyField(position, sSourcePropertyName, "TextureFineTuning:prop:ReferenceCopy:SourcePropertyName".Glc());
            position.y += 18;
            EditorGUI.PropertyField(position, sTargetPropertyName, "TextureFineTuning:prop:ReferenceCopy:TargetPropertyName".Glc());
            position.y += 18;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label) * 3;
        }
    }
}
