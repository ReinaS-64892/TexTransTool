using System;
using UnityEngine;
using UnityEditor;
using System.Linq;
using net.rs64.TexTransUnityCore.BlendTexture;
using net.rs64.TexTransTool.Utils;

namespace net.rs64.TexTransTool.Editor
{
    [CustomPropertyDrawer(typeof(PowerOfTwoAttribute))]
    internal class PowerOfTwoDrawer : PropertyDrawer
    {
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
            var propLabel = EditorGUI.BeginProperty(rect, label, serializedProperty);

            var value = Array.IndexOf(s_ints, serializedProperty.intValue);
            var modifiedValue = EditorGUI.Popup(rect, propLabel, value, s_strings);
            if (value != modifiedValue && modifiedValue >= 0 && modifiedValue < s_ints.Length) { serializedProperty.intValue = s_ints[modifiedValue]; }

            EditorGUI.EndProperty();
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) { return base.GetPropertyHeight(property, label); }
    }
}
