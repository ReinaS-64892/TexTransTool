using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransTool.ShaderSupport;

namespace net.rs64.TexTransTool.Editor
{
    [CustomPropertyDrawer(typeof(PropertyName))]
    internal class PropertyNameEditor : PropertyDrawer
    {
        static string[] s_shadersNames;
        static Dictionary<string, (string[] PropertyName, string[] DisplayName)> s_propertyNames;
        static string[] s_empty = Array.Empty<string>();


        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {

            if (s_shadersNames == null)
            {
                var getData = new ShaderSupportUtils().GetPropertyNames();
                s_shadersNames = getData.Select(i => i.ShaderName).ToArray();
                s_propertyNames = getData.ToDictionary(
                    i => i.ShaderName,
                    i => (i.Item2.Select(v => v.PropertyName).ToArray(), i.Item2.Select(v => v.DisplayName).ToArray()));
            }
            var sTarget = property;

            var sPropertyName = sTarget.FindPropertyRelative("_propertyName");
            var sUseCustomProperty = sTarget.FindPropertyRelative("_useCustomProperty");
            var sShaderName = sTarget.FindPropertyRelative("_shaderName");

            var topLabel = label ?? "TargetPropertyName".Glc();

            var rect = position;
            var PropWith = rect.width / 4;


            var preIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            if (sUseCustomProperty.boolValue)
            {
                rect.width = PropWith * 3;
                EditorGUI.PropertyField(rect, sPropertyName, topLabel);
                rect.x += rect.width;
            }
            else
            {
                rect.width = PropWith;
                EditorGUI.LabelField(rect, topLabel);
                rect.x += rect.width;

                EditorGUI.BeginProperty(rect, GUIContent.none, sShaderName);

                var shaderName = sShaderName.stringValue;
                var shaderSelectIndex = Array.IndexOf(s_shadersNames, shaderName);
                if (sShaderName.hasMultipleDifferentValues) { shaderSelectIndex = -1; }
                shaderSelectIndex = EditorGUI.Popup(rect, shaderSelectIndex, s_shadersNames);
                if (0 <= shaderSelectIndex && shaderSelectIndex < s_shadersNames.Length) { sShaderName.stringValue = s_shadersNames[shaderSelectIndex]; }

                EditorGUI.EndProperty();

                rect.x += rect.width;

                EditorGUI.BeginProperty(rect, GUIContent.none, sPropertyName);

                if (shaderSelectIndex != -1)
                {
                    var propertyName = sPropertyName.stringValue;
                    var propertyArray = s_propertyNames.ContainsKey(shaderName) ? s_propertyNames[shaderName].PropertyName : s_empty;
                    var displayNameArray = s_propertyNames.ContainsKey(shaderName) ? s_propertyNames[shaderName].DisplayName : s_empty;
                    var propertySelectIndex = Array.IndexOf(propertyArray, propertyName);
                    if (sPropertyName.hasMultipleDifferentValues) { propertySelectIndex = -1; }
                    propertySelectIndex = EditorGUI.Popup(rect, propertySelectIndex, displayNameArray);
                    if (0 <= propertySelectIndex && propertySelectIndex < propertyArray.Length) { sPropertyName.stringValue = propertyArray[propertySelectIndex]; }
                    rect.x += rect.width;
                }
                else
                {
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUI.PropertyField(rect, sPropertyName, GUIContent.none);
                    EditorGUI.EndDisabledGroup();

                    rect.x += rect.width;
                }

                EditorGUI.EndProperty();
            }

            rect.width = PropWith;
            EditorGUI.BeginProperty(rect, GUIContent.none, sUseCustomProperty);
            sUseCustomProperty.boolValue = EditorGUI.ToggleLeft(rect, "UseCustomProperty".GetLocalize(), sUseCustomProperty.boolValue);
            EditorGUI.EndProperty();

            EditorGUI.indentLevel = preIndent;
        }
    }
}
