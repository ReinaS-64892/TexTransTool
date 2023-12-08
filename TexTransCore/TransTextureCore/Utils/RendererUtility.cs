using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace net.rs64.TexTransCore.TransTextureCore.Utils
{
    internal static class RendererUtility
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