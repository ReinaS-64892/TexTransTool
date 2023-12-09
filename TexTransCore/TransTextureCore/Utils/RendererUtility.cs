using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;

namespace net.rs64.TexTransCore.TransTextureCore.Utils
{
    internal static class RendererUtility
    {
        /// <summary>
        /// マテリアルをとりあえず集めてくる。同一物を消したりなどしない。
        /// </summary>
        /// <param name="Renderers"></param>
        /// <returns></returns>
        public static List<Material> GetMaterials(IEnumerable<Renderer> Renderers, List<Material> output = null)
        {
            output?.Clear(); output ??= new();
            foreach (var renderer in Renderers)
            {
                output.AddRange(renderer.sharedMaterials);
            }
            return output;
        }
        public static List<Material> GetFilteredMaterials(IEnumerable<Renderer> Renderers, List<Material> output = null)
        {
            output?.Clear(); output ??= new();

            var tempList = ListPool<Material>.Get();
            output.AddRange(GetMaterials(Renderers, tempList).Distinct().Where(I => I != null));

            ListPool<Material>.Release(tempList);
            return output;
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