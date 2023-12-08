using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransTool.ShaderSupport;
using net.rs64.TexTransCore.BlendTexture;
using net.rs64.TexTransTool.Utils;

namespace net.rs64.TexTransTool.Editor
{
    [CustomPropertyDrawer(typeof(BlendTypeKeyAttribute))]
    public class BlendTypeKeyDrawer : PropertyDrawer
    {
        static string[] BlendTypeKeys;

        public override void OnGUI(Rect rect, SerializedProperty serializedProperty, GUIContent label)
        {
            DrawBlendModeKey(rect, serializedProperty, label);
        }

        public static void DrawBlendModeKey(Rect rect, SerializedProperty serializedProperty, GUIContent label)
        {
            if (BlendTypeKeys == null)
            {
                BlendTypeKeys = TextureBlend.BlendShaders.Keys.ToArray();
            }

            var s_Target = serializedProperty;

            GUILayout.BeginHorizontal();
            var PropWith = rect.width / 4;

            rect.width = PropWith;
            EditorGUI.LabelField(rect, label);
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

            EditorGUI.PropertyField(rect, s_Target, new GUIContent("O"));


            GUILayout.EndHorizontal();
        }
    }


}