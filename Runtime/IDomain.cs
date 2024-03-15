using System.Collections.Generic;
using net.rs64.TexTransCore.Island;
using net.rs64.TexTransCore.TransTextureCore.Utils;
using net.rs64.TexTransTool.MultiLayerImage;
using UnityEngine;
using static net.rs64.TexTransCore.BlendTexture.TextureBlend;

namespace net.rs64.TexTransTool
{

    internal interface IDomain : IAssetSaver, IProgressHandling
    {
        void ReplaceMaterials(Dictionary<Material, Material> mapping, bool rendererOnly = false);
        void SetMesh(Renderer renderer, Mesh mesh);
        public void AddTextureStack<BlendTex>(Texture2D dist, BlendTex setTex) where BlendTex : IBlendTexturePair;//RenderTextureを入れる場合 Temp にすること、そしてこちら側でそれが解放される。
        bool TryReplaceQuery(UnityEngine.Object oldObject, out UnityEngine.Object nowObject);
        //今後テクスチャとメッシュとマテリアル以外で置き換えが必要になった時できるようにするために用意はしておく
        void RegisterReplace(UnityEngine.Object oldObject, UnityEngine.Object nowObject);

        bool IsPreview();//極力使わない方針で、どうしようもないやつだけ使うこと。テクスチャとかはプレビューの場合は自動で切り替わるから、これを見るコードをできるだけ作りたくないという意図です。

        ITextureManager GetTextureManager();
    }
    internal interface IAssetSaver
    {
        void TransferAsset(UnityEngine.Object asset);
    }
    internal interface IProgressHandling
    {
        void ProgressStateEnter(string enterName);
        void ProgressUpdate(string state, float value);
        void ProgressStateExit();
        void ProgressFinalize();
    }

    internal interface ITextureManager : IOriginTexture
    {
        void DeferDestroyTexture2D(Texture2D texture2D);
        void DestroyTextures();

        void TextureCompressDelegation((TextureFormat CompressFormat, int Quality) compressFormat, Texture2D target);
        void ReplaceTextureCompressDelegation(Texture2D souse, Texture2D target);
        void TextureFinalize();
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
            origin.WriteOriginalTexture(texture2D, tempRt);
            return tempRt;
        }
        public static RenderTexture GetOriginTempRt(this IOriginTexture origin, Texture2D texture2D, int size)
        {
            var tempRt = RenderTexture.GetTemporary(size, size, 0);
            tempRt.Clear();
            origin.WriteOriginalTexture(texture2D, tempRt);
            return tempRt;
        }
    }
}
