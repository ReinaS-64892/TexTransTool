#nullable enable
using System.Collections.Generic;
using net.rs64.TexTransTool.TextureStack;
using UnityEngine;
using Object = UnityEngine.Object;
using net.rs64.TexTransCore;
using net.rs64.TexTransTool.Utils;
using System;

namespace net.rs64.TexTransTool
{
    /// <summary>
    /// This is an IDomain implementation that applies to specified renderers.
    /// </summary>
    internal class RenderersDomain : IDomain, IDisposable
    {
        protected List<Renderer> _renderers;

        protected readonly GenericReplaceRegistry _genericReplaceRegistry = new();
        protected readonly IAssetSaver _saver;

        protected readonly ITexTransUnityDiskUtil _diskUtil;
        protected readonly ITexTransToolForUnity _ttce4U;

        protected readonly ImmediateStackManager _textureStacks;

        protected readonly RenderTextureDescriptorManager _renderTextureDescriptorManager;
        protected readonly Dictionary<Texture2D, TexTransToolTextureDescriptor> _textureDescriptors = new();

        public RenderersDomain(List<Renderer> previewRenderers, IAssetSaver assetSaver)
        {
            _renderers = previewRenderers;
            _saver = assetSaver;
            _diskUtil = new UnityDiskUtil(false);
#if TTT_TTCE_TRACING
            _ttce4U = new TTCEUnityWithTTT4Unity(new TTDiskUtilInterfaceDebug(_diskUtil, Debug.LogWarning));
            _ttce4U = new TTCEWithTTT4UInterfaceDebug(_ttce4U, Debug.LogWarning);
#else
            _ttce4U = new TTCEUnityWithTTT4Unity(_diskUtil);//TODO : コンストラクタの引数にとることができるようにする必要がある
#endif
            _renderTextureDescriptorManager = new(_ttce4U);
            _textureStacks = new ImmediateStackManager(_ttce4U);
        }
        public RenderersDomain(List<Renderer> previewRenderers, IAssetSaver assetSaver, ITexTransUnityDiskUtil diskUtil, ITexTransToolForUnity ttt4u)
        {
            _renderers = previewRenderers;
            _saver = assetSaver;
            _diskUtil = diskUtil;
            _ttce4U = ttt4u;
            _renderTextureDescriptorManager = new(_ttce4U);
            _textureStacks = new ImmediateStackManager(_ttce4U);
        }

        public ITexTransToolForUnity GetTexTransCoreEngineForUnity() => _ttce4U;
        public virtual void AddTextureStack(Texture dist, ITTRenderTexture addTex, ITTBlendKey blendKey)
        {
            _textureStacks.AddTextureStack(dist, addTex, blendKey);
        }

        public virtual void ReplaceMaterials(Dictionary<Material, Material> mapping)
        { RendererUtility.SwapMaterials(_renderers, mapping); }
        public virtual void SetMesh(Renderer renderer, Mesh mesh) { renderer.SetMesh(mesh); }

        public bool IsTemporaryAsset(Object Asset) => _saver?.IsTemporaryAsset(Asset) ?? false;
        public void TransferAsset(Object Asset) => _saver?.TransferAsset(Asset);

        public virtual bool OriginEqual(Object? l, Object? r)
        {
            return _genericReplaceRegistry.OriginEqual(l, r);
        }
        public virtual void RegisterReplace(Object oldObject, Object nowObject) { _genericReplaceRegistry.RegisterReplace(oldObject, nowObject); }


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
                TransferAsset(r.Value);
                this.ReplaceTexture(r.Key, r.Value);

                var replace = _genericReplaceRegistry.ReplacePooledRenderTexture(r.Key, r.Value);
                if (replace.HasValue) RegisterReplace(replace.Value.Key, replace.Value.Value);
            }
            foreach (var rt in originRt) { rt.Dispose(); }

            foreach (var kv in textureDescriptors)
                _textureDescriptors[kv.Key] = kv.Value;
        }

        public IEnumerable<Renderer> EnumerateRenderer() { return _renderers; }

        public TexTransToolTextureDescriptor GetTextureDescriptor(Texture texture)
        { return _renderTextureDescriptorManager.GetTextureDescriptor(texture); }
        public void RegisterPostProcessingAndLazyGPUReadBack(ITTRenderTexture rt, TexTransToolTextureDescriptor textureDescriptor)
        { _renderTextureDescriptorManager.RegisterPostProcessingAndLazyGPUReadBack(rt, textureDescriptor); }

        T? IDomainCustomContext.GetCustomContext<T>() where T : class
        {
            return GetCustomContext<T>();
        }
        protected virtual T? GetCustomContext<T>() where T : class { return null; }

        public virtual void Dispose()
        {
            MergeStack();
            ReadBackToTexture2D();

            new Texture2DCompressor(_textureDescriptors).CompressDeferred(this);

            if (_diskUtil is IDisposable dd) dd.Dispose();
            if (_ttce4U is IDisposable t4uD) t4uD.Dispose();
        }
    }
}
