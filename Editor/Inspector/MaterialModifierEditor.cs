using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace net.rs64.TexTransTool.Editor
{
    [CustomEditor(typeof(MaterialModifier))]
    public class MaterialModifierEditor : UnityEditor.Editor
    {
        private MaterialModifier _target;
        private SerializedProperty _targetMaterial;
        private SerializedProperty _isOverrideShader;
        private SerializedProperty _overrideShader;
        private SerializedProperty _overrideProperties;

        // recoding UI fields
        private Material _recordingMaterial;
        private CustomMaterialEditor _materialEditor;

        private bool _showOverrides;


        private void OnEnable()
        {
            _target = target as MaterialModifier;
            _targetMaterial = serializedObject.FindProperty(nameof(MaterialModifier.TargetMaterial));
            _isOverrideShader = serializedObject.FindProperty(nameof(MaterialModifier.IsOverrideShader));
            _overrideShader = serializedObject.FindProperty(nameof(MaterialModifier.OverrideShader));
            _overrideProperties = serializedObject.FindProperty(nameof(MaterialModifier.OverrideProperties));

            _recordingMaterial = new Material(Shader.Find("Standard"));
            _recordingMaterial.name = "Configured Material";
            if (_target.TargetMaterial != null) { UpdateRecordingMaterial(); }

            _materialEditor = CreateEditor(_recordingMaterial, typeof(CustomMaterialEditor)) as CustomMaterialEditor;
            // 大体のイベントはObjectChangeEventsから受け取り、_recordingMaterialを更新する
            // MaterialEditorのHeaderからShaderを変更されるイベントはObjectChangeEventsから取得できないのでMaterialEditorから受け取る
            _materialEditor.OnShaderChangedPublic += ApplyOverridesToComponent;
            ObjectChangeEvents.changesPublished += OnObjectChange;
        }

        private void OnDisable()
        {
            if (_recordingMaterial != null) { DestroyImmediate(_recordingMaterial); }
            if (_materialEditor != null) { DestroyImmediate(_materialEditor); }
            _materialEditor.OnShaderChangedPublic -= ApplyOverridesToComponent;
            ObjectChangeEvents.changesPublished -= OnObjectChange;
        }

        public override void OnInspectorGUI()
        {
            TextureTransformerEditor.DrawerWarning(nameof(MaterialModifier));

            serializedObject.Update();

            EditorGUILayout.PropertyField(_targetMaterial);

            if (_targetMaterial.objectReferenceValue != null && _materialEditor != null)
            {
                using var materialEditorChange = new EditorGUI.ChangeCheckScope();
                _materialEditor.DrawHeader();
                _materialEditor.OnInspectorGUI();
                if (materialEditorChange.changed)
                {
                    ApplyOverridesToComponent();
                }
            }

            OverridesGUI();

            serializedObject.ApplyModifiedProperties();
            PreviewButtonDrawUtil.Draw(target as TexTransBehavior);
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
                OverrideUtilityGUI();
                EditorGUI.indentLevel--;
            }
        }

        // use OverrideUtility fields
        private bool _showOverrideUtility;
        private Texture _sourceTexture;
        private Texture _destinationTexture;
        private Material _originalMaterial;
        private Material _overrideMaterial;
        private Material _variantMaterial;
        private void OverrideUtilityGUI()
        {
            _showOverrideUtility = EditorGUILayout.Foldout(_showOverrideUtility, $"Utility", false);
            EditorGUILayout.Space();
            if (_showOverrideUtility)
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.LabelField("Replace Texture", EditorStyles.boldLabel);
                _sourceTexture = EditorGUILayout.ObjectField("Source Texture", _sourceTexture, typeof(Texture), false) as Texture;
                _destinationTexture = EditorGUILayout.ObjectField("Destination Texture", _destinationTexture, typeof(Texture), false) as Texture;
                if (GUILayout.Button("Add diff to this component"))
                {
                    ProcessReplaceTexture(false);
                }

                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Get Material Diff", EditorStyles.boldLabel);
                _originalMaterial ??= _target.TargetMaterial;
                _originalMaterial = EditorGUILayout.ObjectField("Original Material", _originalMaterial, typeof(Material), false) as Material;
                _overrideMaterial = EditorGUILayout.ObjectField("Override Material", _overrideMaterial, typeof(Material), false) as Material;
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Add diff to this component"))
                    {
                        ProcessMaterialDiff(false);
                    }
                }

                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Get Material Variant Diff", EditorStyles.boldLabel);
                _variantMaterial = EditorGUILayout.ObjectField("Material Variant", _variantMaterial, typeof(Material), false) as Material;
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Add diff to this component"))
                    {
                        ProcessMaterialVariantDiff(false);
                    }
                }

                EditorGUI.indentLevel--;
            }

            return;

            void ProcessReplaceTexture(bool clear)
            {
                if (_sourceTexture == null || _destinationTexture == null) { TTTRuntimeLog.Info("MaterialModifier:info:TargetNotSet"); return; }
                var overrideProperties = GetReplaceTextureOverrides(_sourceTexture, _destinationTexture);
                ApplyPropertyOverridesToComponent(overrideProperties, clear);
                _sourceTexture = null;
                _destinationTexture = null;
            }

            void ProcessMaterialDiff(bool clear)
            {
                if (_originalMaterial == null || _overrideMaterial == null) { TTTRuntimeLog.Info("MaterialModifier:info:TargetNotSet"); return; }
                ApplyOverridesToComponent(_originalMaterial, _overrideMaterial, clear);
                _originalMaterial = null;
                _overrideMaterial = null;
            }

            void ProcessMaterialVariantDiff(bool clear)
            {
                if (_variantMaterial == null) { TTTRuntimeLog.Info("MaterialModifier:info:TargetNotSet"); return; }
                var overrideProperties = GetVariantOverrideProperties(_variantMaterial).ToList();
                ApplyPropertyOverridesToComponent(overrideProperties, clear);
                _variantMaterial = null;
            }
        }

        private void ApplyOverridesToComponent()
        {
            var targetMaterial = _targetMaterial.objectReferenceValue as Material;
            ApplyOverridesToComponent(targetMaterial, _recordingMaterial, true);
        }

        private void ApplyOverridesToComponent(Material originalMaterial, Material overrideMaterial, bool clear = true)
        {
            (var isOverideShader, var overrideShader) = MaterialModifier.GetOverrideShader(originalMaterial, overrideMaterial);
            var overrideProperties = MaterialModifier.GetOverrideProperties(originalMaterial, overrideMaterial).ToList();
            ApplyShaderOverrideToComponent(isOverideShader, overrideShader);
            ApplyPropertyOverridesToComponent(overrideProperties, clear);
        }

        private void ApplyShaderOverrideToComponent(bool isOverideShader, Shader overrideShader)
        {
            _isOverrideShader.boolValue = isOverideShader;
            if (isOverideShader) { _overrideShader.objectReferenceValue = overrideShader; }
        }

        private void ApplyPropertyOverridesToComponent(List<MaterialProperty> overrides, bool clear = true)
        {
            if (clear) { _overrideProperties.ClearArray(); }
            var startIndex = _overrideProperties.arraySize;
            _overrideProperties.arraySize += overrides.Count;
            for (int i = 0; i < overrides.Count; i++)
            {
                var element = _overrideProperties.GetArrayElementAtIndex(startIndex + i);
                element.FindPropertyRelative(nameof(MaterialProperty.PropertyName)).stringValue = overrides[i].PropertyName;
                element.FindPropertyRelative(nameof(MaterialProperty.PropertyType)).enumValueIndex = (int)overrides[i].PropertyType;
                element.FindPropertyRelative(nameof(MaterialProperty.TextureValue)).objectReferenceValue = overrides[i].TextureValue;
                element.FindPropertyRelative(nameof(MaterialProperty.TextureOffsetValue)).vector2Value = overrides[i].TextureOffsetValue;
                element.FindPropertyRelative(nameof(MaterialProperty.TextureScaleValue)).vector2Value = overrides[i].TextureScaleValue;
                element.FindPropertyRelative(nameof(MaterialProperty.ColorValue)).colorValue = overrides[i].ColorValue;
                element.FindPropertyRelative(nameof(MaterialProperty.VectorValue)).vector4Value = overrides[i].VectorValue;
                element.FindPropertyRelative(nameof(MaterialProperty.IntValue)).intValue = overrides[i].IntValue;
                element.FindPropertyRelative(nameof(MaterialProperty.FloatValue)).floatValue = overrides[i].FloatValue;
            }
        }

        private List<MaterialProperty> GetReplaceTextureOverrides(Texture src, Texture dst)
        {
            var overrides = new List<MaterialProperty>();
            var properties = MaterialModifier.GetProperties(_recordingMaterial);
            foreach (var property in properties)
            {
                if (property.PropertyType == ShaderPropertyType.Texture && property.TextureValue == src)
                {
                    overrides.Add(new MaterialProperty()
                        {
                            PropertyName = property.PropertyName,
                            PropertyType = ShaderPropertyType.Texture,
                            TextureValue = dst
                        }
                    );
                }
            }
            return overrides;
        }

        private void UpdateRecordingMaterial()
        {
            if (_target.TargetMaterial == null || _recordingMaterial == null) return;
            MaterialModifier.TransferValues(_target.TargetMaterial, _recordingMaterial);
            MaterialModifier.ConfigureMaterial(_recordingMaterial, _target);
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

        private static IEnumerable<MaterialProperty> GetVariantOverrideProperties(Material variant)
        {
            if (variant == null) yield break;
            if (!variant.isVariant) yield break;

            var shader = variant.shader;
            var propertyCount = shader.GetPropertyCount();
            for (var i = 0; propertyCount > i; i += 1)
            {
                var propertyName = shader.GetPropertyName(i);
                var propertyType = shader.GetPropertyType(i);

                if (!variant.IsPropertyOverriden(propertyName)) continue;

                if (!MaterialProperty.TryGet(variant, propertyName, propertyType, out var overrideProperty)) continue;

                yield return overrideProperty;
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
