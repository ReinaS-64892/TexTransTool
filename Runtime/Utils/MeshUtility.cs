using System;
using System.Collections.Generic;
using net.rs64.TexTransCore;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Rendering;

namespace net.rs64.TexTransTool.Utils
{
    internal static class MeshUtility
    {
        public static void ConvertIndicesToTriangles(List<int> indices, List<TriangleIndex> triangles)
        {
            triangles.Clear();
            int count = 0;
            while (indices.Count > count)
            {
                triangles.Add(new TriangleIndex(indices[count++], indices[count++], indices[count++]));
            }
        }
        public static List<TriangleIndex> GetSubMeshTriangleIndices(this Mesh mesh, int SubMesh, List<TriangleIndex> output = null)
        {
            var triList = ListPool<int>.Get();
            mesh.GetTriangles(triList, SubMesh);
            ConvertIndicesToTriangles(triList, output);

            ListPool<int>.Release(triList);
            return output;
        }
        public static bool HasUV(this Mesh mesh, int channel = 0)
        {
            if (channel < 0 || channel > 7) { throw new IndexOutOfRangeException(); }
            return mesh.HasVertexAttribute((VertexAttribute.TexCoord0 + channel));
        }

        public static NativeArray<TriangleIndex> GetSubMeshTriangleIndices(Mesh.MeshData mainMesh, int subMeshIndex, Allocator allocator = Allocator.TempJob)
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
