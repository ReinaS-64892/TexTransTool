#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using net.rs64.TexTransCore;
using net.rs64.TexTransTool.Utils;
using UnityEngine.Profiling;
using net.rs64.TexTransTool.Decal;
using System.Collections;

namespace net.rs64.TexTransTool
{

    internal interface IDomain : IAssetSaver, IOriginalObjectEqualityComparer, IReplacementRegistry, ILookingObject, IRendererTargeting, ITexturePostProcessor, IDomainCustomContext
    {
        ITexTransToolForUnity GetTexTransCoreEngineForUnity();
        // 原則 RegisterReplace や TransferAssets は勝手に行わないこと
        void ReplaceMaterials(Dictionary<Material, Material> mapping);

        // Mesh の Transfer Assets や RegisterReplace は行われない
        void SetMesh(Renderer renderer, Mesh mesh);
        public void AddTextureStack(Texture dist, ITTRenderTexture addTex, ITTBlendKey blendKey);//addTex は借用前提で、保持しておきたいなら Clone すること
        Material ToMutable(Material material)
        {
            if (IsTemporaryAsset(material)) { return material; }
            Profiler.BeginSample("GetMutable-with-Clone", material);
            var original = material;

            var mutableMat = UnityEngine.Object.Instantiate(original);
#if UNITY_EDITOR
            mutableMat.parent = null;
#endif
            ReplaceMaterials(new() { { original, mutableMat } });

            RegisterReplacement(original, mutableMat);
            TransferAsset(mutableMat);

            mutableMat.name = original.name + "(TTT GetMutable)";

            Profiler.EndSample();
            return mutableMat;
        }
    }
    internal interface IAssetSaver
    {
        bool IsTemporaryAsset(UnityEngine.Object asset) { return false; }
        void TransferAsset(UnityEngine.Object asset);
    }
    internal delegate bool UnityObjectEqualityComparison(UnityEngine.Object? l, UnityEngine.Object? r);
    internal interface IOriginalObjectEqualityComparer
    {
        bool OriginalObjectEquals(UnityEngine.Object? l, UnityEngine.Object? r);
    }
    internal interface IReplacementRegistry
    {
        //今後テクスチャとメッシュとマテリアル以外で置き換えが必要になった時できるようにするために用意はしておく
        void RegisterReplacement(UnityEngine.Object oldObject, UnityEngine.Object nowObject);
    }
    internal interface IRendererTargeting : IOriginalObjectEqualityComparer, ILookingObject, IActiveness
    {
        // ParticleSystem などのものも入りうる。 (Static or Skinned)メッシュを持つものだけではないので注意。
        IEnumerable<Renderer> EnumerateRenderers();
        Material?[] GetMaterials(Renderer renderer)
        {
            return LookAtGet(renderer, GetSharedMaterials, (l, r) => l.SequenceEqual(r));
            Material?[] GetSharedMaterials(Renderer r) { return renderer.sharedMaterials; }
        }
        MeshData GetMeshData(Renderer renderer) { return renderer.GetToMemorizedMeshData(); }
        Mesh? GetMesh(Renderer renderer) { return renderer.GetMesh(); }

