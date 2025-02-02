using UnityEngine;
using UnityEditor;
using net.rs64.TexTransTool.TextureAtlas.FineTuning;

namespace net.rs64.TexTransTool.TextureAtlas.Editor
{
    [CustomPropertyDrawer(typeof(MergeTexture))]
    internal class MergeTextureTuningDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.height = 18f;

            var sMargeParent = property.FindPropertyRelative("MargeParent");
            var sMargeChildren = property.FindPropertyRelative("MargeChildren");
            EditorGUI.PropertyField(position, sMargeParent, "MargeParent".GlcV());
            position.y += 18;

            position.height = EditorGUI.GetPropertyHeight(sMargeChildren);
            EditorGUI.PropertyField(position, sMargeChildren, "sMargeChildren".GlcV());
            position.y += position.height;
            position.height = 18f;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label) + EditorGUI.GetPropertyHeight(property.FindPropertyRelative("MargeChildren"));
        }
    }
}
