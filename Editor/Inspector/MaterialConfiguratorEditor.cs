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
        private SerializedProperty _isOverrideShader;
        private SerializedProperty _overrideShader;
        private SerializedProperty _overrideProperties;

        private Material _recordingMaterial;
        private CustomMaterialEditor _materialEditor;
        private bool _showOverrides;

        private void OnEnable()
        {
            _target = target as MaterialConfigurator;
            _targetMaterial = serializedObject.FindProperty(nameof(MaterialConfigurator.TargetMaterial));
            _isOverrideShader = serializedObject.FindProperty(nameof(MaterialConfigurator.IsOverrideShader));
            _overrideShader = serializedObject.FindProperty(nameof(MaterialConfigurator.OverrideShader));
            _overrideProperties = serializedObject.FindProperty(nameof(MaterialConfigurator.OverrideProperties));

            _recordingMaterial = new Material(Shader.Find("Standard"));
            _recordingMaterial.name = "Configured Material";
            if (_target.TargetMaterial != null) { UpdateRecordingMaterial(); }

            _materialEditor = CreateEditor(_recordingMaterial, typeof(CustomMaterialEditor)) as CustomMaterialEditor;
            // 大体のイベントはObjectChangeEventsから受け取り、_recordingMaterialを更新する
            // MaterialEditorのHeaderからShaderを変更されるイベントはObjectChangeEventsから取得できないのでMaterialEditorから受け取る
            _materialEditor.OnShaderChangedPublic += OnShaderChanged;
            ObjectChangeEvents.changesPublished += OnObjectChange;
        }

        private void OnDisable()
        {
            if (_recordingMaterial != null) { DestroyImmediate(_recordingMaterial); }
            if (_materialEditor != null) { DestroyImmediate(_materialEditor); }
            _materialEditor.OnShaderChangedPublic -= OnShaderChanged;
            ObjectChangeEvents.changesPublished -= OnObjectChange;
        }

        public override void OnInspectorGUI()
        {
            TextureTransformerEditor.DrawerWarning(nameof(MaterialConfigurator));

            serializedObject.Update();

            EditorGUILayout.PropertyField(_targetMaterial);

            if (_targetMaterial.objectReferenceValue != null && _materialEditor != null)
            {
                using var materialEditorChange = new EditorGUI.ChangeCheckScope();
                _materialEditor.DrawHeader();
                _materialEditor.OnInspectorGUI();
                if (materialEditorChange.changed)
                {
                    OnMaterialEdited();
                }
            }

            OverridesGUI();

            serializedObject.ApplyModifiedProperties();
        }

        private void OverridesGUI()
        {
            var count = (_isOverrideShader.boolValue ? 1 : 0) + _overrideProperties.arraySize;
            _showOverrides = EditorGUILayout.Foldout(_showOverrides, $"Overrides: {count}", true);
            if (_showOverrides)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_isOverrideShader);
                EditorGUILayout.PropertyField(_overrideShader);
                EditorGUILayout.PropertyField(_overrideProperties);
                EditorGUI.indentLevel--;
            }
        }

        private void OnMaterialEdited()
        {
            var targetMaterial = _targetMaterial.objectReferenceValue as Material;

            CheckShaderChanged();

            var overrideProperties = MaterialConfigurator.GetOverrideProperties(targetMaterial, _recordingMaterial).ToList();
            _overrideProperties.ClearArray();
            _overrideProperties.arraySize = overrideProperties.Count;
            for (int i = 0; i < overrideProperties.Count; i++)
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
            _isOverrideShader.boolValue = isOverrideShader;
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
        
        // 以下のEventによるプロパティの変更からUpdateRecordingMaterialを呼ぶ
        // ・Inspector上からの操作
        // ・Undo/Redo
        // ・Prefab Revert/Apply
        // 他のイベントも混じるが重複実行は問題ないのと、CustomEditorが起動しているときのみ1フレームあたり一回の呼び出しなので多分大丈夫
        // Prefab Revert/ApplyのEventを受け取るのが主な意図
        // PrefabUtility.prefabInstanceUpdatedはPrefabIntanceのEventしか取得できないのと、ApplyAllなどに反応しないっぽい？
        private void OnObjectChange(ref ObjectChangeEventStream stream)
        {
            for (int i = 0; i < stream.length; i++)
            {
                var eventType = stream.GetEventType(i);
                if (eventType == ObjectChangeKind.ChangeGameObjectOrComponentProperties || eventType == ObjectChangeKind.UpdatePrefabInstances || eventType == ObjectChangeKind.ChangeAssetObjectProperties)
                {
                    UpdateRecordingMaterial();
                    return;
                }
            }
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