        // ここで得られる Material は Renderer に存在するもの以外も入りうる
        HashSet<Material> GetAllMaterials()
        {
            var matHash = new HashSet<Material>();
            foreach (var r in EnumerateRenderers()) { matHash.UnionWith(GetMaterials(r).SkipDestroyed()); }
            return matHash;
        }
        HashSet<Texture> GetAllTextures()
        {
            var texHash = new HashSet<Texture>();
            var mats = GetAllMaterials();
            foreach (var m in mats) { texHash.UnionWith(m.EnumerateTextures(GetShader, GetTex)); }
            return texHash;
            Shader GetShader(Material mat) { return LookAtGet(mat, i => i.shader); }
            Texture GetTex(Material mat, int nameID) { return LookAtGet(mat, i => i.GetTexture(nameID)); }
        }
        HashSet<Texture> GetMaterialTextures(Material mat)
        {
            return mat.EnumerateReferencedTextures().ToHashSet();
        }
    }
    internal interface IAffectingRendererTargeting : IRendererTargeting, IReplacementRegistry
    {
        // おおもとの IRendererTargeting.GetMaterials を隠すような形で実装すること (これなんかいい形で明示したいのだけど ... 方法がわからん)
        // Material[] GetMaterials(Renderer renderer) { return GetMutableMaterials(renderer); }
        Material?[] GetMutableMaterials(Renderer renderer);
    }
    internal interface ILookingObject
    {
        void LookAt(UnityEngine.Object obj) { }
        /// <summary>
        /// これで取得した戻り値は基本的に、編集してはならない。 ReadOnly
        /// 編集したい場合は ToArray など複製すること。
        /// </summary>
        TOut LookAtGet<TObj, TOut>(TObj obj, Func<TObj, TOut> getAction, Func<TOut, TOut, bool>? comp = null)
        where TObj : UnityEngine.Object
        {
            return getAction(obj);
        }
        void LookAtGetComponent<LookTargetComponent>(GameObject gameObject) where LookTargetComponent : Component { }
        void LookAtChildeComponents<LookTargetComponent>(GameObject gameObject) where LookTargetComponent : Component { }
    }
    internal interface IActiveness
    {
        bool IsActive(GameObject gameObject)
        {
            return TexTransBehaviorSearch.CheckIsActive(gameObject);
        }
        bool IsEnable(Component component)
        {
            return component switch
            {
                Behaviour bh => bh.enabled,
                Renderer bh => bh.enabled,
                _ => true,
            };
        }
    }
    internal interface ITexturePostProcessor
    {
        // ここ渡す Descriptor は複製したものを渡すように、それと
        TexTransToolTextureDescriptor GetTextureDescriptor(Texture texture);
        // ここで渡される rt は基本的に借用ではなく move のような形で Dispose する役割を受け取る
        void RegisterPostProcessingAndLazyGPUReadBack(ITTRenderTexture rt, TexTransToolTextureDescriptor textureDescriptor);
    }

    internal interface ITexTransToolTextureFormat { public (TextureFormat CompressFormat, int Quality) Get(Texture2D texture2D); }

    internal class TexTransToolTextureDescriptor
    {
        public bool UseMipMap = true;
        public string MipMapGenerationAlgorithm = ITexTransToolForUnity.DS_ALGORITHM_DEFAULT;
        public ITexTransToolTextureFormat TextureFormat = new TextureCompressionData();
        public bool AsLinear = false;
        public bool IsNormalMap = false;// このテクスチャが確定で NormalMap であることを保証しない。 (AtlasTexture などで発生する)

        public FilterMode filterMode = FilterMode.Bilinear;
        public int anisoLevel = 1;
        public float mipMapBias = 0;
        public TextureWrapMode wrapModeU = TextureWrapMode.Repeat;
        public TextureWrapMode wrapModeV = TextureWrapMode.Repeat;
        public TextureWrapMode wrapModeW = TextureWrapMode.Repeat;
        public TextureWrapMode wrapMode = TextureWrapMode.Repeat;

        public TexTransToolTextureDescriptor() { }
        public TexTransToolTextureDescriptor(TexTransToolTextureDescriptor source) { CopyFrom(source); }
        public void CopyFrom(TexTransToolTextureDescriptor descriptor)
        {
            UseMipMap = descriptor.UseMipMap;
            MipMapGenerationAlgorithm = descriptor.MipMapGenerationAlgorithm;
            TextureFormat = descriptor.TextureFormat;
            AsLinear = descriptor.AsLinear;
            IsNormalMap = descriptor.IsNormalMap;
            filterMode = descriptor.filterMode;
            anisoLevel = descriptor.anisoLevel;
            mipMapBias = descriptor.mipMapBias;
            wrapModeU = descriptor.wrapModeU;
            wrapModeV = descriptor.wrapModeV;
            wrapModeW = descriptor.wrapModeW;
            wrapMode = descriptor.wrapMode;
        }

        internal void WriteFillWarp(Texture2D tex2D)
        {
            tex2D.filterMode = filterMode;
            tex2D.anisoLevel = anisoLevel;
            tex2D.mipMapBias = mipMapBias;
            tex2D.wrapModeU = wrapModeU;
            tex2D.wrapModeV = wrapModeV;
            tex2D.wrapModeW = wrapModeW;
            tex2D.wrapMode = wrapMode;
        }
    }
    internal interface IDomainCustomContext
    {
        T? GetCustomContext<T>() where T : class, IDomainCustomContextData;
    }
    internal interface IDomainCustomContextData
    {

    }

