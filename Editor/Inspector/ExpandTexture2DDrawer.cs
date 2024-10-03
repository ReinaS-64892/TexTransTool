using System;
using UnityEngine;
using UnityEditor;
using System.Linq;
using net.rs64.TexTransUnityCore.BlendTexture;
using net.rs64.TexTransTool.Utils;

namespace net.rs64.TexTransTool.Editor
{
    [CustomPropertyDrawer(typeof(ExpandTexture2DAttribute))]
    internal class ExpandTexture2DDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect rect, SerializedProperty serializedProperty, GUIContent label)
        {
            var propLabel = EditorGUI.BeginProperty(rect, label, serializedProperty);

            if (serializedProperty.hasMultipleDifferentValues) { EditorGUI.ObjectField(rect, serializedProperty, propLabel); }
            else { serializedProperty.objectReferenceValue = EditorGUI.ObjectField(rect, propLabel, serializedProperty.objectReferenceValue, typeof(Texture2D), true); }

            EditorGUI.EndProperty();
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return property.hasMultipleDifferentValues ? 18f : 64f;
        }
    }
}
