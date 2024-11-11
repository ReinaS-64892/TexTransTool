using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using net.rs64.TexTransCoreEngineForUnity;
using net.rs64.TexTransCoreEngineForUnity.Utils;
using net.rs64.TexTransTool.TextureStack;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using static net.rs64.TexTransCoreEngineForUnity.TextureBlend;
using net.rs64.TexTransCore;

namespace net.rs64.TexTransTool
{
    /// <summary>
    /// This is an IDomain implementation that applies to specified renderers.
    ///
    /// If <see cref="Previewing"/> is true, This will call <see cref="AnimationMode.AddPropertyModification"/>
    /// everytime modifies some property so you can revert those changes with <see cref="AnimationMode.StopAnimationMode"/>.
    /// This class doesn't call <see cref="AnimationMode.BeginSampling"/> and <see cref="AnimationMode.EndSampling"/>
    /// so user must call those if needed.
    /// </summary>
    internal class RenderersDomain : IDomain
    {
        protected List<Renderer> _renderers;
        public readonly bool Previewing;

        [CanBeNull] protected readonly IAssetSaver _saver;
        protected readonly ITextureManager _textureManager;
        protected readonly ImmediateStackManager _textureStacks;
        protected readonly ITexTransToolForUnity _ttce4U;

        protected Dictionary<UnityEngine.Object, UnityEngine.Object> _replaceMap = new();//New Old

        public RenderersDomain(List<Renderer> renderers, bool previewing, bool saveAsset = false, bool? useCompress = null)
        : this(renderers, previewing, saveAsset ? new AssetSaver() : null, useCompress) { }
        public RenderersDomain(List<Renderer> renderers, bool previewing, IAssetSaver assetSaver, bool? useCompress = null)
        : this(renderers, previewing, new TextureManager(previewing, useCompress), assetSaver) { }
        public RenderersDomain(List<Renderer> previewRenderers, bool previewing, ITextureManager textureManager, IAssetSaver assetSaver)
        {
            _renderers = previewRenderers;
            Previewing = previewing;
            _saver = assetSaver;
            _textureManager = textureManager;
            _ttce4U = new TTCE4UnityWithTTT4Unity(new UnityDiskUtil(_textureManager));//TODO : コンストラクタの引数にとることができるようにする必要がある

            _textureStacks = new ImmediateStackManager(_ttce4U, _textureManager);
        }