    internal class DomainPreviewCtx : IDomainCustomContextData
    {
        public bool IsPreview { get; private set; }
        public DomainPreviewCtx(bool isPreview)
        {
            IsPreview = isPreview;
        }
    }
    internal class GenericReplaceRegistry : IReplacementRegistry, IOriginalObjectEqualityComparer
    {
        Dictionary<UnityEngine.Object, UnityEngine.Object> _replaceMap = new();//New Old
        public IReadOnlyDictionary<UnityEngine.Object, UnityEngine.Object> ReplaceMap => _replaceMap;

        public virtual bool OriginalObjectEquals(UnityEngine.Object? l, UnityEngine.Object? r)
        {
            if (l == null || r == null) { return l == r; }
            if (l == r) { return true; }
            var originL = _replaceMap.TryGetValue(l, out var oL) ? oL : l;
            var originR = _replaceMap.TryGetValue(r, out var oR) ? oR : r;
            return originL == originR;
        }

        public virtual void RegisterReplacement(UnityEngine.Object oldObject, UnityEngine.Object nowObject)
        {
            var originL = _replaceMap.TryGetValue(oldObject, out var oObj) ? oObj : oldObject;
            _replaceMap[nowObject] = originL;
        }
        public virtual KeyValuePair<UnityEngine.Object, UnityEngine.Object>? ReplacePooledRenderTexture(RenderTexture pooledRt, UnityEngine.Object nowObject)
        {
            if (_replaceMap.Remove(pooledRt, out var origin) is false) { return null; }
            return new(origin, nowObject);
        }
    }
    internal static class DomainUtility
    {
        public static UnityObjectEqualityComparison ObjectEqual = (l, r) =>
        {
            if (l == null) { return r == null; }
            return l.Equals(r);
        };
        public static void TransferAssets(this IDomain domain, IEnumerable<UnityEngine.Object> unityObjects)
        {
            foreach (var unityObject in unityObjects)
            {
                domain.TransferAsset(unityObject);
            }
        }
        public static void RegisterReplaces<TOld, TNew>(this IReplacementRegistry domain, IEnumerable<KeyValuePair<TOld, TNew>> unityObjects)
        where TOld : UnityEngine.Object
        where TNew : UnityEngine.Object
        {
            foreach (var unityObject in unityObjects)
                domain.RegisterReplacement(unityObject.Key, unityObject.Value);
        }
        public static IEnumerable<Renderer> GetDomainsRenderers(this IRendererTargeting rendererTargeting, Renderer renderer)
        {
            if (renderer == null) { return Array.Empty<Renderer>(); }
            return rendererTargeting.EnumerateRenderers().Where(Contains);
            bool Contains(Renderer dr) { return rendererTargeting.OriginalObjectEquals(dr, renderer); }
        }
        public static IEnumerable<Renderer> GetDomainsRenderers(this IRendererTargeting rendererTargeting, IEnumerable<Renderer> renderers)
        {
            if (renderers.Any() is false) { return Array.Empty<Renderer>(); }
            return rendererTargeting.EnumerateRenderers().Where(Contains);
            bool Contains(Renderer dr) { return renderers.Any(sr => rendererTargeting.OriginalObjectEquals(dr, sr)); }
        }
        public static IEnumerable<Renderer> GetDomainsRenderers(this UnityObjectEqualityComparison originEqual, IEnumerable<Renderer> domainRenderers, IEnumerable<Renderer> renderers)
        {
            if (renderers.Any() is false) { return Array.Empty<Renderer>(); }
            return domainRenderers.Where(Contains);
            bool Contains(Renderer dr) { return renderers.Any(sr => originEqual(dr, sr)); }
        }
        public static IEnumerable<Texture> GetDomainsTextures(this IRendererTargeting rendererTargeting, Texture texture)
        {
            if (texture == null) { return Array.Empty<Texture>(); }
            return rendererTargeting.GetAllTextures().Where(Contains);
            bool Contains(Texture dr) { return rendererTargeting.OriginalObjectEquals(dr, texture); }
        }

