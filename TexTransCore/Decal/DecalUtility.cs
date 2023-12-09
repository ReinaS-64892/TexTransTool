using System.Collections.ObjectModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using net.rs64.TexTransCore.TransTextureCore;
using net.rs64.TexTransCore.TransTextureCore.Utils;
using UnityEngine.Pool;

namespace net.rs64.TexTransCore.Decal
{
    internal static class DecalUtility
    {
        public interface IConvertSpace<UVDimension>
        where UVDimension : struct
        {
            void Input(MeshData meshData);
            List<UVDimension> OutPutUV(List<UVDimension> output = null);
        }
        public interface ITrianglesFilter<SpaceConverter>
        {
            List<TriangleIndex> Filtering(SpaceConverter Space, List<TriangleIndex> Triangles, List<TriangleIndex> output = null);
        }
        public class MeshData
        {
            public readonly List<Vector3> Vertex;
            public readonly List<Vector2> UV;
            public readonly List<List<TriangleIndex>> TrianglesSubMesh;

            public MeshData(List<Vector3> vertex, List<Vector2> uV, List<List<TriangleIndex>> trianglesSubMesh)
            {
                Vertex = vertex;
                UV = uV;
                TrianglesSubMesh = trianglesSubMesh;
            }
        }
        public static Dictionary<Material, Dictionary<string, RenderTexture>> CreateDecalTexture<SpaceConverter, UVDimension>(
            Renderer TargetRenderer,
            Dictionary<Material, Dictionary<string, RenderTexture>> RenderTextures,
            Texture SousTextures,
            SpaceConverter ConvertSpace,
            ITrianglesFilter<SpaceConverter> Filter,
            string TargetPropertyName = "_MainTex",
            TextureWrap? TextureWarp = null,
            float DefaultPadding = 0.5f,
            bool HighQualityPadding = false,
            bool? UseDepthOrInvert = null
        )
        where SpaceConverter : IConvertSpace<UVDimension>
        where UVDimension : struct
        {
            if (RenderTextures == null) RenderTextures = new Dictionary<Material, Dictionary<string, RenderTexture>>();

            var vertices = ListPool<Vector3>.Get(); GetWorldSpaceVertices(TargetRenderer, vertices);
            var targetMesh = TargetRenderer.GetMesh();
            var tUV = ListPool<Vector2>.Get(); targetMesh.GetUVs(0, tUV);
            var trianglesSubMesh = targetMesh.GetPooledSubTriangle();

            ConvertSpace.Input(new MeshData(vertices, tUV, trianglesSubMesh));
            var sUVPooled = ListPool<UVDimension>.Get();
            var sUV = ConvertSpace.OutPutUV(sUVPooled);

            var materials = TargetRenderer.sharedMaterials;

            for (int i = 0; i < trianglesSubMesh.Count; i++)
            {
                var triangle = trianglesSubMesh[i];
                var targetMat = materials[i];

                if (!targetMat.HasProperty(TargetPropertyName)) { continue; };
                var targetTexture = targetMat.GetTexture(TargetPropertyName);
                if (targetTexture == null) { continue; }
                var targetTexSize = new Vector2Int(targetTexture.width, targetTexture.height);

                List<TriangleIndex> filteredTriangle;
                var filteredTrianglePooled = ListPool<TriangleIndex>.Get();
                if (Filter != null) { filteredTriangle = Filter.Filtering(ConvertSpace, triangle, filteredTrianglePooled); }
                else { filteredTriangle = triangle; }

                if (filteredTriangle.Any() == false) { continue; }

                if (!RenderTextures.ContainsKey(targetMat))
                {
                    RenderTextures.Add(targetMat, new());
                }

                if (!RenderTextures[targetMat].ContainsKey(TargetPropertyName))
                {
                    var rendererTexture = new RenderTexture(targetTexSize.x, targetTexSize.y, 32);
                    RenderTextures[targetMat].Add(TargetPropertyName, rendererTexture);
                }

                TransTexture.ForTrans(
                    RenderTextures[targetMat][TargetPropertyName],
                    SousTextures,
                    new TransTexture.TransData<UVDimension>(filteredTriangle, tUV, sUV),
                    DefaultPadding,
                    TextureWarp,
                    HighQualityPadding,
                    UseDepthOrInvert
                );


                ListPool<TriangleIndex>.Release(filteredTrianglePooled);
            }
            ListPool<Vector3>.Release(vertices);
            ListPool<Vector2>.Release(tUV);
            ListPool<UVDimension>.Release(sUVPooled);
            ReleasePooledSubTriangle(trianglesSubMesh);

            return RenderTextures;
        }
        public static List<Vector3> GetWorldSpaceVertices(Renderer Target, List<Vector3> outPut = null)
        {
            outPut?.Clear(); outPut ??= new List<Vector3>();
            switch (Target)
            {
                case SkinnedMeshRenderer SMR:
                    {
                        Mesh mesh = new Mesh();
                        SMR.BakeMesh(mesh);
                        mesh.GetVertices(outPut);
                        Matrix4x4 matrix;
                        if (SMR.bones.Any())
                        {
                            matrix = Matrix4x4.TRS(SMR.transform.position, SMR.transform.rotation, Vector3.one);
                        }
                        else if (SMR.rootBone == null)
                        {
                            matrix = SMR.localToWorldMatrix;
                        }
                        else
                        {
                            matrix = SMR.rootBone.localToWorldMatrix;
                        }
                        ConvertVerticesInMatrix(matrix, outPut, Vector3.zero);
                        break;
                    }
                case MeshRenderer MR:
                    {
                        MR.GetComponent<MeshFilter>().sharedMesh.GetVertices(outPut);
                        ConvertVerticesInMatrix(MR.localToWorldMatrix, outPut, Vector3.zero);
                        break;
                    }
                default:
                    {
                        throw new System.ArgumentException("Rendererが対応したタイプではないか、TargetRendererが存在しません。");
                    }
            }
            return outPut;
        }
        public static List<List<TriangleIndex>> GetPooledSubTriangle(this Mesh mesh)
        {
            var result = ListPool<List<TriangleIndex>>.Get();
            for (var i = 0; mesh.subMeshCount > i; i += 1)
            {
                var subTri = ListPool<TriangleIndex>.Get();
                result.Add(mesh.GetSubTriangleIndex(i, subTri));
            }
            return result;
        }
        public static void ReleasePooledSubTriangle(List<List<TriangleIndex>> subTriList)
        {
            foreach (var subTri in subTriList)
            {
                ListPool<TriangleIndex>.Release(subTri);
            }
            ListPool<List<TriangleIndex>>.Release(subTriList);
        }

