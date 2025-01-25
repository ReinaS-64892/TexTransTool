#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using net.rs64.TexTransCore;
using net.rs64.TexTransTool.Utils;
using UnityEngine.Profiling;
using net.rs64.TexTransTool.Decal;

namespace net.rs64.TexTransTool
{

    internal interface IDomain : IAssetSaver, IReplaceTracking, IReplaceRegister, ILookingObject, IRendererTargeting
    {
        ITexTransToolForUnity GetTexTransCoreEngineForUnity();
        // TransferAsset と one2one の場合は RegisterReplace を適切に実行するように。
        void ReplaceMaterials(Dictionary<Material, Material> mapping, bool one2one = true);
        void SetMesh(Renderer renderer, Mesh mesh);
        public void AddTextureStack(Texture dist, ITTRenderTexture addTex, ITTBlendKey blendKey);//addTex は借用前提
        ITextureManager GetTextureManager();

        void GetMutable(ref Material material)
        {
            if (IsTemporaryAsset(material)) { return; }
            Profiler.BeginSample("GetMutable-with-Clone", material);
            var origin = material;

            var mutableMat = UnityEngine.Object.Instantiate(origin);
#if UNITY_EDITOR
            mutableMat.parent = null;
#endif
            ReplaceMaterials(new() { { origin, mutableMat } }, true);
            // ReplaceMaterials が基本これらを実行するから必要がない。
            // TransferAsset(mutableMat);
            // RegisterReplace(origin, mutableMat);
            mutableMat.name = origin.name + "(TTT GetMutable)";

            material = mutableMat;
            Profiler.EndSample();
        }
        bool IsPreview();//極力使わない方針で、どうしようもないやつだけ使うこと。テクスチャとかはプレビューの場合は自動で切り替わるから、これを見るコードをできるだけ作りたくないという意図です。
    }
    internal interface IAssetSaver
    {
        bool IsTemporaryAsset(UnityEngine.Object asset) { return false; }
        void TransferAsset(UnityEngine.Object asset);
    }
    internal interface IReplaceTracking
    {
        bool OriginEqual(UnityEngine.Object l, UnityEngine.Object r);
    }
    public delegate bool OriginEqual(UnityEngine.Object l, UnityEngine.Object r);
    internal interface IReplaceRegister
    {
        //今後テクスチャとメッシュとマテリアル以外で置き換えが必要になった時できるようにするために用意はしておく
        void RegisterReplace(UnityEngine.Object oldObject, UnityEngine.Object nowObject);
    }
    internal interface IRendererTargeting : IReplaceTracking
    {
        IEnumerable<Renderer> EnumerateRenderer();
        Material?[] GetMaterials(Renderer renderer) { return renderer.sharedMaterials; }
        MeshData GetMeshData(Renderer renderer) { return renderer.GetToMemorizedMeshData(); }
        HashSet<Material> GetAllMaterials()
        {
            var matHash = new HashSet<Material>();
            foreach (var r in EnumerateRenderer()) { matHash.UnionWith(GetMaterials(r).Where(m => m != null).Cast<Material>()); }
            return matHash;
        }
        HashSet<Texture> GetAllTextures()
        {
            var texHash = new HashSet<Texture>();
            var mats = GetAllMaterials();
            foreach (var m in mats) { texHash.UnionWith(m.GetAllTexture<Texture>()); }
            return texHash;
        }
    }
    internal interface IAffectingRendererTargeting : IRendererTargeting, IReplaceRegister
    {
        // おおもとの IRendererTargeting.GetMaterials を隠すような形で実装すること (これなんかいい形で明示したいのだけど ... 方法がわからん)
        // Material[] GetMaterials(Renderer renderer) { return GetMutableMaterials(renderer); }
        Material?[] GetMutableMaterials(Renderer renderer);
    }
    public interface ILookingObject
    {
        void LookAt(UnityEngine.Object obj) { }
        void LookAtChildeComponents<LookTargetComponent>(GameObject gameObject) where LookTargetComponent : Component { }
    }
    internal interface ITextureManager : IOriginTexture, IDeferredDestroyTexture, IDeferTextureCompress { }
    public interface IDeferredDestroyTexture
    {
        void DeferredDestroyOf(Texture2D texture2D);
        void DestroyDeferred();
    }
    public interface IOriginTexture
    {
        int GetOriginalTextureSize(Texture2D texture2D);
        void WriteOriginalTexture(Texture2D texture2D, RenderTexture writeTarget);
        void PreloadOriginalTexture(Texture2D texture2D);


