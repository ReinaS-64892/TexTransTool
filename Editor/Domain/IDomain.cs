#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using static net.rs64.TexTransTool.TextureLayerUtil;

namespace net.rs64.TexTransTool
{
    public interface IAssetSaver
    {
        void TransferAsset(UnityEngine.Object Asset);
    }

    public interface IDomain : IAssetSaver
    {
        void SetMaterial(Material target, Material replacement, bool isPaired);
        void SetMesh(Renderer renderer, Mesh mesh);
        void AddTextureStack(Texture2D dist, BlendTextures setTex);
    }

    public static class DomainUtility
    {
        public static void SetMaterials(this IDomain domain, List<MatPair> matPairs, bool isPaired)
        {
            foreach (var matPair in matPairs)
            {
                domain.SetMaterial(matPair.Material, matPair.SecondMaterial, isPaired);
            }
        }
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