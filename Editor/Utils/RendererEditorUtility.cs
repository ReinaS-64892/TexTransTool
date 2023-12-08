#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransCore.TransTextureCore.Utils;
using UnityEngine;

namespace net.rs64.TexTransTool.Utils
{
    internal static class RendererEditorUtility
    {

        /// <summary>
        /// レンダラーを捜索して、ターゲットのテクスチャをSetに差し替えたマテリアルを生成する。
        /// </summary>
        /// <param name="Renderers"></param>
        /// <param name="Target"></param>
        /// <param name="SetTex"></param>
        /// <returns>差し替え元と差し替え先のペア</returns>
        public static Dictionary<Material, Material> SetTexture(IEnumerable<Renderer> Renderers, Texture2D Target, Texture2D SetTex)
        {
            var mats = RendererUtility.GetFilteredMaterials(Renderers);
            var mapping = new Dictionary<Material, Material>();
            foreach (var mat in mats)
            {
                var Textures = MaterialUtility.FiltalingUnused(MaterialUtility.GetPropAndTextures(mat), mat);

                if (Textures.ContainsValue(Target))
                {
                    var material = Object.Instantiate(mat);

                    foreach (var KVP in Textures)
                    {
                        if (KVP.Value == Target)
                        {
                            material.SetTexture(KVP.Key, SetTex);
                        }
                    }

                    mapping.Add(mat, material);
                }
            }

            return mapping;
        }

    }
}
#endif
