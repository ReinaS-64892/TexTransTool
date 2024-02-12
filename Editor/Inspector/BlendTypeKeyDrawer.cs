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
        static GUIContent[] s_blendTypeKeyContents;
        static string[] s_blendTypeKeys;

        public override void OnGUI(Rect rect, SerializedProperty serializedProperty, GUIContent label)
        {
            DrawBlendModeKey(rect, serializedProperty, label);
        }

        public static void DrawBlendModeKey(Rect rect, SerializedProperty serializedProperty, GUIContent label)
        {
            if (s_blendTypeKeyContents == null)
            {
                if (TextureBlend.BlendShaders == null) { return; }//次フレームを待つ
                s_blendTypeKeys = TextureBlend.BlendShaders.Keys.ToArray();
                s_blendTypeKeyContents = s_blendTypeKeys.Select(str => new GUIContent(str)).ToArray();
            }

            var sTarget = serializedProperty;
            var propLabel = EditorGUI.BeginProperty(rect, label, serializedProperty);

            var keyName = sTarget.stringValue;
            var shaderSelectIndex = Array.IndexOf(s_blendTypeKeys, keyName);
            if (sTarget.hasMultipleDifferentValues) { shaderSelectIndex = -1; }
            shaderSelectIndex = EditorGUI.Popup(rect, propLabel, shaderSelectIndex, s_blendTypeKeyContents);
            if (0 <= shaderSelectIndex && shaderSelectIndex < s_blendTypeKeyContents.Length) { sTarget.stringValue = s_blendTypeKeyContents[shaderSelectIndex].text; }

            EditorGUI.EndProperty();
        }
    }
}
