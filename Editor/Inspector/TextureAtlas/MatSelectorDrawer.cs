using UnityEngine;
using UnityEditor;
using static net.rs64.TexTransTool.TextureAtlas.AtlasTexture;

namespace net.rs64.TexTransTool.TextureAtlas.Editor
{
    [CustomPropertyDrawer(typeof(MatSelector))]
    public class MatSelectorDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var sMat = property.FindPropertyRelative("Material");
            var sOffset = property.FindPropertyRelative("MaterialFineTuningValue");
            position.width /= 2;
            EditorGUI.PropertyField(position, sOffset, new GUIContent(sMat.objectReferenceValue?.name));
            position.x += position.width;
            position.x += 2f; position.width -= 2f;
            EditorGUI.PropertyField(position, sMat, GUIContent.none);
        }
    }
}
