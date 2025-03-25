#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransCore;
using net.rs64.TexTransTool.Preview.Custom;
using net.rs64.TexTransTool.TextureStack;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Profiling;
using Object = UnityEngine.Object;

namespace net.rs64.TexTransTool.Preview
{
    internal class UnityAnimationPreviewDomain : RenderersDomain, IDisposable
    {
        protected readonly Dictionary<Texture2D, TexTransToolTextureDescriptor> TextureDescriptors = new();

        private UnityAnimationPreviewDomain(List<Renderer> previewRenderers, IAssetSaver assetSaver, ITexTransUnityDiskUtil diskUtil, ITexTransToolForUnity ttt4u)
        : base(previewRenderers, assetSaver, diskUtil, ttt4u) { }
        public static (UnityAnimationPreviewDomain domain, TempAssetHolder tempAssetHolder) Create(List<Renderer> renderers)
        {
            var tempAssetHolder = new TempAssetHolder();
            var diskUtil = new UnityDiskUtil(true);
            var ttt4u = new TTCEUnityWithTTT4Unity(diskUtil);
            return (new(renderers, tempAssetHolder, diskUtil, ttt4u), tempAssetHolder);
        }
        public override void ReplaceMaterials(Dictionary<Material, Material> mapping)
        {
            foreach (var r in _renderers)
                UnityAnimationPreviewUtility.ReplaceMaterials(r, mapping);
        }
        public override void SetMesh(Renderer renderer, Mesh mesh) { UnityAnimationPreviewUtility.ReplaceMesh(renderer, mesh); }

        DomainPreviewCtx DomainPreviewCtx = new(true);
        protected override T? GetCustomContext<T>() where T : class
        { if (DomainPreviewCtx is T dpc) { return dpc; } return null; }
    }
    internal static class UnityAnimationPreviewUtility
    {
        public static IDisposable UnityAnimationModeScoop() { return new UniAniModeScoop(); }
        class UniAniModeScoop : IDisposable
        {
            public UniAniModeScoop() { AnimationMode.StartAnimationMode(); }
            public void Dispose() { AnimationMode.StopAnimationMode(); }
        }

        private static void AddPropertyModification(Object component, string property, Object value)
        {
            AnimationMode.AddPropertyModification(
                EditorCurveBinding.PPtrCurve("", component.GetType(), property),
                new PropertyModification
                {
                    target = component,
                    propertyPath = property,
                    objectReference = value,
                },
                true);
        }
        internal static void ReplaceMaterials(Renderer renderer, Dictionary<Material, Material> mapping)
        {
            if (renderer == null) { return; }
            if (renderer.sharedMaterials.Any() is false) { return; }
            using (var serialized = new SerializedObject(renderer))
            {
                foreach (SerializedProperty property in serialized.FindProperty("m_Materials"))
                    if (property.objectReferenceValue is Material material && mapping.TryGetValue(material, out var replacement))
                    {
                        AddPropertyModification(property.serializedObject.targetObject, property.propertyPath, property.objectReferenceValue);
                        property.objectReferenceValue = replacement;
                    }
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
        }
        internal static void ReplaceMesh(Renderer renderer, Mesh mesh)
        {
            switch (renderer)
            {
                case SkinnedMeshRenderer skinnedRenderer:
                    {
                        AddPropertyModification(renderer, "m_Mesh", skinnedRenderer.sharedMesh);
                        skinnedRenderer.sharedMesh = mesh;
                        break;
                    }
                case MeshRenderer meshRenderer:
                    {
                        var meshFilter = meshRenderer.GetComponent<MeshFilter>();
                        AddPropertyModification(meshFilter, "m_Mesh", meshFilter.sharedMesh);
                        meshFilter.sharedMesh = mesh;
                        break;
                    }
                default:
                    throw new ArgumentException($"Unexpected Renderer Type: {renderer.GetType()}", nameof(renderer));
            }
        }
    }
}
