using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using net.rs64.TexTransTool.Utils;
using net.rs64.TexTransCore;

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
            _recordingMaterial.name = "Modified Material";
            if (_target.TargetMaterial != null) { UpdateRecordingMaterial(); }

            _materialEditor = CreateEditor(_recordingMaterial, typeof(CustomMaterialEditor)) as CustomMaterialEditor;
            // 大体のイベントはObjectChangeEventsから受け取り、_recordingMaterialを更新する
            // MaterialEditorのHeaderからShaderを変更されるイベントはObjectChangeEventsから取得できないのでMaterialEditorから受け取る
            _materialEditor.OnShaderChangedPublic += OnShaderChanged;
            ObjectChangeEvents.changesPublished += OnObjectChanged;
        }

        private void OnDisable()
        {
            if (_recordingMaterial != null) { DestroyImmediate(_recordingMaterial); }
            if (_materialEditor != null) { DestroyImmediate(_materialEditor); }
            _materialEditor.OnShaderChangedPublic -= OnShaderChanged;
            ObjectChangeEvents.changesPublished -= OnObjectChanged;
        }

        public override void OnInspectorGUI()
        {
            TextureTransformerEditor.DrawOldSaveDataVersionWarning(target as TexTransMonoBase);
            TextureTransformerEditor.DrawerWarning(nameof(MaterialModifier));
            serializedObject.Update();

            EditorGUILayout.PropertyField(_targetMaterial);
            RecordingMaterialGUI();
            OverridesGUI();

            serializedObject.ApplyModifiedProperties();
            PreviewButtonDrawUtil.Draw(target as TexTransBehavior);
        }

        private void RecordingMaterialGUI()
        {
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
                EditorGUILayout.PropertyField(_overrideProperties, "OverrideProperties".GlcV());
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
                _sourceTexture = EditorGUILayout.ObjectField("Source Texture", _sourceTexture, typeof(Texture), false, GUILayout.Height(18f)) as Texture;
                _destinationTexture = EditorGUILayout.ObjectField("Destination Texture", _destinationTexture, typeof(Texture), false, GUILayout.Height(18f)) as Texture;
                if (GUILayout.Button("Add diff to this component"))
                {
                    ProcessReplaceTexture();
                }

                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Get Material Diff", EditorStyles.boldLabel);
                _originalMaterial ??= _target.TargetMaterial;
                _originalMaterial = EditorGUILayout.ObjectField("Original Material", _originalMaterial, typeof(Material), false) as Material;
                _overrideMaterial = EditorGUILayout.ObjectField("Override Material", _overrideMaterial, typeof(Material), false) as Material;
                if (GUILayout.Button("Add diff to this component"))
                {
                    ProcessMaterialDiff();
                }

                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Get Material Variant Diff", EditorStyles.boldLabel);
                _variantMaterial = EditorGUILayout.ObjectField("Material Variant", _variantMaterial, typeof(Material), false) as Material;
                if (GUILayout.Button("Add diff to this component"))
                {
                    ProcessMaterialVariantDiff();
                }

                EditorGUI.indentLevel--;
            }

            return;

            void ProcessReplaceTexture()
            {
                if (_sourceTexture == null || _destinationTexture == null) { TTLog.Info("MaterialModifier:info:TargetNotSet"); return; }

                _recordingMaterial.ReplaceTextureInPlace(_destinationTexture, _sourceTexture);
                ApplyOverridesToComponent();

                _sourceTexture = null;
                _destinationTexture = null;
            }

            void ProcessMaterialDiff()
            {
                if (_originalMaterial == null || _overrideMaterial == null) { TTLog.Info("MaterialModifier:info:TargetNotSet"); return; }

                var overrideProperties = MaterialModifier.GetOverrideProperties(_originalMaterial, _overrideMaterial).ToList();
                MaterialModifier.ConfigureMaterial(_recordingMaterial, false, null, overrideProperties);
                ApplyOverridesToComponent();

                _originalMaterial = null;
                _overrideMaterial = null;
            }

            void ProcessMaterialVariantDiff()
            {
                if (_variantMaterial == null) { TTLog.Info("MaterialModifier:info:TargetNotSet"); return; }

                var overrideProperties = GetVariantOverrideProperties(_variantMaterial).ToList();
                MaterialModifier.ConfigureMaterial(_recordingMaterial, false, null, overrideProperties);
                ApplyOverridesToComponent();

                _variantMaterial = null;
            }
        }

        private void ApplyOverridesToComponent()
        {
            var targetMaterial = _targetMaterial.objectReferenceValue as Material;
            ApplyOverridesToComponent(targetMaterial, _recordingMaterial);
        }

        private void ApplyOverridesToComponent(Material originalMaterial, Material overrideMaterial)
        {
            var (isOverideShader, overrideShader) = MaterialModifier.GetOverrideShader(originalMaterial, overrideMaterial);
            ApplyShaderOverrideToComponent(isOverideShader, overrideShader);

            var overrideProperties = MaterialModifier.GetOverrideProperties(originalMaterial, overrideMaterial).ToList();
            ApplyPropertyOverridesToComponent(overrideProperties);
        }

        private void ApplyShaderOverrideToComponent(bool isOverrideShader, Shader overrideShader)
        {
            _isOverrideShader.boolValue = isOverrideShader;
            if (isOverrideShader) { _overrideShader.objectReferenceValue = overrideShader; }
        }

        private void ApplyPropertyOverridesToComponent(List<MaterialProperty> overrides)
        {
            var willRemove = new HashSet<string>();//削除対象を事前に調査
            {
                var nowAdd = overrides.Select(i => i.PropertyName).ToHashSet();
                for (int i = 0; i < _overrideProperties.arraySize; i += 1)
                {
                    var element = _overrideProperties.GetArrayElementAtIndex(i);
                    var pn = element.FindPropertyRelative(nameof(MaterialProperty.PropertyName)).stringValue;
                    if (nowAdd.Contains(pn) is false) { willRemove.Add(pn); }
                }
            }

            var addNew = new List<MaterialProperty>(overrides.Count);

            foreach (var mp in overrides)// 上書き add , 無かったら addHashに
            {
                var propertyName = mp.PropertyName;

                var writeIndex = -1;
                for (int i = 0; i < _overrideProperties.arraySize; i += 1)
                {
                    var element = _overrideProperties.GetArrayElementAtIndex(i);
                    if (element.FindPropertyRelative(nameof(MaterialProperty.PropertyName)).stringValue != propertyName) { continue; }

                    WriteProperties(element, mp);
                    writeIndex = i;
                    break;
                }
                if (writeIndex is -1) { addNew.Add(mp); continue; }

                var startIndex = _overrideProperties.arraySize - 1;
                for (int i = startIndex; (writeIndex + 1) <= i; i -= 1)// 重複を削除
                {
                    var element = _overrideProperties.GetArrayElementAtIndex(i);
                    if (element.FindPropertyRelative(nameof(MaterialProperty.PropertyName)).stringValue != propertyName) { continue; }
                    _overrideProperties.DeleteArrayElementAtIndex(i);
                }
            }

            foreach (var mp in addNew)// 新規 add
            {
                var newIndex = _overrideProperties.arraySize;
                _overrideProperties.arraySize += 1;
                WriteProperties(_overrideProperties.GetArrayElementAtIndex(newIndex), mp);
            }

            if (willRemove.Any())
            {
                var startIndex = _overrideProperties.arraySize - 1;
                for (int i = startIndex; 0 <= i; i -= 1)
                {
                    var element = _overrideProperties.GetArrayElementAtIndex(i);
                    var pn = element.FindPropertyRelative(nameof(MaterialProperty.PropertyName)).stringValue;
                    if (willRemove.Contains(pn) is false) { continue; }
                    _overrideProperties.DeleteArrayElementAtIndex(i);
                }
            }

            static void WriteProperties(SerializedProperty element, MaterialProperty mp)
            {
                element.FindPropertyRelative(nameof(MaterialProperty.PropertyName)).stringValue = mp.PropertyName;
                element.FindPropertyRelative(nameof(MaterialProperty.PropertyType)).enumValueIndex = (int)mp.PropertyType;
                element.FindPropertyRelative(nameof(MaterialProperty.TextureValue)).objectReferenceValue = mp.TextureValue;
                element.FindPropertyRelative(nameof(MaterialProperty.TextureOffsetValue)).vector2Value = mp.TextureOffsetValue;
                element.FindPropertyRelative(nameof(MaterialProperty.TextureScaleValue)).vector2Value = mp.TextureScaleValue;
                element.FindPropertyRelative(nameof(MaterialProperty.ColorValue)).colorValue = mp.ColorValue;
                element.FindPropertyRelative(nameof(MaterialProperty.VectorValue)).vector4Value = mp.VectorValue;
                element.FindPropertyRelative(nameof(MaterialProperty.IntValue)).intValue = mp.IntValue;
                element.FindPropertyRelative(nameof(MaterialProperty.FloatValue)).floatValue = mp.FloatValue;
            }
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
        private void OnObjectChanged(ref ObjectChangeEventStream stream)
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

        private void OnShaderChanged()
        {
            ApplyOverridesToComponent();
            serializedObject.ApplyModifiedProperties();
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
