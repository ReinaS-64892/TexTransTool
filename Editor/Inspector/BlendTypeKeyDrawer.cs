using System;
using UnityEngine;
using UnityEditor;
using System.Linq;
using net.rs64.TexTransCore.BlendTexture;
using net.rs64.TexTransTool.Utils;

namespace net.rs64.TexTransTool.Editor
{
    [CustomPropertyDrawer(typeof(BlendTypeKeyAttribute))]
    internal class BlendTypeKeyDrawer : PropertyDrawer
    {
        static string[] s_blendTypeKeys;

        public override void OnGUI(Rect rect, SerializedProperty serializedProperty, GUIContent label)
        {
            DrawBlendModeKey(rect, serializedProperty, label);
        }

        public static void DrawBlendModeKey(Rect rect, SerializedProperty serializedProperty, GUIContent label)
        {
            if (s_blendTypeKeys == null)
            {
                if (TextureBlend.BlendShaders == null) { return; }//次フレームを待つ
                s_blendTypeKeys = TextureBlend.BlendShaders.Keys.ToArray();
            }

            var sTarget = serializedProperty;

            GUILayout.BeginHorizontal();
            var PropWith = rect.width / 4;

            rect.width = PropWith;
            EditorGUI.LabelField(rect, label);
            rect.x += rect.width;

            var enumWidth = PropWith * 3f * 0.9f;
            var strWidth = (PropWith * 3f) - enumWidth;
            rect.width = enumWidth;

            var keyName = sTarget.stringValue;
            var shaderSelectIndex = Array.IndexOf(s_blendTypeKeys, keyName);
            if (sTarget.hasMultipleDifferentValues) { shaderSelectIndex = -1; }
            shaderSelectIndex = EditorGUI.Popup(rect, shaderSelectIndex, s_blendTypeKeys);
            if (0 <= shaderSelectIndex && shaderSelectIndex < s_blendTypeKeys.Length) { sTarget.stringValue = s_blendTypeKeys[shaderSelectIndex]; }

            rect.x += rect.width;
            rect.width = strWidth;

            EditorGUI.PropertyField(rect, sTarget, new GUIContent("O"));


            GUILayout.EndHorizontal();
        }
    }
}
