using System;
using UnityEngine;
using UnityEditor;
using System.Linq;
using net.rs64.TexTransCore.BlendTexture;
using net.rs64.TexTransTool.Utils;

namespace net.rs64.TexTransTool.Editor
{
    [CustomPropertyDrawer(typeof(PowerOfTwoAttribute))]
    internal class PowerOfTwoDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect rect, SerializedProperty serializedProperty, GUIContent label)
        {
            var propLabel = EditorGUI.BeginProperty(rect, label, serializedProperty);

            var value = serializedProperty.intValue;
            var modifiedValue = EditorGUI.IntField(rect, propLabel, value);
            if (value != modifiedValue) { serializedProperty.intValue = Mathf.IsPowerOfTwo(modifiedValue) ? modifiedValue : Mathf.NextPowerOfTwo(modifiedValue); }

            EditorGUI.EndProperty();
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) { return base.GetPropertyHeight(property, label); }
    }
}
