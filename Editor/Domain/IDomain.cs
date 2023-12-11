#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static net.rs64.TexTransCore.BlendTexture.TextureBlend;

namespace net.rs64.TexTransTool
{
    internal interface IAssetSaver
    {
        void TransferAsset(UnityEngine.Object asset);
    }

    internal interface IDomain : IAssetSaver, IProgressHandling, ITextureManager
    {
        /// <summary>
        /// Sets the value to specified property with recording for revert
        /// </summary>
        void SetSerializedProperty(SerializedProperty property, Object value);

        void ReplaceMaterials(Dictionary<Material, Material> mapping, bool rendererOnly = false);
        void SetMesh(Renderer renderer, Mesh mesh);
        void AddTextureStack(Texture2D dist, BlendTexturePair setTex);
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
        void TexCompressDelegationInvoke();
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
#endif