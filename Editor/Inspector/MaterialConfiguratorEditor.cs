using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace net.rs64.TexTransTool.Editor
{
    [CustomEditor(typeof(MaterialConfigurator))]
    public class MaterialConfiguratorEditor : UnityEditor.Editor
    {
        private MaterialConfigurator _target;
        private SerializedProperty _targetMaterial;
        private SerializedProperty _isoverrideShader;
        private SerializedProperty _overrideShader;
        private SerializedProperty _overrideProperties;

        private Material _recordingMaterial;
        private CustomMaterialEditor _materialEditor;
        private bool _showOverrides;

        private void OnEnable()
        {
            _target = target as MaterialConfigurator;
            _targetMaterial = serializedObject.FindProperty(nameof(MaterialConfigurator.TargetMaterial));
            _isoverrideShader = serializedObject.FindProperty(nameof(MaterialConfigurator.IsOverrideShader));
            _overrideShader = serializedObject.FindProperty(nameof(MaterialConfigurator.OverrideShader));
            _overrideProperties = serializedObject.FindProperty(nameof(MaterialConfigurator.OverrideProperties));
            _recordingMaterial = new Material(Shader.Find("Standard"));
            _recordingMaterial.name = "Configured Material";
            if (_target.TargetMaterial != null) { UpdateRecordingMaterial(); }

            _materialEditor = CreateEditor(_recordingMaterial, typeof(CustomMaterialEditor)) as CustomMaterialEditor;
            _materialEditor.OnShaderChangedPublic += OnShaderChanged;
            Undo.undoRedoPerformed += UpdateRecordingMaterial;
        }

        private void OnDisable()
        {
            if (_recordingMaterial != null)
            {
                DestroyImmediate(_recordingMaterial);
            }
            if (_materialEditor != null)
            {
                DestroyImmediate(_materialEditor);
            }
            _materialEditor.OnShaderChangedPublic -= OnShaderChanged;
            Undo.undoRedoPerformed -= UpdateRecordingMaterial;
        }


        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            bool shouldUpdate = false;

            EditorGUILayout.PropertyField(_targetMaterial);
            if (serializedObject.hasModifiedProperties)
            {
                OnTargetChanged();
                shouldUpdate = true;
            }

            if (_targetMaterial.objectReferenceValue != null)
            {
                if (_materialEditor != null) {
                    
                    _materialEditor.DrawHeader();

                    EditorGUI.BeginChangeCheck();
                    _materialEditor.OnInspectorGUI();
                    if (EditorGUI.EndChangeCheck())
                    {
                        OnMaterialEdited();
                        // shouldUpdate = true; 必須ではない
                    }
                }

                EditorGUI.BeginChangeCheck();
                OverridesGUI();
                if (EditorGUI.EndChangeCheck())
                {
                    shouldUpdate = true;
                }
            }

            serializedObject.ApplyModifiedProperties();

            if (shouldUpdate) {
                UpdateRecordingMaterial();
            }
        }

        private void OverridesGUI()
        {
            var count = (_isoverrideShader.boolValue ? 1 : 0) + _overrideProperties.arraySize;
            _showOverrides = EditorGUILayout.Foldout(_showOverrides, $"Overrides: {count}", true);
            if (_showOverrides)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_isoverrideShader);
                GUI.enabled = _isoverrideShader.boolValue;
                EditorGUILayout.PropertyField(_overrideShader);
                GUI.enabled = true;
                EditorGUILayout.PropertyField(_overrideProperties);
                EditorGUI.indentLevel--;
            }
        }

        private void OnTargetChanged()
        {
            _isoverrideShader.boolValue = false;
            _overrideShader.objectReferenceValue = null;
            _overrideProperties.ClearArray();
        }

        
        private void OnMaterialEdited()
        {
            var targetMaterial = _targetMaterial.objectReferenceValue as Material;

            CheckShaderChanged();

            var overrideProperties = MaterialConfigurator.GetOverrideProperties(targetMaterial, _recordingMaterial).ToList();
            _overrideProperties.ClearArray();
            _overrideProperties.arraySize = overrideProperties.Count;
            for(int i = 0; i < overrideProperties.Count; i++)
            {
                var element = _overrideProperties.GetArrayElementAtIndex(i);
                element.FindPropertyRelative(nameof(MaterialProperty.PropertyName)).stringValue = overrideProperties[i].PropertyName;
                element.FindPropertyRelative(nameof(MaterialProperty.PropertyType)).enumValueIndex = (int)overrideProperties[i].PropertyType;
                element.FindPropertyRelative(nameof(MaterialProperty.TextureValue)).objectReferenceValue = overrideProperties[i].TextureValue;
                element.FindPropertyRelative(nameof(MaterialProperty.TextureOffsetValue)).vector2Value = overrideProperties[i].TextureOffsetValue;
                element.FindPropertyRelative(nameof(MaterialProperty.TextureScaleValue)).vector2Value = overrideProperties[i].TextureScaleValue;
                element.FindPropertyRelative(nameof(MaterialProperty.ColorValue)).colorValue = overrideProperties[i].ColorValue;
                element.FindPropertyRelative(nameof(MaterialProperty.VectorValue)).vector4Value = overrideProperties[i].VectorValue;
                element.FindPropertyRelative(nameof(MaterialProperty.IntValue)).intValue = overrideProperties[i].IntValue;
                element.FindPropertyRelative(nameof(MaterialProperty.FloatValue)).floatValue = overrideProperties[i].FloatValue;
            }
        }

        private void CheckShaderChanged()
        {
            var targetMaterial = _targetMaterial.objectReferenceValue as Material;
            var isOverrideShader = _recordingMaterial.shader != targetMaterial.shader;
            _isoverrideShader.boolValue = isOverrideShader;
            if (isOverrideShader)
            {
                _overrideShader.objectReferenceValue = _recordingMaterial.shader;
            }
        }

        private void OnShaderChanged()
        {
            CheckShaderChanged();
            serializedObject.ApplyModifiedProperties();
        }

        private void UpdateRecordingMaterial()
        {
            if (_target.TargetMaterial == null) return;
            MaterialConfigurator.TransferValues(_target.TargetMaterial, _recordingMaterial);
            MaterialConfigurator.ConfigureMaterial(_recordingMaterial, _target);
        }
    }

    // OnShaderChangedがprotectedなのでラップする
    internal class CustomMaterialEditor : MaterialEditor
    {
        public event Action OnShaderChangedPublic;

        protected override void OnShaderChanged()
        {
            base.OnShaderChanged();
            OnShaderChangedPublic?.Invoke();
        }
    }

}