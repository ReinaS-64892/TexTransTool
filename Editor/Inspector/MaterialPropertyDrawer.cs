using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

namespace net.rs64.TexTransTool.Editor
{
    [CustomPropertyDrawer(typeof(MaterialProperty))]
    internal class MaterialPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            position.height = EditorGUIUtility.singleLineHeight;
            
            var propertyName = property.FindPropertyRelative(nameof(MaterialProperty.PropertyName));
            var propertyType = property.FindPropertyRelative(nameof(MaterialProperty.PropertyType));
            var textureValue = property.FindPropertyRelative(nameof(MaterialProperty.TextureValue));
            var textureOffsetValue = property.FindPropertyRelative(nameof(MaterialProperty.TextureOffsetValue));
            var textureScaleValue = property.FindPropertyRelative(nameof(MaterialProperty.TextureScaleValue));
            var colorValue = property.FindPropertyRelative(nameof(MaterialProperty.ColorValue));
            var vectorValue = property.FindPropertyRelative(nameof(MaterialProperty.VectorValue));
            var intValue = property.FindPropertyRelative(nameof(MaterialProperty.IntValue));
            var floatValue = property.FindPropertyRelative(nameof(MaterialProperty.FloatValue));

            EditorGUI.PropertyField(position, propertyName);
            position.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(position, propertyType);
            position.y += EditorGUIUtility.singleLineHeight;

            switch ((ShaderPropertyType)propertyType.enumValueIndex)
            {
                case ShaderPropertyType.Texture:
                    EditorGUI.PropertyField(position, textureValue);
                    position.y += EditorGUIUtility.singleLineHeight;
                    EditorGUI.PropertyField(position, textureOffsetValue);
                    position.y += EditorGUIUtility.singleLineHeight;
                    EditorGUI.PropertyField(position, textureScaleValue);
                    position.y += EditorGUIUtility.singleLineHeight;
                    break;
                case ShaderPropertyType.Color:
                    EditorGUI.PropertyField(position, colorValue);
                    position.y += EditorGUIUtility.singleLineHeight;
                    break;
                case ShaderPropertyType.Vector:
                    Vector4 newValue = EditorGUI.Vector4Field(position, label, vectorValue.vector4Value);
                    if (EditorGUI.EndChangeCheck())
                    {
                        vectorValue.vector4Value = newValue;
                    }
                    position.y += EditorGUIUtility.singleLineHeight;
                    break;
                case ShaderPropertyType.Int:
                    EditorGUI.PropertyField(position, intValue);
                    position.y += EditorGUIUtility.singleLineHeight;
                    break;
                case ShaderPropertyType.Float:
                case ShaderPropertyType.Range:
                    EditorGUI.PropertyField(position, floatValue);
                    position.y += EditorGUIUtility.singleLineHeight;
                    break;
            }

            EditorGUI.EndProperty();
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {   
            var height = EditorGUIUtility.singleLineHeight * 2;
            var propertyType = property.FindPropertyRelative(nameof(MaterialProperty.PropertyType));
            switch ((ShaderPropertyType)propertyType.enumValueIndex)
            {
                case ShaderPropertyType.Texture:
                    height += EditorGUIUtility.singleLineHeight * 3;
                    break;
                case ShaderPropertyType.Color:
                case ShaderPropertyType.Vector:
                case ShaderPropertyType.Int:
                case ShaderPropertyType.Float:
                case ShaderPropertyType.Range:
                    height += EditorGUIUtility.singleLineHeight;
                    break;
            }
            return height;
        }
    }
}
