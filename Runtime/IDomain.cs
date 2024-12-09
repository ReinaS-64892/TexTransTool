using System;
using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransCoreEngineForUnity;
using UnityEngine;
using net.rs64.TexTransCore;
using net.rs64.TexTransTool.Utils;
using UnityEngine.Profiling;

namespace net.rs64.TexTransTool
{

    internal interface IDomain : IAssetSaver, IReplaceTracking, IReplaceRegister, ILookingObject
    {
        ITexTransToolForUnity GetTexTransCoreEngineForUnity();
        void ReplaceMaterials(Dictionary<Material, Material> mapping, bool one2one = true);
        void SetMesh(Renderer renderer, Mesh mesh);
        public void AddTextureStack(Texture dist, ITTRenderTexture addTex, ITTBlendKey blendKey);//addTex は借用前提
        public IEnumerable<Renderer> EnumerateRenderer();
        ITextureManager GetTextureManager();

        void GetMutable(ref Material material)
        {
            if (IsTemporaryAsset(material)) { return; }
            Profiler.BeginSample("GetMutable-with-Clone", material);
            var origin = material;
            material = UnityEngine.Object.Instantiate(material);
            ReplaceMaterials(new() { { origin, material } }, true);
            TransferAsset(material);
            RegisterReplace(origin, material);
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
        public static void transferAssets(this IDomain domain, IEnumerable<UnityEngine.Object> unityObjects)
        {
            foreach (var unityObject in unityObjects)
            {
                domain.TransferAsset(unityObject);
            }
        }
        public static void AddTextureStack<BlendTex>(this IDomain domain, Texture dist, BlendTex setTex) where BlendTex : TextureBlend.IBlendTexturePair
        {
            var ttce4u = domain.GetTexTransCoreEngineForUnity();
            switch (setTex.Texture)
            {
                case Texture2D texture2D: { break; }
                case RenderTexture renderTexture:
                    {
                        var rt = ttce4u.UploadTexture(renderTexture);
                        ttce4u.LinearToGamma(rt);
                        domain.AddTextureStack(dist, rt, ttce4u.QueryBlendKey(setTex.BlendTypeKey));
                        break;
                    }
            }
        }

        public static RenderTexture GetOriginTempRt(this IOriginTexture origin, Texture2D texture2D)
        {
            var originSize = origin.GetOriginalTextureSize(texture2D);
            var tempRt = TTRt.G(originSize, originSize, true);
            tempRt.name = $"{texture2D.name}:GetOriginTempRt-{tempRt.width}x{tempRt.height}";
            tempRt.CopyFilWrap(texture2D);
            origin.WriteOriginalTexture(texture2D, tempRt);
            return tempRt;
        }

        public static IDisposable GetOriginTempRtU(this IOriginTexture origin, out RenderTexture tempRt, Texture2D texture2D)
        {
            var originSize = origin.GetOriginalTextureSize(texture2D);
            var useVal = TTRt.U(out tempRt, originSize, originSize, true);
            tempRt.name = $"{texture2D.name}:GetOriginTempRtU-{tempRt.width}x{tempRt.height}";
            tempRt.CopyFilWrap(texture2D);
            origin.WriteOriginalTexture(texture2D, tempRt);
            return useVal;
        }
        public static RenderTexture GetOriginTempRt(this IOriginTexture origin, Texture2D texture2D, int size)
        {
            var tempRt = TTRt.G(size, size, true);
            tempRt.name = $"{texture2D.name}:GetOriginTempRtWithSize-{tempRt.width}x{tempRt.height}";
            tempRt.CopyFilWrap(texture2D);
            origin.WriteOriginalTexture(texture2D, tempRt);
            return tempRt;
        }

        public static IDisposable GetOriginTempRt(this IOriginTexture origin, out RenderTexture tempRt, Texture2D texture2D, int size)
        {
            var useVal = TTRt.U(out tempRt, size, size, true);
            tempRt.name = $"{texture2D.name}:GetOriginTempRtWithSizeU-{tempRt.width}x{tempRt.height}";
            tempRt.CopyFilWrap(texture2D);
            origin.WriteOriginalTexture(texture2D, tempRt);
            return useVal;
        }
        public static IEnumerable<Renderer> GetDomainsRenderers(this IDomain domain, IEnumerable<Renderer> renderers)
        {
            return domain.EnumerateRenderer().Where(Contains);
            bool Contains(Renderer dr) { return renderers.Any(sr => domain.OriginEqual(dr, sr)); }
        }
        public static IEnumerable<Renderer> GetDomainsRenderers(this OriginEqual originEqual, IEnumerable<Renderer> domainRenderer, IEnumerable<Renderer> renderers)
        {
            return domainRenderer.Where(Contains);
            bool Contains(Renderer dr) { return renderers.Any(sr => originEqual(dr, sr)); }
        }

        public static IEnumerable<Material> GetDomainMaterials(this IDomain domain, IEnumerable<Material> materials)
        {
            return RendererUtility.GetFilteredMaterials(domain.EnumerateRenderer()).Where(m => Contains(m));
            bool Contains(Material m) { return materials.Any(tm => domain.OriginEqual(m, tm)); }
        }

        public static void LookAt(this ILookingObject domain, IEnumerable<UnityEngine.Object> objs) { foreach (var obj in objs) { domain.LookAt(obj); } }



    }

}
