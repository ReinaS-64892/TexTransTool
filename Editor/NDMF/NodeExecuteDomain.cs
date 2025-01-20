using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using nadena.dev.ndmf;
using nadena.dev.ndmf.preview;
using net.rs64.TexTransCore;
using net.rs64.TexTransCoreEngineForUnity;
using net.rs64.TexTransTool.Utils;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace net.rs64.TexTransTool.NDMF
{
    internal class NodeExecuteDomain : IDomain, IDisposable
    {
        HashSet<UnityEngine.Object> _transferredObject = new();
        HashSet<RenderTexture> _neededReleaseTempRt = new();
        protected readonly NDMFPreviewStackManager _textureStacks;
        protected readonly ITextureManager _textureManager;

        ComputeContext _ctx;

        List<Renderer> _proxyDomainRenderers;
        Dictionary<Renderer, Renderer> _proxy2OriginRendererDict;

        Dictionary<Renderer, Action<Renderer>> _rendererApplyRecaller = new();//origin 2 apply call
        private IObjectRegistry _objectRegistry;
        private TTCEUnityWithTTT4Unity _ttce4U;

        public bool UsedTextureStack { get; private set; } = false;
        public bool UsedMaterialReplace { get; private set; } = false;
        public bool UsedSetMesh { get; private set; } = false;
        public bool UsedLookAt { get; private set; } = false;


        public NodeExecuteDomain(Dictionary<Renderer, Renderer> o2pDict, ComputeContext computeContext, IObjectRegistry objectRegistry)
        {
            _proxy2OriginRendererDict = o2pDict.ToDictionary(i => i.Value, i => i.Key);
            _proxyDomainRenderers = o2pDict.Values.ToList();
            _textureManager = new TextureManager(true);
            _ttce4U = new TTCEUnityWithTTT4Unity(new UnityDiskUtil(_textureManager));
            _textureStacks = new(_ttce4U);
            _ctx = computeContext;
            _objectRegistry = objectRegistry;
        }

        public void LookAt(UnityEngine.Object obj)
        {
            if (obj is Renderer renderer && _proxy2OriginRendererDict.ContainsKey(renderer)) { return; }
            _ctx?.Observe(obj);
            UsedLookAt = true;
        }

        public void LookAtChildeComponents<LookTargetComponent>(GameObject gameObject) where LookTargetComponent : Component
        {
            _ctx.GetComponentsInChildren<LookTargetComponent>(gameObject, true);
        }

        public void AddTextureStack(Texture dist, TexTransCore.ITTRenderTexture addTex, TexTransCore.ITTBlendKey blendKey)
        {
            _textureStacks.AddTextureStack(dist, addTex, blendKey);
            UsedTextureStack = true;
        }
        public IEnumerable<Renderer> EnumerateRenderer() { return _proxyDomainRenderers; }

        public ITextureManager GetTextureManager() => _textureManager;

        public bool IsPreview() => true;

        private void RegisterRecall(Renderer proxyRenderer, Action<Renderer> recall)
        {
            if (!_proxy2OriginRendererDict.ContainsKey(proxyRenderer)) { throw new InvalidOperationException($" {proxyRenderer.name} はプロキシーリストにないよ...?"); }

            if (_rendererApplyRecaller.ContainsKey(_proxy2OriginRendererDict[proxyRenderer])) { _rendererApplyRecaller[_proxy2OriginRendererDict[proxyRenderer]] += recall; }
            else { _rendererApplyRecaller[_proxy2OriginRendererDict[proxyRenderer]] = recall; }
        }
        public bool IsTemporaryAsset(UnityEngine.Object obj) { return _transferredObject.Contains(obj); }
        public void GetMutable(ref Material material)
        {
            if (IsTemporaryAsset(material)) { return; }
            var origin = material;

            var mutableMat = NDMFPreviewMaterialPool.Get(origin);
            ReplaceMaterials(new() { { origin, mutableMat } }, true);
            mutableMat.name = origin.name + "(TTT GetMutable on NDMF Preview for pooled)";
            material = mutableMat;
        }
        public void ReplaceMaterials(Dictionary<Material, Material> mapping, bool one2one = true)
        {
            foreach (var dr in _proxyDomainRenderers)
            {
                RegisterRecall(dr, i => RendererUtility.SwapMaterials(i, mapping));
                RendererUtility.SwapMaterials(dr, mapping);
            }
            if (one2one) foreach (var matKV in mapping) { RegisterReplace(matKV.Key, matKV.Value); }
            this.transferAssets(mapping.Values);
            UsedMaterialReplace = true;
        }

        public void SetMesh(Renderer renderer, Mesh mesh)
        {
            RegisterRecall(renderer, i => i.SetMesh(mesh));
            renderer.SetMesh(mesh);
            UsedSetMesh = true;
        }

        public void RegisterReplace(UnityEngine.Object oldObject, UnityEngine.Object nowObject)
        {
            _objectRegistry.RegisterReplacedObject(oldObject, nowObject);
        }
        public bool OriginEqual(UnityEngine.Object l, UnityEngine.Object r)
        {
            if (l == r) { return true; }
            if (l is Renderer lRen && r is Renderer rRen)
            {
                if (RenderersDomain.GetOrigin(_proxy2OriginRendererDict, lRen) == RenderersDomain.GetOrigin(_proxy2OriginRendererDict, rRen)) { return true; }
            }
            return _objectRegistry.GetReference(l).Equals(_objectRegistry.GetReference(r));
        }

        public void TransferAsset(UnityEngine.Object asset) { _transferredObject.Add(asset); }


        public void DomainFinish()
        {
            MargeStack();
            _textureManager.DestroyDeferred();
            _textureManager.CompressDeferred(EnumerateRenderer(), OriginEqual);
        }

        private void MargeStack()
        {
            foreach (var mergeResult in _textureStacks.StackDict)
            {
                if (mergeResult.Key == null || mergeResult.Value == null) continue;
                this.ReplaceTexture(mergeResult.Key, mergeResult.Value.Unwrap());
                _ttce4U.GammaToLinear(mergeResult.Value);
            }

            _textureManager.DestroyDeferred();
            _textureManager.CompressDeferred(EnumerateRenderer(), OriginEqual);
        }

        public void Dispose()
        {
            foreach (var obj in _transferredObject)
            {
                if (obj is Material mat) { _ = NDMFPreviewMaterialPool.Ret(mat); continue; }
                UnityEngine.Object.DestroyImmediate(obj, true);
            }
            foreach (var tRt in _neededReleaseTempRt) { TTRt.R(tRt); }
            _transferredObject.Clear();
            _textureStacks.Dispose();

            _ctx = null;
        }

        internal void DomainRecaller(Renderer original, Renderer proxy)
        {
            if (_rendererApplyRecaller.Any() is false) { return; }
            if (!_rendererApplyRecaller.ContainsKey(original))
            {
#if TTT_DISPLAY_RUNTIME_LOG
                Debug.Log($"{original.name} is can not Recall");
#endif
                return;
            }

            _rendererApplyRecaller[original].Invoke(proxy);
        }
        public ITexTransToolForUnity GetTexTransCoreEngineForUnity() => _ttce4U;
    }


    internal static class TempAssetContainer
    {
        [InitializeOnLoadMethod]
        static void Initialize()
        {
            AssemblyReloadEvents.beforeAssemblyReload += Page;
        }

        static AvatarDomainAsset s_container;
        static string s_containerPath = Path.Combine(AssetSaver.SaveDirectory, "TempAssets.asset");

        public static void TempPost(UnityEngine.Object asset)
        {
            if (s_container == null)
            {
                AssetSaver.CheckSaveDirectory();
                s_container = ScriptableObject.CreateInstance<AvatarDomainAsset>();
                AssetDatabase.CreateAsset(s_container, s_containerPath);
            }
            s_container.AddSubObject(asset);
        }

        static void Page()
        {
            AssetDatabase.DeleteAsset(s_containerPath);
        }
    }

    class NDMFPreviewStackManager : IDisposable
    {
        ITexTransToolForUnity _ttce4u;
        Dictionary<Texture, ITTRenderTexture> _stackDict = new();
        public IReadOnlyDictionary<Texture, ITTRenderTexture> StackDict => _stackDict;
        public NDMFPreviewStackManager(ITexTransToolForUnity ttce4u)
        {
            _ttce4u = ttce4u;
        }

        public void AddTextureStack(Texture dist, ITTRenderTexture addTex, ITTBlendKey blendKey)
        {
            if (_stackDict.ContainsKey(dist) is false)
            {
                var stackTexture = _ttce4u.CreateRenderTexture(dist.width, dist.height);
                stackTexture.Name = $"{dist.name}:StackTexture-{dist.width}x{dist.height}";
                stackTexture.Unwrap().CopyFilWrap(dist);

                Graphics.Blit(dist, stackTexture.Unwrap());
                _ttce4u.LinearToGamma(stackTexture);

                _stackDict.Add(dist, stackTexture);
            }
            _ttce4u.BlendingWithAnySize(_stackDict[dist], addTex, blendKey);
        }

        public void Dispose()
        {
            foreach (var rt in _stackDict) { rt.Value.Dispose(); }
            _stackDict.Clear();
        }
    }
}
