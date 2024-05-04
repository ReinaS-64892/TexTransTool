using System.Collections.Generic;
using net.rs64.TexTransCore.Island;
using net.rs64.TexTransCore.Utils;
using net.rs64.TexTransTool.MultiLayerImage;
using UnityEngine;
using static net.rs64.TexTransCore.BlendTexture.TextureBlend;

namespace net.rs64.TexTransTool
{

    internal interface IDomain : IAssetSaver, IReplaceTracking
    {
        void ReplaceMaterials(Dictionary<Material, Material> mapping, bool rendererOnly = false);
        void SetMesh(Renderer renderer, Mesh mesh);
        public void AddTextureStack<BlendTex>(Texture dist, BlendTex setTex) where BlendTex : IBlendTexturePair;// TempRenderTexture 想定

        public IEnumerable<Renderer> EnumerateRenderer();

        ITextureManager GetTextureManager();

        bool IsPreview();//極力使わない方針で、どうしようもないやつだけ使うこと。テクスチャとかはプレビューの場合は自動で切り替わるから、これを見るコードをできるだけ作りたくないという意図です。
    }
    internal interface IAssetSaver
    {
        void TransferAsset(UnityEngine.Object asset);
    }

    internal interface ITextureManager : IOriginTexture
    {
        void DeferDestroyOf(Texture2D texture2D);
        void DestroyDeferred();


        void DeferTextureCompress((TextureFormat CompressFormat, int Quality) compressFormat, Texture2D target);
        void DeferInheritTextureCompress(Texture2D source, Texture2D target);
        void CompressDeferred();
    }
    internal interface IReplaceTracking
    {
        bool OriginEqual(UnityEngine.Object l, UnityEngine.Object r);

        //今後テクスチャとメッシュとマテリアル以外で置き換えが必要になった時できるようにするために用意はしておく
        void RegisterReplace(UnityEngine.Object oldObject, UnityEngine.Object nowObject);
    }
    public interface IOriginTexture
    {
        int GetOriginalTextureSize(Texture2D texture2D);
        void WriteOriginalTexture(Texture2D texture2D, RenderTexture writeTarget);
        void WriteOriginalTexture(TTTImportedImage texture, RenderTexture writeTarget);
    }

    internal static class DomainUtility
    {
        public static void transferAssets(this IDomain domain, IEnumerable<UnityEngine.Object> unityObjects)
        {
            foreach (var unityObject in unityObjects)
            {
                domain.TransferAsset(unityObject);
            }
        }

        public static RenderTexture GetOriginTempRt(this IOriginTexture origin, Texture2D texture2D)
        {
            var originSize = origin.GetOriginalTextureSize(texture2D);
            var tempRt = RenderTexture.GetTemporary(originSize, originSize, 0);
            tempRt.CopyFilWrap(texture2D);
            origin.WriteOriginalTexture(texture2D, tempRt);
            return tempRt;
        }
        public static RenderTexture GetOriginTempRt(this IOriginTexture origin, Texture2D texture2D, int size)
        {
            var tempRt = RenderTexture.GetTemporary(size, size, 0);
            tempRt.CopyFilWrap(texture2D);
            tempRt.Clear();
            origin.WriteOriginalTexture(texture2D, tempRt);
            return tempRt;
        }
    }

}
