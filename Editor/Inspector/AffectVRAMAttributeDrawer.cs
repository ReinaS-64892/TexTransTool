using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

namespace net.rs64.TexTransTool.Editor
{
    [CustomPropertyDrawer(typeof(AffectVRAMAttribute))]
    internal class AffectVRAMAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label.image = TTTImageAssets.VramICon;
            EditorGUI.PropertyField(position, property, label);
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label);
        }
    }
}