        (int x, int y) PreloadAndTextureSizeForTex2D(Texture2D diskTexture);
        void LoadTexture(RenderTexture writeRt, Texture2D diskSource);
        bool IsPreview { get; }
    }
    public interface IDeferTextureCompress
    {
        void DeferredTextureCompress(ITTTextureFormat compressFormat, Texture2D target);
        void DeferredInheritTextureCompress(Texture2D source, Texture2D target);
        void CompressDeferred(IEnumerable<Renderer> renderers, OriginEqual originEqual);
    }

    public interface ITTTextureFormat { public (TextureFormat CompressFormat, int Quality) Get(Texture2D texture2D); }

    internal static class DomainUtility
    {
        public static OriginEqual ObjectEqual = (l, r) => l.Equals(r);
        public static void transferAssets(this IDomain domain, IEnumerable<UnityEngine.Object> unityObjects)
        {
            foreach (var unityObject in unityObjects)
            {
                domain.TransferAsset(unityObject);
            }
        }
        public static IEnumerable<Renderer> GetDomainsRenderers(this IRendererTargeting rendererTargeting, Renderer renderer)
        {
            if (renderer == null) { return Array.Empty<Renderer>(); }
            return rendererTargeting.EnumerateRenderer().Where(Contains);
            bool Contains(Renderer dr) { return rendererTargeting.OriginEqual(dr, renderer); }
        }
        public static IEnumerable<Renderer> GetDomainsRenderers(this IRendererTargeting rendererTargeting, IEnumerable<Renderer> renderers)
        {
            if (renderers.Any() is false) { return Array.Empty<Renderer>(); }
            return rendererTargeting.EnumerateRenderer().Where(Contains);
            bool Contains(Renderer dr) { return renderers.Any(sr => rendererTargeting.OriginEqual(dr, sr)); }
        }
        public static IEnumerable<Renderer> GetDomainsRenderers(this OriginEqual originEqual, IEnumerable<Renderer> domainRenderers, IEnumerable<Renderer> renderers)
        {
            if (renderers.Any() is false) { return Array.Empty<Renderer>(); }
            return domainRenderers.Where(Contains);
            bool Contains(Renderer dr) { return renderers.Any(sr => originEqual(dr, sr)); }
        }


        public static IEnumerable<Renderer> RendererFilterForMaterial(this IRendererTargeting rendererTargeting, IEnumerable<Material> material)
        {
            if (material.Any() is false) { return Array.Empty<Renderer>(); }
            var matHash = rendererTargeting.GetDomainsMaterialsHashSet(material);
            return rendererTargeting.EnumerateRenderer().Where(i => rendererTargeting.GetMaterials(i).Any(m => m != null ? matHash.Contains(m) : false));
        }
        public static IEnumerable<Renderer> RendererFilterForMaterial(this IRendererTargeting rendererTargeting, Material? material)
        {
            if (material == null) { return Array.Empty<Renderer>(); }
            var matHash = rendererTargeting.GetDomainsMaterialsHashSet(material);
            return rendererTargeting.EnumerateRenderer().Where(i => rendererTargeting.GetMaterials(i).Any(m => m != null ? matHash.Contains(m) : false));
        }

        public static HashSet<Material> GetDomainsMaterialsHashSet(this IRendererTargeting rendererTargeting, IEnumerable<Material> material)
        {
            if (material.Any() is false) { return new(); }
            return rendererTargeting.GetAllMaterials().Where(m => material.Any(tm => rendererTargeting.OriginEqual(m, tm))).ToHashSet();
        }
        public static HashSet<Material> GetDomainsMaterialsHashSet(this OriginEqual originEqual, IEnumerable<Renderer> domainRenderers, IEnumerable<Material> material)
        {
            if (material.Any() is false) { return new(); }
            return RendererUtility.GetFilteredMaterials(domainRenderers).Where(m => material.Any(tm => originEqual(m, tm))).ToHashSet();
        }
        public static HashSet<Material> GetDomainsMaterialsHashSet(this IRendererTargeting rendererTargeting, Material? material)
        {
            if (material == null) { return new(); }
            return rendererTargeting.GetAllMaterials().Where(m => rendererTargeting.OriginEqual(m, material)).ToHashSet();
        }



        public static void LookAt(this ILookingObject domain, IEnumerable<UnityEngine.Object> objs) { foreach (var obj in objs) { domain.LookAt(obj); } }

        public static void ReplaceTexture<Tex>(this IDomain domain, Tex target, Tex setTex)
        where Tex : Texture
        {
            var mats = RendererUtility.GetFilteredMaterials(domain.EnumerateRenderer());

            foreach (var m in mats)
            {
                if (m.ContainsTexture(target) is false) { continue; }

                var mutableMat = m;
                domain.GetMutable(ref mutableMat);
                mutableMat.ReplaceTextureInPlace(target, setTex);
            }
            domain.RegisterReplace(target, setTex);
        }

    }

}
