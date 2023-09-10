#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace net.rs64.TexTransTool.Utils
{
    public static class RendererUtility
    {
        /// <summary>
        /// マテリアルをとりあえず集めてくる。同一物を消したりなどしない。
        /// </summary>
        /// <param name="Renderers"></param>
        /// <returns></returns>
        public static List<Material> GetMaterials(IEnumerable<Renderer> Renderers)
        {
            List<Material> matList = new List<Material>();
            foreach (var renderer in Renderers)
            {
                matList.AddRange(renderer.sharedMaterials);
            }
            return matList;
        }
        public static List<Material> GetFilteredMaterials(IEnumerable<Renderer> Renderers)
        {
            return GetMaterials(Renderers).Distinct().Where(I => I != null).ToList();
        }

        /// <summary>
        /// レンダラーを捜索して、ターゲットのテクスチャをSetに差し替えたマテリアルを生成する。
        /// </summary>
        /// <param name="Renderers"></param>
        /// <param name="Target"></param>
        /// <param name="SetTex"></param>
        /// <returns>差し替え元と差し替え先のペア</returns>
        public static Dictionary<Material, Material> SetTexture(IEnumerable<Renderer> Renderers, Texture2D Target, Texture2D SetTex)
        {
            var mats = GetFilteredMaterials(Renderers);
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

        public static Mesh GetMesh(this Renderer Target)
        {
            Mesh mesh = null;
            switch (Target)
            {
                case SkinnedMeshRenderer SMR:
                    {
                        mesh = SMR.sharedMesh;
                        break;
                    }
                case MeshRenderer MR:
                    {
                        mesh = MR.GetComponent<MeshFilter>().sharedMesh;
                        break;
                    }
                default:
                    break;
            }
            return mesh;
        }
    }
}
#endif
