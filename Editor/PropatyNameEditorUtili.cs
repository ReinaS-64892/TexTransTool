#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using Rs64.TexTransTool.ShaderSupport;

namespace Rs64.TexTransTool.Editor
{

    public static class PropatyNameEditor
    {
        static string[] ShadersNames;
        static Dictionary<string, string[]> ShaderNameAndDisplayNames;
        static Dictionary<string, PropertyNameAndDisplayName[]> ShaderNameAndPorertys;


        public static void DrawInspectorGUI(SerializedProperty serializedProperty)
        {
            if (ShaderNameAndPorertys == null || ShadersNames == null || ShaderNameAndDisplayNames == null)
            {
                ShaderNameAndPorertys = new ShaderSupportUtili().GetPropatyNames();
                ShadersNames = ShaderNameAndPorertys.Keys.ToArray();
                ShaderNameAndDisplayNames = new Dictionary<string, string[]>();
                foreach (var item in ShaderNameAndPorertys) { ShaderNameAndDisplayNames.Add(item.Key, item.Value.Select(x => x.DisplayName).ToArray()); }

            }
            var s_Target = serializedProperty;

            var s_propatyName = s_Target.FindPropertyRelative("_propatyName");
            var s_useCustomProperty = s_Target.FindPropertyRelative("_useCustomProperty");
            var s_shaderIndex = s_Target.FindPropertyRelative("_shaderIndex");
            var s_propatyIndex = s_Target.FindPropertyRelative("_propatyIndex");


            EditorGUILayout.BeginHorizontal();
            var rect = EditorGUILayout.GetControlRect();
            var PropWith = rect.width / 4;

            rect.width = PropWith;
            EditorGUI.LabelField(rect, "TargetPropertyName");
            rect.x += rect.width;

            if (s_useCustomProperty.boolValue)
            {
                rect.width = PropWith * 2;
                EditorGUI.PropertyField(rect, s_propatyName, GUIContent.none);
                rect.x += rect.width;
            }
            else
            {
                bool editoFlag = false;

                rect.width = PropWith;
                var editShaderIndex = EditorGUI.Popup(rect, s_shaderIndex.intValue, ShadersNames);
                rect.x += rect.width;
                if (editShaderIndex != s_shaderIndex.intValue)
                {
                    s_shaderIndex.intValue = editShaderIndex;
                    s_propatyIndex.intValue = Mathf.Clamp(s_propatyIndex.intValue, 0, ShaderNameAndPorertys[ShadersNames[s_shaderIndex.intValue]].Length - 1);
                    editoFlag = true;
                }

                var editPropIndex = EditorGUI.Popup(rect, s_propatyIndex.intValue, ShaderNameAndDisplayNames[ShadersNames[s_shaderIndex.intValue]]);
                rect.x += rect.width;
                if (editPropIndex != s_propatyIndex.intValue)
                {
                    s_propatyIndex.intValue = editPropIndex;
                    editoFlag = true;
                }

                if (editoFlag || string.IsNullOrWhiteSpace(s_propatyName.stringValue)) { s_propatyName.stringValue = ShaderNameAndPorertys[ShadersNames[s_shaderIndex.intValue]][s_propatyIndex.intValue].PropertyName; }
            }
            rect.width = PropWith;
            s_useCustomProperty.boolValue = EditorGUI.ToggleLeft(rect, new GUIContent("UseCustomProperty"), s_useCustomProperty.boolValue);

            EditorGUILayout.EndHorizontal();
        }


    }
}
#endif