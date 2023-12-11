using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransTool.ShaderSupport;

namespace net.rs64.TexTransTool.Editor
{
    internal static class PropertyNameEditor
    {
        static string[] s_shadersNames;
        static Dictionary<string, (string[] PropertyName, string[] DisplayName)> s_propertyNames;
        static string[] s_empty = Array.Empty<string>();


        public static void DrawInspectorGUI(SerializedProperty serializedProperty, string label = null)
        {
            if (s_shadersNames == null)
            {
                var getData = new ShaderSupportUtils().GetPropertyNames();
                s_shadersNames = getData.Select(i => i.ShaderName).ToArray();
                s_propertyNames = getData.ToDictionary(
                    i => i.ShaderName,
                    i => (i.Item2.Select(v => v.PropertyName).ToArray(), i.Item2.Select(v => v.DisplayName).ToArray()));
            }
            var sTarget = serializedProperty;

            var sPropertyName = sTarget.FindPropertyRelative("_propertyName");
            var sUseCustomProperty = sTarget.FindPropertyRelative("_useCustomProperty");
            var sShaderName = sTarget.FindPropertyRelative("_shaderName");


            var rect = EditorGUILayout.GetControlRect();
            var PropWith = rect.width / 4;

            rect.width = PropWith;
            EditorGUI.LabelField(rect, label == null ? "TargetPropertyName".GetLocalize() : label);
            rect.x += rect.width;

            var preIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            if (sUseCustomProperty.boolValue)
            {
                rect.width = PropWith * 2;
                EditorGUI.PropertyField(rect, sPropertyName, GUIContent.none);
                rect.x += rect.width;
            }
            else
            {
                rect.width = PropWith;

                var shaderName = sShaderName.stringValue;
                var shaderSelectIndex = Array.IndexOf(s_shadersNames, shaderName);
                if (sShaderName.hasMultipleDifferentValues) { shaderSelectIndex = -1; }
                shaderSelectIndex = EditorGUI.Popup(rect, shaderSelectIndex, s_shadersNames);
                if (0 <= shaderSelectIndex && shaderSelectIndex < s_shadersNames.Length) { sShaderName.stringValue = s_shadersNames[shaderSelectIndex]; }

                rect.x += rect.width;

                rect.width = 15;
                EditorGUI.indentLevel = 1;
                EditorGUI.PropertyField(rect, sShaderName, new GUIContent(""));
                EditorGUI.indentLevel = 0;
                rect.x += 10f;

                rect.width = PropWith;

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

                    rect.width = 15;
                    EditorGUI.indentLevel = 1;
                    EditorGUI.PropertyField(rect, sPropertyName, new GUIContent(""));
                    EditorGUI.indentLevel = 0;
                    rect.x += 10f;

                }
                else
                {
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUI.PropertyField(rect, sPropertyName, GUIContent.none);
                    EditorGUI.EndDisabledGroup();
                    rect.x += rect.width;

                }
            }
            rect.x += 5f;
            rect.width = 30f;
            EditorGUI.PropertyField(rect, sUseCustomProperty, GUIContent.none);
            rect.x += 15f;
            rect.width = PropWith - rect.width;
            EditorGUI.LabelField(rect, new GUIContent("UseCustomProperty".GetLocalize()));


            EditorGUI.indentLevel = preIndent;
        }


    }
}