        public static IEnumerable<Renderer> RendererFilterForMaterial(this IRendererTargeting rendererTargeting, IEnumerable<Material> material)
        {
            if (material.Any() is false) { return Array.Empty<Renderer>(); }
            var matHash = rendererTargeting.GetDomainsMaterialsHashSet(material);
            return rendererTargeting.EnumerateRenderers().Where(i => rendererTargeting.GetMaterials(i).SkipDestroyed().Any(matHash.Contains));
        }
        public static IEnumerable<Renderer> RendererFilterForMaterialFromDomains(this IRendererTargeting rendererTargeting, HashSet<Material> domainMaterial)
        {
            if (domainMaterial.Any() is false) { return Array.Empty<Renderer>(); }
            return rendererTargeting.EnumerateRenderers().Where(i => rendererTargeting.GetMaterials(i).SkipDestroyed().Any(domainMaterial.Contains));
        }
        public static IEnumerable<Renderer> RendererFilterForMaterial(this IRendererTargeting rendererTargeting, Material? material)
        {
            if (material == null) { return Array.Empty<Renderer>(); }
            var matHash = rendererTargeting.GetDomainsMaterialsHashSet(material);
            return rendererTargeting.EnumerateRenderers().Where(i => rendererTargeting.GetMaterials(i).SkipDestroyed().Any(matHash.Contains));
        }

        public static HashSet<Material> GetDomainsMaterialsHashSet(this IRendererTargeting rendererTargeting, IEnumerable<Material> material)
        {
            if (material.Any() is false) { return new(); }
            return rendererTargeting.GetAllMaterials().Where(m => material.Any(tm => rendererTargeting.OriginalObjectEquals(m, tm))).ToHashSet();
        }
        public static HashSet<Material> GetDomainsMaterialsHashSet(this UnityObjectEqualityComparison originEqual, IEnumerable<Renderer> domainRenderers, IEnumerable<Material> material)
        {
            if (material.Any() is false) { return new(); }
            return RendererUtility.GetFilteredMaterials(domainRenderers).Where(m => material.Any(tm => originEqual(m, tm))).ToHashSet();
        }
        public static HashSet<Material> GetDomainsMaterialsHashSet(this IRendererTargeting rendererTargeting, Material? material)
        {
            if (material == null) { return new(); }
            return rendererTargeting.GetAllMaterials().Where(m => rendererTargeting.OriginalObjectEquals(m, material)).ToHashSet();
        }

        public static Material? GetDomainsMaterial(this UnityObjectEqualityComparison originEqual, IEnumerable<Material> domainsMaterial, Material material)
        {
            return domainsMaterial.FirstOrDefault(m => originEqual(m, material));
        }
        public static HashSet<Material> GetDomainsMaterialsHashSet(this UnityObjectEqualityComparison originEqual, IEnumerable<Material> domainsMaterial, IEnumerable<Material> material)
        {
            if (material.Any() is false) { return new(); }
            return domainsMaterial.Where(m => material.Any(tm => originEqual(m, tm))).ToHashSet();
        }

        public static void LookAt(this ILookingObject domain, IEnumerable<UnityEngine.Object> objs) { foreach (var obj in objs) { domain.LookAt(obj); } }

        public static void ReplaceTexture<TexDist, TexNew>(this IDomain domain, TexDist dist, TexNew newTexture)
        where TexDist : Texture
        where TexNew : Texture
        {
            var mats = domain.GetAllMaterials();

            foreach (var m in mats)
            {
                if (m.ReferencesTexture(dist) is false) { continue; }

                var mutableMat = domain.ToMutable(m);
                mutableMat.ReplaceTexture(dist, newTexture);
            }
        }
    }
    internal static class DestroyedUnityObjectHelpers
    {
        public static T? DestroyedAsNull<T>(this T? obj) where T : notnull, UnityEngine.Object
        {
            return (obj == null) ? null : obj;
        }
        public static IEnumerable<T> SkipDestroyed<T>(this IEnumerable<T?> source)
            where T : notnull, UnityEngine.Object
        {
            return source.Where(item => item != null)!;
        }
        public static IEnumerable<TResult> OfType<TResult>(this IEnumerable<UnityEngine.Object?> source)
            where TResult : notnull, UnityEngine.Object
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            foreach (var item in source)
            {
                if (item is TResult result && item != null)
                {
                    yield return result;
                }
            }
        }
    }
}
