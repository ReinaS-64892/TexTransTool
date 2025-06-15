using System;
using System.Collections.Generic;
using net.rs64.TexTransCore;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Pool;

namespace net.rs64.TexTransTool.Utils
{
    internal static class MeshUtility
    {
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
        public static List<TriangleIndex> GetSubTriangleIndex(this Mesh mesh, int SubMesh, List<TriangleIndex> output = null)
        {
            var triList = ListPool<int>.Get();

            mesh.GetTriangles(triList, SubMesh);
            output = ConvertOnList(triList, output);

            ListPool<int>.Release(triList);
            return output;
        }
        public static bool HasUV(this Mesh mesh, int channel = 0)
        {
            if (channel < 0 || channel > 7) { throw new IndexOutOfRangeException(); }
            return mesh.HasVertexAttribute((UnityEngine.Rendering.VertexAttribute)(channel + 4));
        }

        public static NativeArray<TriangleIndex> GetTriangleIndices(Mesh.MeshData mainMesh, int subMeshIndex, Allocator allocator = Allocator.TempJob)
        {
            System.Diagnostics.Debug.Assert((uint)subMeshIndex < (uint)mainMesh.subMeshCount);
            var desc = mainMesh.GetSubMesh(subMeshIndex);
            System.Diagnostics.Debug.Assert(desc.topology == MeshTopology.Triangles);
            var triangleBuffer = new NativeArray<TriangleIndex>(desc.indexCount / 3, allocator, NativeArrayOptions.UninitializedMemory);
            var indices = triangleBuffer.Reinterpret<int>();
            mainMesh.GetIndices(indices, subMeshIndex);
            return triangleBuffer;
        }
    }

    public enum UVChannel
    {
        UV0 = 0,
        UV1 = 1,
        UV2 = 2,
        UV3 = 3,
        UV4 = 4,
        UV5 = 5,
        UV6 = 6,
        UV7 = 7,
    }
}
