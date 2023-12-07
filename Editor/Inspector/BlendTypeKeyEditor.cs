using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransTool.ShaderSupport;
using net.rs64.TexTransCore.BlendTexture;

namespace net.rs64.TexTransTool.Editor
{

    public static class BlendTypeKeyEditor
    {
        static string[] BlendTypeKeys;

        public static void DrawInspectorGUI(SerializedProperty serializedProperty, string Label = null)
        {
            if (BlendTypeKeys == null)
            {
                BlendTypeKeys = TextureBlend.BlendShaders.Keys.ToArray();
            }
            var s_Target = serializedProperty;

            EditorGUILayout.BeginHorizontal();
            var rect = EditorGUILayout.GetControlRect();
            var PropWith = rect.width / 4;

            rect.width = PropWith;
            EditorGUI.LabelField(rect, Label == null ? "TargetPropertyName".GetLocalize() : Label);
            rect.x += rect.width;

            var enumWidth = PropWith * 3f * 0.9f;
            var strWidth = (PropWith * 3f) - enumWidth;
            rect.width = enumWidth;

            var keyName = s_Target.stringValue;
            var shaderSelectIndex = Array.IndexOf(BlendTypeKeys, keyName);
            shaderSelectIndex = EditorGUI.Popup(rect, shaderSelectIndex, BlendTypeKeys);
            s_Target.stringValue = 0 <= shaderSelectIndex && shaderSelectIndex < BlendTypeKeys.Length ? BlendTypeKeys[shaderSelectIndex] : keyName;

            rect.x += rect.width;
            rect.width = strWidth;

            EditorGUI.PropertyField(rect, s_Target,new GUIContent("O"));


            EditorGUILayout.EndHorizontal();
        }


    }
}