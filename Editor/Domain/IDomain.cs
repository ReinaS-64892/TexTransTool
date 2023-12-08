#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static net.rs64.TexTransCore.BlendTexture.TextureBlend;

namespace net.rs64.TexTransTool
{
    internal interface IAssetSaver
    {
        void TransferAsset(UnityEngine.Object Asset);
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
        void ProgressStateEnter(string EnterName);
        void ProgressUpdate(string State, float Value);
        void ProgressStateExit();
        void ProgressFinalize();
    }

    internal interface ITextureManager
    {
        Texture2D GetOriginalTexture2D(Texture2D texture2D);
        void DeferDestroyTexture2D(Texture2D texture2D);
        void DeferTexDestroy();

        void TextureCompressDelegation((TextureFormat CompressFormat, int Quality) CompressFormat, Texture2D Target);
        void ReplaceTextureCompressDelegation(Texture2D Souse, Texture2D Target);
        void TexCompressDelegationInvoke();
    }

    internal static class DomainUtility
    {
        public static void transferAssets(this IDomain domain, IEnumerable<UnityEngine.Object> UnityObjects)
        {
            foreach (var unityObject in UnityObjects)
            {
                domain.TransferAsset(unityObject);
            }
        }
    }
}
#endif