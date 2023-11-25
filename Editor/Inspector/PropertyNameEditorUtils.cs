using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransTool.ShaderSupport;

namespace net.rs64.TexTransTool.Editor
{

    public static class PropertyNameEditor
    {
        static string[] ShadersNames;
        static Dictionary<string, (string[] PropertyName, string[] DisplayName, Dictionary<string, string> Dict)> PropertyNames;
        static string[] Empty = Array.Empty<string>();


        public static void DrawInspectorGUI(SerializedProperty serializedProperty, string Label = null)
        {
            if (ShadersNames == null)
            {
                var getData = new ShaderSupportUtils().GetPropertyNames();
                ShadersNames = getData.Select(i => i.ShaderName).ToArray();
                PropertyNames = getData.ToDictionary(
                    i => i.ShaderName,
                     i => (
                        i.Item2.Select(v => v.PropertyName).ToArray(),
                        i.Item2.Select(v => v.DisplayName).ToArray(),
                        i.Item2.ToDictionary(v => v.PropertyName, v => v.DisplayName)
                        ));
            }
            var s_Target = serializedProperty;

            var s_propertyName = s_Target.FindPropertyRelative("_propertyName");
            var s_useCustomProperty = s_Target.FindPropertyRelative("_useCustomProperty");
            var s_shaderName = s_Target.FindPropertyRelative("_shaderName");




            EditorGUILayout.BeginHorizontal();
            var rect = EditorGUILayout.GetControlRect();
            var PropWith = rect.width / 4;

            rect.width = PropWith;
            EditorGUI.LabelField(rect, Label == null ? "TargetPropertyName".GetLocalize() : Label);
            rect.x += rect.width;

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

                var propertyName = s_propertyName.stringValue;
                var propertyArray = PropertyNames.ContainsKey(shaderName) ? PropertyNames[shaderName].PropertyName : Empty;
                var displayNameArray = PropertyNames.ContainsKey(shaderName) ? PropertyNames[shaderName].DisplayName : Empty;
                var propertySelectIndex = Array.IndexOf(propertyArray, propertyName);
                propertySelectIndex = EditorGUI.Popup(rect, propertySelectIndex, displayNameArray);
                s_propertyName.stringValue = 0 <= propertySelectIndex && propertySelectIndex < propertyArray.Length ? propertyArray[propertySelectIndex] : propertyName;

                rect.x += rect.width;
            }
            rect.width = PropWith;
            s_useCustomProperty.boolValue = EditorGUI.ToggleLeft(rect, new GUIContent("UseCustomProperty".GetLocalize()), s_useCustomProperty.boolValue);

            EditorGUILayout.EndHorizontal();
        }


    }
}