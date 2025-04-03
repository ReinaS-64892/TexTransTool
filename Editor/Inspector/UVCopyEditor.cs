#nullable enable
using UnityEngine;
using UnityEditor;
using net.rs64.TexTransTool.Preview;
using net.rs64.TexTransTool.MultiLayerImage;
using net.rs64.TexTransTool.Editor.OtherMenuItem;
using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransTool.Utils;
using net.rs64.TexTransTool.Decal;
using net.rs64.TexTransTool.Editor.Decal;
namespace net.rs64.TexTransTool.Editor
{

    [CustomEditor(typeof(UVCopy))]
    internal class UVCopyEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            TextureTransformerEditor.DrawOldSaveDataVersionWarning(target as TexTransMonoBase);
            TextureTransformerEditor.DrawerWarning(nameof(UVCopy));
            serializedObject.Update();

            var sTargetMesh = serializedObject.FindProperty(nameof(UVCopy.TargetMeshes));
            var sCopySource = serializedObject.FindProperty(nameof(UVCopy.CopySource));
            var sCopyTarget = serializedObject.FindProperty(nameof(UVCopy.CopyTarget));

            EditorGUILayout.PropertyField(sTargetMesh);
            if (sTargetMesh.isExpanded is false && PreviewUtility.IsPreviewContains is false)
            {
                DrawDomainsMeshSelector(sTargetMesh);
            }
            EditorGUILayout.PropertyField(sCopySource);
            EditorGUILayout.PropertyField(sCopyTarget);

            serializedObject.ApplyModifiedProperties();
            PreviewButtonDrawUtil.Draw(target as TexTransBehavior);
        }

        List<Mesh>? _domainsMeshes;
        private void DrawDomainsMeshSelector(SerializedProperty sTargetMesh)
        {
            if (_domainsMeshes == null)
            {
                _domainsMeshes = new();
                var entry = sTargetMesh.serializedObject.targetObject as UVCopy;
                if (entry == null) { return; }
                var domainRoot = DomainMarkerFinder.FindMarker(entry.gameObject);
                if (domainRoot == null) { return; }
                var renderers = domainRoot.GetComponentsInChildren<Renderer>();
                _domainsMeshes = renderers.Select(r => r.GetMesh()).Where(m => m != null).Distinct().ToList();
            }
            TargetObjectSelector.DrawTargetSelectionLayout(sTargetMesh, _domainsMeshes);
        }

    }

}
