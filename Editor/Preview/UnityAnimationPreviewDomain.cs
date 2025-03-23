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
    internal class UnityAnimationPreviewDomain : IDomain, IDisposable
    {
        readonly Renderer[] domainRenderers;
        readonly UnityDiskUtil _diskUtil;
        readonly TTCEUnityWithTTT4Unity _ttce4U;
        readonly ImmediateStackManager _textureStacks;
        readonly GenericReplaceRegistry _genericReplaceRegistry = new();
        readonly HashSet<UnityEngine.Object> _transferredAssets = new();
        readonly HashSet<ITTRenderTexture> _registeredRenderTextures = new();
        readonly RenderTextureDescriptorManager _renderTextureDescriptorManager;

        protected readonly Dictionary<Texture2D, TexTransToolTextureDescriptor> TextureDescriptors = new();
        public UnityAnimationPreviewDomain(IEnumerable<Renderer> renderers)
        {
            domainRenderers = renderers.ToArray();

            _diskUtil = new UnityDiskUtil(true);
            _ttce4U = new TTCEUnityWithTTT4Unity(_diskUtil);
            _renderTextureDescriptorManager = new(_ttce4U);
            _textureStacks = new ImmediateStackManager(_ttce4U);
        }
        public IEnumerable<Renderer> EnumerateRenderer() => domainRenderers;

        public void AddTextureStack(Texture dist, ITTRenderTexture addTex, ITTBlendKey blendKey)
        {
            var tex2D = dist as Texture2D;
            if (tex2D == null) { throw new InvalidOperationException(); }
            _textureStacks.AddTextureStack(tex2D, addTex, blendKey);
        }

        public ITexTransToolForUnity GetTexTransCoreEngineForUnity() => _ttce4U;

        public TexTransToolTextureDescriptor GetTextureDescriptor(Texture texture)
        {
            return TextureManagerUtility.GetTextureDescriptor((Texture2D)texture);
        }
        public void RegisterPostProcessingAndLazyGPUReadBack(ITTRenderTexture rt, TexTransToolTextureDescriptor textureDescriptor)
        {
            _registeredRenderTextures.Add(rt);
        }

        public bool OriginEqual(Object l, Object r) { return _genericReplaceRegistry.OriginEqual(l, r); }
        public void RegisterReplace(Object oldObject, Object nowObject) { _genericReplaceRegistry.RegisterReplace(oldObject, nowObject); }


        public void ReplaceMaterials(Dictionary<Material, Material> mapping)
        {
            foreach (var r in domainRenderers)
                UnityAnimationPreviewUtility.ReplaceMaterials(r, mapping);
        }
        public void SetMesh(Renderer renderer, Mesh mesh)
        { UnityAnimationPreviewUtility.ReplaceMesh(renderer, mesh); }


        public bool IsTemporaryAsset(UnityEngine.Object asset) { return _transferredAssets.Contains(asset); }
        public void TransferAsset(Object asset)
        {
            _transferredAssets.Add(asset);
        }

        DomainPreviewCtx DomainPreviewCtx = new(true);
        T? IDomainCustomContext.GetCustomContext<T>() where T : class
        {
            if (DomainPreviewCtx is T dpc) { return dpc; }
            return null;
        }

        public void MergeStack()
        {
            var MergedStacks = _textureStacks.MergeStacks();

            foreach (var mergeResult in MergedStacks)
            {
                if (mergeResult.FirstTexture == null || mergeResult.MergeTexture == null) continue;

                var refTex = _ttce4U.GetReferenceRenderTexture(mergeResult.MergeTexture);
                this.ReplaceTexture(mergeResult.FirstTexture, refTex);
                RegisterReplace(mergeResult.FirstTexture, refTex);
                RegisterPostProcessingAndLazyGPUReadBack(mergeResult.MergeTexture, GetTextureDescriptor(mergeResult.FirstTexture));
            }
        }
        public void ReadBackToTexture2D()
        {
            var (textureDescriptors, replaceMap, originRt) = _renderTextureDescriptorManager.DownloadTexture2D();
            foreach (var r in replaceMap)
            {
                this.ReplaceTexture(r.Key, r.Value);
                RegisterReplace(r.Key, r.Value);
            }
            foreach (var rt in originRt) { rt.Dispose(); }

            foreach (var kv in textureDescriptors)
                TextureDescriptors[kv.Key] = kv.Value;
        }
        public void DomainFinish()
        {
            MergeStack();
            ReadBackToTexture2D();

            new Texture2DCompressor(TextureDescriptors).CompressDeferred(this);

            _diskUtil.Dispose();
        }
        public void Dispose()
        {
            foreach (var r in _registeredRenderTextures) { r.Dispose(); }
            foreach (var a in _transferredAssets.Where(i => i != null)) { UnityEngine.Object.DestroyImmediate(a); }
            _transferredAssets.Clear();
        }
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
