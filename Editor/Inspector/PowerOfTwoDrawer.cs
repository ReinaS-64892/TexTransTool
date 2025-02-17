using System;
using UnityEngine;
using UnityEditor;
using net.rs64.TexTransTool.Utils;

namespace net.rs64.TexTransTool.Editor
{
    [CustomPropertyDrawer(typeof(PowerOfTwoAttribute))]
    internal class PowerOfTwoDrawer : PropertyDrawer
    {
        static bool s_inputFieldMode = false;
        static GUIContent[] s_strings = new GUIContent[]
        {
            new ("32"),
            new ("64"),
            new ("128"),
            new ("256"),
            new ("512"),
            new ("1024"),
            new ("2048"),
            new ("4096"),
        };
        static int[] s_ints = new[]{
            32,
            64,
            128,
            256,
            512,
            1024,
            2048,
            4096,
        };
        public override void OnGUI(Rect rect, SerializedProperty serializedProperty, GUIContent label)
        {
            rect.width -= 12f;
            var toggleRect = rect;
            toggleRect.x += toggleRect.width;
            toggleRect.width = 12f;

            var propLabel = EditorGUI.BeginProperty(rect, label, serializedProperty);
            if (s_inputFieldMode is false)
            {
                serializedProperty.intValue = EditorGUI.IntPopup(rect, propLabel, serializedProperty.intValue, s_strings, s_ints);
            }
            else
            {
                serializedProperty.intValue = EditorGUI.IntField(rect, propLabel, serializedProperty.intValue);
                if (Mathf.IsPowerOfTwo(serializedProperty.intValue) is false)
                { serializedProperty.intValue = Mathf.NextPowerOfTwo(serializedProperty.intValue); }
            }
            EditorGUI.EndProperty();
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            s_inputFieldMode = EditorGUI.ToggleLeft(toggleRect, GUIContent.none, s_inputFieldMode);
            EditorGUI.indentLevel = indent;
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) { return base.GetPropertyHeight(property, label); }
    }
}
