using UnityEngine;
using UnityEditor;
using net.rs64.TexTransTool.TextureAtlas.FineTuning;

namespace net.rs64.TexTransTool.TextureAtlas.Editor
{
    [CustomPropertyDrawer(typeof(MipMapRemove))]
    internal class MipMapRemoveTuningDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.height = 18f;

            var sPropertyNames = property.FindPropertyRelative("PropertyNameList");
            var sSelect = property.FindPropertyRelative("Select");
            var sIsRemove = property.FindPropertyRelative("IsRemove");

            EditorGUI.PropertyField(position, sIsRemove, "TextureFineTuning:prop:IsRemove".GlcV());
            position.y += 18;

            position.height = EditorGUI.GetPropertyHeight(sPropertyNames);
            EditorGUI.PropertyField(position, sPropertyNames, "TextureFineTuning:prop:TargetPropertyName".GlcV());
            position.y += position.height;
            position.height = 18f;

            EditorGUI.PropertyField(position, sSelect, "TextureFineTuning:prop:Select".GlcV());
            position.y += 18;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label) * 2 + EditorGUI.GetPropertyHeight(property.FindPropertyRelative("PropertyNameList"));
        }
    }
}
