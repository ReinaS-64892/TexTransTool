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
        private SerializedProperty _overrideShader;
        private SerializedProperty _overrideProperties;
        private UnityEditor.Editor _materialEditor;
        private bool _showOverrides;

        private void OnEnable()
        {
            _target = target as MaterialConfigurator;
            _targetMaterial = serializedObject.FindProperty(nameof(MaterialConfigurator.TargetMaterial));
            _overrideShader = serializedObject.FindProperty(nameof(MaterialConfigurator.OverrideShader));
            _overrideProperties = serializedObject.FindProperty(nameof(MaterialConfigurator.OverrideProperties));
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
            EditMaterialGUI();
            OverridesGUI();

            serializedObject.ApplyModifiedProperties();
        }

        private void EditMaterialGUI()
        {
            GUI.enabled = _target.TargetMaterial != null;
            
            if (_target.IsRecording)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Apply", GUILayout.Width(EditorGUIUtility.currentViewWidth * 0.7f)))
                    {
                        StopEditing(true);
                    }
                    if (GUILayout.Button("Cancel"))
                    {
                        StopEditing(false);
                    }
                }

                if (_target.RecordingMaterial == null) { StopEditing(); return; }

                if (_target.TargetMaterial != null && _target.RecordingMaterial != null) {
                    _materialEditor ??= UnityEditor.Editor.CreateEditor(_target.RecordingMaterial);
                    _materialEditor?.DrawHeader ();
                    _materialEditor?.OnInspectorGUI (); 
                }

            }
            else
            {
                if (GUILayout.Button("Start Editing"))
                {
                    StartEditing();
                }
            }

            GUI.enabled = true;
        }

        private void OverridesGUI()
        {
            var count = (_overrideShader.objectReferenceValue != null ? 1 : 0) + _overrideProperties.arraySize;
            _showOverrides = EditorGUILayout.Foldout(_showOverrides, $"Overrides: {count}");
            if (_showOverrides)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_overrideShader);
                EditorGUILayout.PropertyField(_overrideProperties);
                EditorGUI.indentLevel--;
            }
        }

        private void StartEditing()
        {
            _target.IsRecording = true;

            _target.RecordingMaterial = Material.Instantiate(_target.TargetMaterial);
            _target.RecordingMaterial.name += "_Configured";
            if (_target.OverrideShader != null) _target.RecordingMaterial.shader = _target.OverrideShader;
            MaterialConfigurator.SetProperties(_target.RecordingMaterial, _target.OverrideProperties);
            EditorUtility.SetDirty(_target);
        }

        private void StopEditing(bool apply = true)
        {
            _target.IsRecording = false;

            if (_target.RecordingMaterial != null)
            {
                if (apply) {
                    _target.OverrideShader = _target.TargetMaterial.shader != _target.RecordingMaterial.shader ? _target.RecordingMaterial.shader : null;
                    _target.OverrideProperties = MaterialConfigurator.GetOverrideProperties(_target.TargetMaterial, _target.RecordingMaterial).ToList();
                }
                DestroyImmediate(_target.RecordingMaterial);
                _target.RecordingMaterial = null;
            }
            EditorUtility.SetDirty(_target);

            if (_materialEditor != null)
            {
                DestroyImmediate(_materialEditor);
                _materialEditor = null;
            }
        }
    }

}