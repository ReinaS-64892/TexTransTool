using System.Collections.Generic;
using net.rs64.TexTransCore.Island;
using UnityEngine;
using static net.rs64.TexTransCore.BlendTexture.TextureBlend;

namespace net.rs64.TexTransTool
{

    internal interface IDomain : IAssetSaver, IProgressHandling
    {

        void ReplaceMaterials(Dictionary<Material, Material> mapping, bool rendererOnly = false);
        void SetMesh(Renderer renderer, Mesh mesh);
        void AddTextureStack(Texture2D dist, BlendTexturePair setTex);//RenderTextureを入れる場合 Temp にすること
        bool TryReplaceQuery(UnityEngine.Object oldObject, out UnityEngine.Object nowObject);
        //今後テクスチャとメッシュとマテリアル以外で置き換えが必要になった時できるようにするために用意はしておく
        void RegisterReplace(UnityEngine.Object oldObject, UnityEngine.Object nowObject);


        ITextureManager GetTextureManager();
        IIslandCache GetIslandCacheManager();
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

    internal interface ITextureManager : IGetOriginTex2DManager
    {
        void DeferDestroyTexture2D(Texture2D texture2D);
        void DeferTexDestroy();

        void TextureCompressDelegation((TextureFormat CompressFormat, int Quality) compressFormat, Texture2D target);
        void ReplaceTextureCompressDelegation(Texture2D souse, Texture2D target);
        void TextureFinalize();
    }
    public interface IGetOriginTex2DManager
    {
        Texture2D GetOriginalTexture2D(Texture2D texture2D);
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
    }
}