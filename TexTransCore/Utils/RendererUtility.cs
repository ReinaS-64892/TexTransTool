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
        public static void SwapMaterials(IEnumerable<Renderer> renderers, Dictionary<Material, Material> matMap) { foreach (var r in renderers) { SwapMaterials(r, matMap); } }
        public static void SwapMaterials(Renderer renderer, Dictionary<Material, Material> matMap)
        {
            if (renderer == null) { return; }
            if (!renderer.sharedMaterials.Any()) { return; }
            renderer.sharedMaterials = renderer.sharedMaterials.Select(i => i != null ? matMap.TryGetValue(i, out var r) ? r : i : i).ToArray();
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
        public static bool SetMesh(this Renderer target, Mesh mesh)
        {
            switch (target)
            {
                case SkinnedMeshRenderer SMR:
                    {
                        SMR.sharedMesh = mesh;
                        return true;
                    }
                case MeshRenderer MR:
                    {
                        MR.GetComponent<MeshFilter>().sharedMesh = mesh;
                        return true;
                    }
                default:
                    return false;
            }
        }
    }
}
