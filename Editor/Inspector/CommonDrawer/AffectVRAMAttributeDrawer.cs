using UnityEngine;
using UnityEditor;

namespace net.rs64.TexTransTool.Editor
{
    [CustomPropertyDrawer(typeof(AffectVRAMAttribute))]
    internal class AffectVRAMAttributeDrawer : PropertyDrawer
    {
        static TTTProjectConfig s_projectConfig;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            s_projectConfig ??= TTTProjectConfig.instance;
            
            if (s_projectConfig.DisplayVRAMIcon) label.image = TTTImageAssets.VramICon;
            EditorGUI.PropertyField(position, property, label);
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label);
        }
    }
}
