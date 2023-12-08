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
        static string[] ShadersNames;
        static Dictionary<string, (string[] PropertyName, string[] DisplayName)> PropertyNames;
        static string[] Empty = Array.Empty<string>();


        public static void DrawInspectorGUI(SerializedProperty serializedProperty, string Label = null)
        {
            if (ShadersNames == null)
            {
                var getData = new ShaderSupportUtils().GetPropertyNames();
                ShadersNames = getData.Select(i => i.ShaderName).ToArray();
                PropertyNames = getData.ToDictionary(
                    i => i.ShaderName,
                    i => (i.Item2.Select(v => v.PropertyName).ToArray(), i.Item2.Select(v => v.DisplayName).ToArray()));
            }
            var s_Target = serializedProperty;

            var s_propertyName = s_Target.FindPropertyRelative("_propertyName");
            var s_useCustomProperty = s_Target.FindPropertyRelative("_useCustomProperty");
            var s_shaderName = s_Target.FindPropertyRelative("_shaderName");


            var rect = EditorGUILayout.GetControlRect();
            var PropWith = rect.width / 4;

            rect.width = PropWith;
            EditorGUI.LabelField(rect, Label == null ? "TargetPropertyName".GetLocalize() : Label);
            rect.x += rect.width;

            var preIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            if (s_useCustomProperty.boolValue)
            {
                rect.width = PropWith * 2;
                EditorGUI.PropertyField(rect, s_propertyName, GUIContent.none);
                rect.x += rect.width;
            }
            else
            {
                rect.width = PropWith;

                var shaderName = s_shaderName.stringValue;
                var shaderSelectIndex = Array.IndexOf(ShadersNames, shaderName);
                shaderSelectIndex = EditorGUI.Popup(rect, shaderSelectIndex, ShadersNames);
                s_shaderName.stringValue = 0 <= shaderSelectIndex && shaderSelectIndex < ShadersNames.Length ? ShadersNames[shaderSelectIndex] : shaderName;

                rect.x += rect.width;

                rect.width = 15;
                EditorGUI.indentLevel = 1;
                EditorGUI.PropertyField(rect, s_shaderName, new GUIContent(""));
                EditorGUI.indentLevel = 0;
                rect.x += 10f;

                rect.width = PropWith;

                if (shaderSelectIndex != -1)
                {

                    var propertyName = s_propertyName.stringValue;
                    var propertyArray = PropertyNames.ContainsKey(shaderName) ? PropertyNames[shaderName].PropertyName : Empty;
                    var displayNameArray = PropertyNames.ContainsKey(shaderName) ? PropertyNames[shaderName].DisplayName : Empty;
                    var propertySelectIndex = Array.IndexOf(propertyArray, propertyName);
                    propertySelectIndex = EditorGUI.Popup(rect, propertySelectIndex, displayNameArray);
                    s_propertyName.stringValue = 0 <= propertySelectIndex && propertySelectIndex < propertyArray.Length ? propertyArray[propertySelectIndex] : propertyName;

                    rect.x += rect.width;

                    rect.width = 15;
                    EditorGUI.indentLevel = 1;
                    EditorGUI.PropertyField(rect, s_propertyName, new GUIContent(""));
                    EditorGUI.indentLevel = 0;
                    rect.x += 10f;

                }
                else
                {
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUI.PropertyField(rect, s_propertyName, GUIContent.none);
                    EditorGUI.EndDisabledGroup();
                    rect.x += rect.width;

                }
            }
            rect.x += 5f;
            rect.width = 30f;
            EditorGUI.PropertyField(rect, s_useCustomProperty, GUIContent.none);
            rect.x += 15f;
            rect.width = PropWith - rect.width;
            EditorGUI.LabelField(rect, new GUIContent("UseCustomProperty".GetLocalize()));


            EditorGUI.indentLevel = preIndent;
        }


    }
}