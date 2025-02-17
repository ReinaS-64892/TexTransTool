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

            var sSourcePropertyName = property.FindPropertyRelative("SourcePropertyName");
            var sTargetPropertyName = property.FindPropertyRelative("TargetPropertyNameList");
            EditorGUI.PropertyField(position, sSourcePropertyName, "TextureFineTuning:prop:ReferenceCopy:SourcePropertyName".GlcV());
            position.y += 18;

            position.height = EditorGUI.GetPropertyHeight(sTargetPropertyName);
            EditorGUI.PropertyField(position, sTargetPropertyName, "TextureFineTuning:prop:ReferenceCopy:TargetPropertyName".GlcV());
            position.y += position.height;
            position.height = 18f;

        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label) + EditorGUI.GetPropertyHeight(property.FindPropertyRelative("TargetPropertyNameList"));
        }
    }
}