        public ITexTransToolForUnity GetTexTransCoreEngineForUnity() => _ttce4U;
        public virtual void AddTextureStack(Texture dist, ITTRenderTexture addTex, ITTBlendKey blendKey)
        {
            _textureStacks.AddTextureStack(dist as Texture2D, addTex, blendKey);
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

        public void ReplaceMaterials(Dictionary<Material, Material> mapping, bool one2one = true)
        {
            foreach (var renderer in _renderers) { ReplaceMaterial(renderer, mapping, Previewing); }

            if (one2one) { foreach (var keyValuePair in mapping) { RegisterReplace(keyValuePair.Key, keyValuePair.Value); } }
            this.transferAssets(mapping.Values);
        }

        internal static void ReplaceMaterial(Renderer renderer, Dictionary<Material, Material> mapping, bool previewing = false)
        {
            if (renderer == null) { return; }
            if (!renderer.sharedMaterials.Any()) { return; }
            using (var serialized = new SerializedObject(renderer))
            {
                foreach (SerializedProperty property in serialized.FindProperty("m_Materials"))
                    if (property.objectReferenceValue is Material material && mapping.TryGetValue(material, out var replacement))
                    {
                        if (previewing) AddPropertyModification(property.serializedObject.targetObject, property.propertyPath, property.objectReferenceValue);
                        property.objectReferenceValue = replacement;
                    }

                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        public void SetMesh(Renderer renderer, Mesh mesh)
        {
            var preMesh = ReplaceMesh(renderer, mesh, Previewing);
            RegisterReplace(preMesh, mesh);
        }

        private static Mesh ReplaceMesh(Renderer renderer, Mesh mesh, bool previewing = false)
        {
            var preMesh = renderer.GetMesh();
            switch (renderer)
            {
                case SkinnedMeshRenderer skinnedRenderer:
                    {
                        if (previewing) AddPropertyModification(renderer, "m_Mesh", skinnedRenderer.sharedMesh);
                        skinnedRenderer.sharedMesh = mesh;
                        break;
                    }
                case MeshRenderer meshRenderer:
                    {
                        var meshFilter = meshRenderer.GetComponent<MeshFilter>();
                        if (previewing) AddPropertyModification(meshFilter, "m_Mesh", meshFilter.sharedMesh);
                        meshFilter.sharedMesh = mesh;
                        break;
                    }
                default:
                    throw new ArgumentException($"Unexpected Renderer Type: {renderer.GetType()}", nameof(renderer));
            }

            return preMesh;
        }

        public void SetTexture(Texture2D target, Texture2D setTex)
        {
            var mats = RendererUtility.GetFilteredMaterials(_renderers);
            ReplaceMaterials(MaterialUtility.ReplaceTextureAll(mats, target, setTex));
            RegisterReplace(target, setTex);
        }

        public void TransferAsset(Object Asset) => _saver?.TransferAsset(Asset);


        public virtual bool OriginEqual(Object l, Object r)
        {
            if (l == r) { return true; }
            return GetOrigin(_replaceMap, l) == GetOrigin(_replaceMap, r);
        }

        public static T GetOrigin<T>(Dictionary<T, T> replaceMap, T obj)
        {
            if (obj == null) { return default; }
            while (replaceMap.ContainsKey(obj)) { obj = replaceMap[obj]; }
            return obj;
        }

        public virtual void RegisterReplace(Object oldObject, Object nowObject)
        {
            _replaceMap[nowObject] = oldObject;
        }
        public virtual void EditFinish()
        {
            MergeStack();
            _textureManager.DestroyDeferred();
            _textureManager.CompressDeferred();
        }

        public void MergeStack()
        {
            var MergedStacks = _textureStacks.MergeStacks();

            foreach (var mergeResult in MergedStacks)
            {
                if (mergeResult.FirstTexture == null || mergeResult.MergeTexture == null) continue;
                SetTexture(mergeResult.FirstTexture, mergeResult.MergeTexture);
                TransferAsset(mergeResult.MergeTexture);
            }
        }


        public IEnumerable<Renderer> EnumerateRenderer() { return _renderers; }
        public bool IsPreview() => Previewing;
        public ITextureManager GetTextureManager() => _textureManager;

        public RenderersSubDomain GetSubDomain(List<Renderer> subDomainRenderers)
        {
            var subIDomain = new RenderersSubDomain(this, subDomainRenderers);
            return subIDomain;
        }



        internal class RenderersSubDomain : AbstractSubDomain<RenderersDomain>
        {
            ImmediateStackManager _textureStacks;
            public RenderersSubDomain(RenderersDomain domain, IEnumerable<Renderer> subDomainRenderers) : base(domain, subDomainRenderers)
            {
                _textureStacks = new ImmediateStackManager(domain.GetTexTransCoreEngineForUnity(), domain.GetTextureManager());
            }
            public override void AddTextureStack(Texture dist, ITTRenderTexture addTex, ITTBlendKey blendKey)
            { _textureStacks.AddTextureStack(dist as Texture2D, addTex, blendKey); }


            public override void ReplaceMaterials(Dictionary<Material, Material> mapping, bool one2one = true)
            {
                foreach (var r in _subDomainsRenderer)
                    ReplaceMaterial(r, mapping, _rootDomain.Previewing);

                if (one2one) { foreach (var keyValuePair in mapping) { RegisterReplace(keyValuePair.Key, keyValuePair.Value); } }
                this.transferAssets(mapping.Values);
            }
            public override void SetMesh(Renderer renderer, Mesh mesh)
            {
                var preMesh = ReplaceMesh(renderer, mesh, _rootDomain.Previewing);
                RegisterReplace(preMesh, mesh);
            }

            public override void MergeStack()
            {
                var MergedStacks = _textureStacks.MergeStacks();

                foreach (var mergeResult in MergedStacks)
                {
                    if (mergeResult.FirstTexture == null || mergeResult.MergeTexture == null) continue;
                    SetTexture(mergeResult.FirstTexture, mergeResult.MergeTexture);
                    TransferAsset(mergeResult.MergeTexture);
                }
                void SetTexture(Texture2D target, Texture2D setTex)
                {
                    var mats = RendererUtility.GetFilteredMaterials(_subDomainsRenderer);
                    ReplaceMaterials(MaterialUtility.ReplaceTextureAll(mats, target, setTex));
                    RegisterReplace(target, setTex);
                }
            }
        }

    }
    internal abstract class AbstractSubDomain<RootDomain> : IDomain
    where RootDomain : IDomain
    {
        protected RootDomain _rootDomain;
        protected IEnumerable<Renderer> _subDomainsRenderer;

        public AbstractSubDomain(RootDomain rootDomain, IEnumerable<Renderer> subDomainsRenderer)
        {
            _rootDomain = rootDomain;
            _subDomainsRenderer = subDomainsRenderer;
        }
        public IEnumerable<Renderer> EnumerateRenderer() => _subDomainsRenderer;

        public ITexTransToolForUnity GetTexTransCoreEngineForUnity() => _rootDomain.GetTexTransCoreEngineForUnity();

        public ITextureManager GetTextureManager() => _rootDomain.GetTextureManager();
        public bool IsPreview() => _rootDomain.IsPreview();
        public bool OriginEqual(Object l, Object r) => _rootDomain.OriginEqual(l, r);
        public void RegisterReplace(Object oldObject, Object nowObject) => _rootDomain.RegisterReplace(oldObject, nowObject);
        public void TransferAsset(Object asset) => _rootDomain.TransferAsset(asset);

        public abstract void AddTextureStack(Texture dist, ITTRenderTexture addTex, ITTBlendKey blendKey);
        public abstract void ReplaceMaterials(Dictionary<Material, Material> mapping, bool one2one = true);

        public abstract void SetMesh(Renderer renderer, Mesh mesh);
        public abstract void MergeStack();

    }
}
