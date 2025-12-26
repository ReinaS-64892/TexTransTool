#nullable enable
using UnityEngine;
using System.Collections.Generic;
using System;
using net.rs64.TexTransCore;

namespace net.rs64.TexTransTool
{
    internal class NotWorkDomain : IDomain, IDisposable
    {
        IEnumerable<Renderer> _domainRenderers;
        HashSet<UnityEngine.Object> _transferredObject = new();
        HashSet<ITTRenderTexture> _transferredRT = new();

        private readonly ITexTransToolForUnity _ttce4U;

        public NotWorkDomain(IEnumerable<Renderer> renderers, ITexTransToolForUnity iTexTransToolForUnity)
        {
            _domainRenderers = renderers;
            _ttce4U = iTexTransToolForUnity;
        }

        public void AddTextureStack(Texture dist, ITTRenderTexture addTex, ITTBlendKey blendKey) { }
        public IEnumerable<Renderer> EnumerateRenderers() { return _domainRenderers; }
        public bool IsPreview() { return true; }
        public bool OriginalObjectEquals(UnityEngine.Object? l, UnityEngine.Object? r) { return l == r; }
        public void RegisterReplacement(UnityEngine.Object oldObject, UnityEngine.Object nowObject) { }
        public void ReplaceMaterials(Dictionary<Material, Material> mapping) { }
        public void SetMesh(Renderer renderer, Mesh mesh) { }
        public void SetMaterials(Renderer renderer, Material[] materials) { }
        public void SaveAsset(UnityEngine.Object asset) { _transferredObject.Add(asset); }
        public void Dispose()
        {
            foreach (var obj in _transferredObject) { UnityEngine.Object.DestroyImmediate(obj); }
            foreach (var rt in _transferredRT) { rt.Dispose(); }
        }
        public ITexTransToolForUnity GetTexTransCoreEngineForUnity() => _ttce4U;
        public TexTransToolTextureDescriptor GetTextureDescriptor(Texture texture) { return new(); }
        public void RegisterTextureDescription(ITTRenderTexture rt, TexTransToolTextureDescriptor textureDescriptor) { _transferredRT.Add(rt); }
        T? IDomainCustomContext.GetCustomContext<T>() where T : class { return null; }
    }


}

