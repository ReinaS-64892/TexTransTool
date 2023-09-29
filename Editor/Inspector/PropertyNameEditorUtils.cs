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
        static Dictionary<string, string[]> ShaderNameAndDisplayNames;
        static Dictionary<string, PropertyNameAndDisplayName[]> ShaderNameAndProperties;


        public static void DrawInspectorGUI(SerializedProperty serializedProperty, string Label = null)
        {
            if (ShaderNameAndProperties == null || ShadersNames == null || ShaderNameAndDisplayNames == null)
            {
                ShaderNameAndProperties = new ShaderSupportUtils().GetPropertyNames();
                ShadersNames = ShaderNameAndProperties.Keys.ToArray();
                ShaderNameAndDisplayNames = new Dictionary<string, string[]>();
                foreach (var item in ShaderNameAndProperties) { ShaderNameAndDisplayNames.Add(item.Key, item.Value.Select(x => x.DisplayName).ToArray()); }

            }
            var s_Target = serializedProperty;

            var s_propertyName = s_Target.FindPropertyRelative("_propertyName");
            var s_useCustomProperty = s_Target.FindPropertyRelative("_useCustomProperty");
            var s_shaderName = s_Target.FindPropertyRelative("_shaderName");
            var s_propertyIndex = s_Target.FindPropertyRelative("_propertyIndex");


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
                bool editorFlag = false;

                rect.width = PropWith;
                var notEditShaderIndex = Mathf.Clamp(Array.IndexOf(ShadersNames, s_shaderName.stringValue), 0, ShadersNames.Length - 1);
                var editShaderIndex = EditorGUI.Popup(rect, notEditShaderIndex, ShadersNames);
                rect.x += rect.width;
                if (editShaderIndex != notEditShaderIndex)
                {
                    s_shaderName.stringValue = ShadersNames[editShaderIndex];
                    s_propertyIndex.intValue = Mathf.Clamp(s_propertyIndex.intValue, 0, ShaderNameAndProperties[ShadersNames[editShaderIndex]].Length - 1);
                    editorFlag = true;
                }

                var editPropIndex = EditorGUI.Popup(rect, s_propertyIndex.intValue, ShaderNameAndDisplayNames[ShadersNames[editShaderIndex]]);
                rect.x += rect.width;
                if (editPropIndex != s_propertyIndex.intValue)
                {
                    s_propertyIndex.intValue = editPropIndex;
                    editorFlag = true;
                }

                if (editorFlag || string.IsNullOrWhiteSpace(s_propertyName.stringValue)) { s_propertyName.stringValue = ShaderNameAndProperties[ShadersNames[editShaderIndex]][s_propertyIndex.intValue].PropertyName; }
            }
            rect.width = PropWith;
            s_useCustomProperty.boolValue = EditorGUI.ToggleLeft(rect, new GUIContent("UseCustomProperty".GetLocalize()), s_useCustomProperty.boolValue);

            EditorGUILayout.EndHorizontal();
        }


    }
}