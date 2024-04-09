using UnityEngine;
using UnityEditor;
using net.rs64.TexTransTool.TextureAtlas.AtlasScriptableObject;
using net.rs64.TexTransTool.Editor;
using System;
namespace net.rs64.TexTransTool.TextureAtlas.Editor
{
    [CustomPropertyDrawer(typeof(IAtlasDefineConstraints))]
    public class IAtlasDefineConstraintsDrawer : PropertyDrawer
    {
        static string[] comparers = new[] {
            "AndConstraints",
            "OrConstraints",
            "FloatPropertyValueGreater",
            "FloatPropertyValueEqual",
            "IntPropertyValueGreater",
            "IntPropertyValueEqual",
            "ShaderNameContains",
            "Anything"
            };
        static Func<IAtlasDefineConstraints>[] comparerGet = new Func<IAtlasDefineConstraints>[] {
            () => new AndConstraints(),
            () => new OrConstraints(),
            () => new FloatPropertyValueGreater(),
            () => new FloatPropertyValueEqual(),
            () => new IntPropertyValueGreater(),
            () => new IntPropertyValueEqual(),
            () => new ShaderNameContains(),
            () => new Anything()
            };

        public static bool DrawSelector(Rect rect, SerializedProperty sSupportedShaderComparer)
        {
            var bef = Array.IndexOf(comparers, sSupportedShaderComparer.managedReferenceValue?.GetType()?.Name);
            var aft = EditorGUI.Popup(rect, "Comparer", bef, comparers);
            if (aft != bef) { sSupportedShaderComparer.managedReferenceValue = comparerGet[aft].Invoke(); }
            return aft != bef;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            DrawSelector(position, property);
        }
    }
    [CustomPropertyDrawer(typeof(AndConstraints))]
    public class AndConstraintsDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var listSize = position.height - 18f;
            position.height = 18f;
            EditorGUI.BeginProperty(position, label, property);
            if (IAtlasDefineConstraintsDrawer.DrawSelector(position, property)) { return; }
            EditorGUI.EndProperty();
            EditorGUI.indentLevel += 1;
            position.y += position.height;
            position.height = listSize;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("AtlasDefineConstraints"));
            EditorGUI.indentLevel += -1;

        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label) + EditorGUI.GetPropertyHeight(property.FindPropertyRelative("AtlasDefineConstraints"));
        }
    }
    [CustomPropertyDrawer(typeof(OrConstraints))]
    public class OrConstraintsDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var listSize = position.height - 18f;
            position.height = 18f;
            EditorGUI.BeginProperty(position, label, property);
            if (IAtlasDefineConstraintsDrawer.DrawSelector(position, property)) { return; }
            EditorGUI.EndProperty();
            EditorGUI.indentLevel += 1;
            position.y += position.height;
            position.height = listSize;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("AtlasDefineConstraints"));
            EditorGUI.indentLevel += -1;

        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label) + EditorGUI.GetPropertyHeight(property.FindPropertyRelative("AtlasDefineConstraints"));
        }
    }
    [CustomPropertyDrawer(typeof(FloatPropertyValueGreater))]
    public class FloatPropertyValueGreaterDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.height = 18f;
            EditorGUI.BeginProperty(position, label, property);
            if (IAtlasDefineConstraintsDrawer.DrawSelector(position, property)) { return; }
            EditorGUI.EndProperty();
            EditorGUI.indentLevel += 1;
            position.y += position.height;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("PropertyName"));
            position.y += position.height;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("ComparerValue"));
            position.y += position.height;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("Less"));
            EditorGUI.indentLevel += -1;
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label) * 4f;
        }
    }
    [CustomPropertyDrawer(typeof(FloatPropertyValueEqual))]
    public class FloatPropertyValueEqualDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.height = 18f;
            EditorGUI.BeginProperty(position, label, property);
            if (IAtlasDefineConstraintsDrawer.DrawSelector(position, property)) { return; }
            EditorGUI.EndProperty();
            EditorGUI.indentLevel += 1;
            position.y += position.height;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("PropertyName"));
            position.y += position.height;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("Value"));
            position.y += position.height;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("NotEqual"));
            EditorGUI.indentLevel += -1;

        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label) * 4f;
        }
    }
    [CustomPropertyDrawer(typeof(IntPropertyValueGreater))]
    public class IntPropertyValueGreaterDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.height = 18f;
            EditorGUI.BeginProperty(position, label, property);
            if (IAtlasDefineConstraintsDrawer.DrawSelector(position, property)) { return; }
            EditorGUI.EndProperty();
            EditorGUI.indentLevel += 1;
            position.y += position.height;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("PropertyName"));
            position.y += position.height;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("ComparerValue"));
            position.y += position.height;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("Less"));
            EditorGUI.indentLevel += -1;
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label) * 4f;
        }
    }
    [CustomPropertyDrawer(typeof(IntPropertyValueEqual))]
    public class IntPropertyValueEqualDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.height = 18f;
            EditorGUI.BeginProperty(position, label, property);
            if (IAtlasDefineConstraintsDrawer.DrawSelector(position, property)) { return; }
            EditorGUI.EndProperty();
            EditorGUI.indentLevel += 1;
            position.y += position.height;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("PropertyName"));
            position.y += position.height;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("Value"));
            position.y += position.height;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("NotEqual"));
            EditorGUI.indentLevel += -1;

        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label) * 4f;
        }
    }
    [CustomPropertyDrawer(typeof(ShaderNameContains))]
    public class ShaderNameContainsDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.height = 18f;
            EditorGUI.BeginProperty(position, label, property);
            if (IAtlasDefineConstraintsDrawer.DrawSelector(position, property)) { return; }
            EditorGUI.EndProperty();
            EditorGUI.indentLevel += 1;
            position.y += position.height;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("Name"));
            EditorGUI.indentLevel += -1;

        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label) * 2f;
        }
    }

    [CustomPropertyDrawer(typeof(Anything))]
    public class AnythingDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.height = 18f;
            EditorGUI.BeginProperty(position, label, property);
            if (IAtlasDefineConstraintsDrawer.DrawSelector(position, property)) { return; }
            EditorGUI.EndProperty();

        }
    }

}
