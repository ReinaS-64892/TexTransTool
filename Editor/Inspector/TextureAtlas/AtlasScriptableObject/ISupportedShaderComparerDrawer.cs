using UnityEngine;
using UnityEditor;
using net.rs64.TexTransTool.TextureAtlas.AtlasScriptableObject;
using net.rs64.TexTransTool.Editor;
using System;
namespace net.rs64.TexTransTool.TextureAtlas.Editor
{
    static class ISupportedShaderComparerEditorUtility
    {
        static string[] comparers = new[] { "ContainsName", "ShaderReference" };
        static Func<ISupportedShaderComparer>[] comparerGet = new Func<ISupportedShaderComparer>[] { () => new ContainsName(), () => new ShaderReference() };

        public static bool DrawSelector(Rect rect, SerializedProperty sSupportedShaderComparer)
        {
            var bef = Array.IndexOf(comparers, sSupportedShaderComparer.managedReferenceValue.GetType().Name);
            var aft = EditorGUI.Popup(rect, "Comparer", bef, comparers);
            if (aft != bef) { sSupportedShaderComparer.managedReferenceValue = comparerGet[aft].Invoke(); }
            return aft != bef;
        }
    }
    [CustomPropertyDrawer(typeof(ContainsName))]
    public class ContainsNameDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.height = 18f;

            EditorGUI.BeginProperty(position, label, property);
            if (ISupportedShaderComparerEditorUtility.DrawSelector(position, property)) { return; }
            EditorGUI.EndProperty();

            position.y += position.height;

            EditorGUI.PropertyField(position, property.FindPropertyRelative("Name"));
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label) * 2;
        }
    }

    [CustomPropertyDrawer(typeof(ShaderReference))]
    public class ShaderReferenceDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.height = 18f;

            EditorGUI.BeginProperty(position, label, property);
            if (ISupportedShaderComparerEditorUtility.DrawSelector(position, property)) { return; }
            EditorGUI.EndProperty();

            position.y += position.height;

            EditorGUI.PropertyField(position, property.FindPropertyRelative("Shader"));
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label) * 2;
        }
    }

}
