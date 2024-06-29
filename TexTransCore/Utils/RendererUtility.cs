using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;

namespace net.rs64.TexTransCore.Utils
{
    internal static class RendererUtility
    {
        /// <summary>
        /// マテリアルをとりあえず集めてくる。同一物を消したりなどしない。
        /// </summary>
        /// <param name="renderers"></param>
        /// <returns></returns>
        public static List<Material> GetMaterials(IEnumerable<Renderer> renderers, List<Material> output = null)
        {
            output?.Clear(); output ??= new();
            foreach (var renderer in renderers)
            {
                if (renderer == null) { continue; }
                output.AddRange(renderer.sharedMaterials);
            }
            return output;
        }
        public static List<Material> GetFilteredMaterials(IEnumerable<Renderer> renderers, List<Material> output = null)
        {
            output?.Clear(); output ??= new();

            var tempList = ListPool<Material>.Get();
            output.AddRange(GetMaterials(renderers, tempList).Distinct().Where(I => I != null));

            ListPool<Material>.Release(tempList);
            return output;
        }
        public static Mesh GetMesh(this Renderer target)
        {
            Mesh mesh = null;
            switch (target)
            {
                case SkinnedMeshRenderer SMR:
                    {
                        mesh = SMR.sharedMesh;
                        break;
                    }
                case MeshRenderer MR:
                    {
                        var meshFilter = MR.GetComponent<MeshFilter>();
                        if (meshFilter == null) { break; }
                        mesh = meshFilter.sharedMesh;
                        break;
                    }
                default:
                    break;
            }
            return mesh;
        }
    }
}
