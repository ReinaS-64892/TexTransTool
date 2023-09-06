using System.Collections.Generic;
using UnityEngine;

namespace net.rs64.TexTransCore.TransTextureCore.Utils
{
    public static class MeshUtility
    {
        public static List<TriangleIndex> ConvertOnList(int[] triangles)
        {
            var trianglesList = new List<TriangleIndex>();
            int count = 0;
            while (triangles.Length > count)
            {
                trianglesList.Add(new TriangleIndex(triangles[count], triangles[count += 1], triangles[count += 1]));
                count += 1;
            }
            return trianglesList;
        }
        public static List<TriangleIndex> GetTriangleIndex(this Mesh Mesh) => ConvertOnList(Mesh.triangles);
        public static List<List<TriangleIndex>> GetSubTriangleIndex(this Mesh mesh)
        {
            var subMeshCount = mesh.subMeshCount;
            List<List<TriangleIndex>> subTriangles = new List<List<TriangleIndex>>(subMeshCount);

            for (int i = 0; i < subMeshCount; i++)
            {
                subTriangles.Add(mesh.GetSubTriangleIndex(i));
            }
            return subTriangles;
        }
        public static List<TriangleIndex> GetSubTriangleIndex(this Mesh mesh, int SubMesh)
        {
            return ConvertOnList(mesh.GetTriangles(SubMesh));
        }

        public static List<Vector2> GetUVList(this Mesh mesh, int subMesh = 0)
        {
            var uv = new List<Vector2>();
            mesh.GetUVs(subMesh, uv);
            return uv;
        }
    }
}