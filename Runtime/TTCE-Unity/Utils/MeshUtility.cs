using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace net.rs64.TexTransCoreEngineForUnity.Utils
{
    internal static class MeshUtility
    {
        public static List<TriangleIndex> ConvertOnList(int[] triangles, List<TriangleIndex> output = null)
        {
            output ??= new();
            int count = 0;
            while (triangles.Length > count)
            {
                output.Add(new TriangleIndex(triangles[count], triangles[count += 1], triangles[count += 1]));
                count += 1;
            }
            return output;
        }
        public static List<TriangleIndex> ConvertOnList(List<int> triangles, List<TriangleIndex> output = null)
        {
            output?.Clear();
            output ??= new();
            int count = 0;
            while (triangles.Count > count)
            {
                output.Add(new TriangleIndex(triangles[count], triangles[count += 1], triangles[count += 1]));
                count += 1;
            }
            return output;
        }
        public static List<TriangleIndex> GetTriangleIndex(this Mesh Mesh, List<TriangleIndex> output = null)
        {
            output?.Clear();
            output ??= new();
            var triList = ListPool<int>.Get();
            var subTriList = ListPool<int>.Get();

            for (var i = 0; Mesh.subMeshCount > i; i += 1)
            {
                Mesh.GetTriangles(subTriList, i);
                triList.AddRange(subTriList);
                subTriList.Clear();
            }
            output = ConvertOnList(triList, output);

            ListPool<int>.Release(triList);
            ListPool<int>.Release(subTriList);

            return output;
        }
        public static List<List<TriangleIndex>> GetSubTriangleIndex(this Mesh mesh)
        {
            var subMeshCount = mesh.subMeshCount;
            List<List<TriangleIndex>> subTriangles = new(subMeshCount);

            for (int i = 0; subMeshCount > i; i++)
            {
                subTriangles.Add(mesh.GetSubTriangleIndex(i));
            }

            return subTriangles;
        }
        public static List<TriangleIndex> GetSubTriangleIndex(this Mesh mesh, int SubMesh, List<TriangleIndex> output = null)
        {
            var triList = ListPool<int>.Get();

            mesh.GetTriangles(triList, SubMesh);
            output = ConvertOnList(triList, output);

            ListPool<int>.Release(triList);
            return output;
        }

        public static List<Vector2> GetUVList(this Mesh mesh, int channel = 0, List<Vector2> uvOutput = null)
        {
            uvOutput?.Clear(); uvOutput ??= new();
            mesh.GetUVs(channel, uvOutput);
            return uvOutput;
        }
    }
}