        public static List<Vector3> ConvertVerticesInMatrix(Matrix4x4 matrix, IEnumerable<Vector3> Vertices, Vector3 Offset, List<Vector3> outPut = null)
        {
            outPut?.Clear(); outPut ??= new List<Vector3>();
            foreach (var vert in Vertices)
            {
                var pos = matrix.MultiplyPoint3x4(vert) + Offset;
                outPut.Add(pos);
            }
            return outPut;
        }
        public static void ConvertVerticesInMatrix(Matrix4x4 matrix, List<Vector3> Vertices, Vector3 Offset)
        {
            for (int i = 0; i < Vertices.Count; i++)
            {
                Vertices[i] = matrix.MultiplyPoint3x4(Vertices[i]) + Offset;
            }
        }
        public static Vector2 QuadNormalize(IReadOnlyList<Vector2> Quad, Vector2 TargetPos)
        {
            var oneNearPoint = VectorUtility.NearPoint(Quad[0], Quad[2], TargetPos);
            var oneCross = Vector3.Cross(Quad[2] - Quad[0], TargetPos - Quad[0]).z > 0 ? -1 : 1;

            var twoNearPoint = VectorUtility.NearPoint(Quad[0], Quad[1], TargetPos);
            var twoCross = Vector3.Cross(Quad[1] - Quad[0], TargetPos - Quad[0]).z > 0 ? 1 : -1;

            var threeNearPoint = VectorUtility.NearPoint(Quad[1], Quad[3], TargetPos);
            var threeCross = Vector3.Cross(Quad[3] - Quad[1], TargetPos - Quad[1]).z > 0 ? 1 : -1;

            var forNearPoint = VectorUtility.NearPoint(Quad[2], Quad[3], TargetPos);
            var forCross = Vector3.Cross(Quad[3] - Quad[2], TargetPos - Quad[2]).z > 0 ? -1 : 1;

            var oneDistance = Vector2.Distance(oneNearPoint, TargetPos) * oneCross;
            var towDistance = Vector2.Distance(twoNearPoint, TargetPos) * twoCross;
            var threeDistance = Vector2.Distance(threeNearPoint, TargetPos) * threeCross;
            var forDistance = Vector2.Distance(forNearPoint, TargetPos) * forCross;

            var x = oneDistance / (oneDistance + threeDistance);
            var y = towDistance / (towDistance + forDistance);

            return new Vector2(x, y);
        }
        public static List<Vector2> QuadNormalize(IReadOnlyList<Vector2> Quad, List<Vector2> TargetPoss, List<Vector2> outPut = null)
        {
            outPut?.Clear(); outPut ??= new List<Vector2>(TargetPoss.Count);
            foreach (var targetPos in TargetPoss)
            {
                outPut.Add(QuadNormalize(Quad, targetPos));
            }
            return outPut;
        }
    }

    internal enum PolygonCulling
    {
        Vertex,
        Edge,
        EdgeAndCenterRay,
    }
}