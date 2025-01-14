using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using net.rs64.TexTransTool.EditorProcessor;

namespace net.rs64.TexTransTool.Editor
{
    [CustomEditor(typeof(MaterialOverrideTransfer))]
    public class MaterialOverrideTransferEditor : UnityEditor.Editor
    {
        private MaterialOverrideTransfer _target;
        private SerializedProperty _targetMaterial;
        private SerializedProperty _mode;
        private SerializedProperty _materialVariantSource;
        private SerializedProperty _overrideShader;
        private SerializedProperty _overrideProperties;
        private UnityEditor.Editor _materialEditor;
        private bool _showOverrides;

        private void OnEnable()
        {
            _target = target as MaterialOverrideTransfer;
            _targetMaterial = serializedObject.FindProperty(nameof(MaterialOverrideTransfer.TargetMaterial));
            _mode = serializedObject.FindProperty(nameof(MaterialOverrideTransfer.Mode));
            _materialVariantSource = serializedObject.FindProperty(nameof(MaterialOverrideTransfer.MaterialVariantSource));
            _overrideShader = serializedObject.FindProperty(nameof(MaterialOverrideTransfer.OverrideShader));
            _overrideProperties = serializedObject.FindProperty(nameof(MaterialOverrideTransfer.OverrideProperties));
        }

        private void OnDisable()
        {
            if (_materialEditor != null)
            {
                DestroyImmediate(_materialEditor);
                _materialEditor = null;
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_targetMaterial);
            EditorGUILayout.PropertyField(_mode);

            var mode = (MaterialOverrideTransferMode)_mode.enumValueIndex;
            switch (mode)
            {
                case MaterialOverrideTransferMode.Variant:
                    EditorGUILayout.PropertyField(_materialVariantSource);
                    break;
                case MaterialOverrideTransferMode.Record:
                    EditMaterialGUI();
                    var count = (_overrideShader.objectReferenceValue != null ? 1 : 0) + _overrideProperties.arraySize;
                    _showOverrides = EditorGUILayout.Foldout(_showOverrides, $"Overrides: {count}");
                    if (_showOverrides)
                    {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(_overrideShader);
                        EditorGUILayout.PropertyField(_overrideProperties);
                        EditorGUI.indentLevel--;
                    }
                    break;
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void EditMaterialGUI()
        {
            if (_target.TargetMaterial == null) return;

            if (_target.IsRecording)
            {
                if (_target.TempMaterial == null) { StopEditing(); return; }

                if (GUILayout.Button("Stop Editing"))
                {
                    StopEditing();
                }
                
                _materialEditor ??= UnityEditor.Editor.CreateEditor(_target.TempMaterial);
                _materialEditor?.DrawHeader ();
                _materialEditor?.OnInspectorGUI (); 

            }
            else
            {
                if (GUILayout.Button("Start Editing"))
                {
                    StartEditing();
                }
            }
        }

        private void StartEditing()
        {
            _target.IsRecording = true;

            _target.TempMaterial = Material.Instantiate(_target.TargetMaterial);
            _target.TempMaterial.shader = _target.OverrideShader;
            MaterialOverrideTransferProcessor.SetProperties(_target.TempMaterial, _target.OverrideProperties);
            EditorUtility.SetDirty(_target);
        }

        private void StopEditing()
        {
            _target.IsRecording = false;

            if (_target.TempMaterial != null)
            {
                _target.OverrideShader = _target.TargetMaterial.shader != _target.TempMaterial.shader ? _target.TempMaterial.shader : null;
                _target.OverrideProperties = MaterialOverrideTransferProcessor.GetOverrideProperties(_target.TargetMaterial, _target.TempMaterial).ToList();
                DestroyImmediate(_target.TempMaterial);
                _target.TempMaterial = null;
            }
            EditorUtility.SetDirty(_target);

            if (_materialEditor != null)
            {
                DestroyImmediate(_materialEditor);
                _materialEditor = null;
            }
        }
    }

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