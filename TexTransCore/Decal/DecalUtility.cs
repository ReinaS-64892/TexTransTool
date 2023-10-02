using System.Collections.ObjectModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using net.rs64.TexTransCore.TransTextureCore;
using net.rs64.TexTransCore.TransTextureCore.TransCompute;
using net.rs64.TexTransCore.TransTextureCore.Utils;

namespace net.rs64.TexTransCore.Decal
{
    public static class DecalUtility
    {
        public interface IConvertSpace
        {
            void Input(MeshData meshData);
            List<Vector2> OutPutUV();
        }
        public interface ITrianglesFilter<SpaceConverter>
        {
            List<TriangleIndex> Filtering(SpaceConverter Space, List<TriangleIndex> Triangles);
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
        public static Dictionary<Material, Dictionary<string, RenderTexture>> CreateDecalTexture<SpaceConverter>(
            Renderer TargetRenderer,
            Dictionary<Material, Dictionary<string, RenderTexture>> RenderTextures,
            Texture SousTextures,
            SpaceConverter ConvertSpace,
            ITrianglesFilter<SpaceConverter> Filter,
            string TargetPropertyName = "_MainTex",
            TextureWrap TextureWarp = null,
            float DefaultPadding = 0.5f,
            bool HighQualityPadding = false
        )
        where SpaceConverter : IConvertSpace
        {
            if (RenderTextures == null) RenderTextures = new Dictionary<Material, Dictionary<string, RenderTexture>>();

            var vertices = GetWorldSpaceVertices(TargetRenderer);
            var (tUV, trianglesSubMesh) = RendererMeshToGetUVAndTriangle(TargetRenderer);

            ConvertSpace.Input(new MeshData(vertices, tUV, trianglesSubMesh));
            var sUV = ConvertSpace.OutPutUV();

            var materials = TargetRenderer.sharedMaterials;

            for (int i = 0; i < trianglesSubMesh.Count; i++)
            {
                var triangle = trianglesSubMesh[i];
                var targetMat = materials[i];

                if (!targetMat.HasProperty(TargetPropertyName)) { continue; };
                var targetTexture = targetMat.GetTexture(TargetPropertyName);
                if (targetTexture == null) { continue; }
                var targetTexSize = new Vector2Int(targetTexture.width, targetTexture.height);

                var filteredTriangle = Filter != null ? Filter.Filtering(ConvertSpace, triangle) : triangle;
                if (filteredTriangle.Any() == false) { continue; }

                if (!RenderTextures.ContainsKey(targetMat))
                {
                    RenderTextures.Add(targetMat, new Dictionary<string, RenderTexture>());
                }

                if (!RenderTextures[targetMat].ContainsKey(TargetPropertyName))
                {
                    var rendererTexture = new RenderTexture(targetTexSize.x, targetTexSize.y, 32);
                    RenderTextures[targetMat].Add(TargetPropertyName, rendererTexture);
                }

                TransTexture.TransTextureToRenderTexture(
                    RenderTextures[targetMat][TargetPropertyName],
                    SousTextures,
                    new TransTexture.TransData(filteredTriangle, tUV, sUV),
                    DefaultPadding,
                    TextureWarp,
                    HighQualityPadding
                );


            }

            return RenderTextures;
        }
        public static Dictionary<Texture2D, List<TwoDimensionalMap<Color>>> CreateDecalTextureCS<SpaceConverter>(
            TransTextureCompute transTextureCompute,
            Renderer TargetRenderer,
            TwoDimensionalMap<Color> SousTextures,
            SpaceConverter ConvertSpace,
            ITrianglesFilter<SpaceConverter> Filter,
            string TargetPropertyName = "_MainTex",
            TextureWrap TextureWarp = null,
            float DefaultPadding = 1f
        )
        where SpaceConverter : IConvertSpace
        {
            var resultTextures = new Dictionary<Texture2D, List<TwoDimensionalMap<Color>>>();

            var vertices = GetWorldSpaceVertices(TargetRenderer);
            var (tUV, TrianglesSubMesh) = RendererMeshToGetUVAndTriangle(TargetRenderer);

            ConvertSpace.Input(new MeshData(vertices, tUV, TrianglesSubMesh));
            var sUV = ConvertSpace.OutPutUV();

            var materials = TargetRenderer.sharedMaterials;

            for (int i = 0; i < TrianglesSubMesh.Count; i++)
            {
                var triangle = TrianglesSubMesh[i];
                var targetMat = materials[i];

                var targetTexture = targetMat.GetTexture(TargetPropertyName) as Texture2D;
                if (targetTexture == null) { continue; }
                var targetTexSize = new Vector2Int(targetTexture.width, targetTexture.height);

                var filteredTriangle = Filter != null ? Filter.Filtering(ConvertSpace, triangle) : triangle;
                if (filteredTriangle.Any() == false) { continue; }


                var atlasTex = new TwoDimensionalMap<TransColor>(new TransColor(new Color(0, 0, 0, 0), TransTextureCompute.CSPadding(DefaultPadding)), targetTexSize);
                transTextureCompute.TransTextureUseCS(atlasTex, SousTextures, new TransTexture.TransData(filteredTriangle, tUV, sUV), DefaultPadding, TextureWarp);


                if (resultTextures.ContainsKey(targetTexture) == false) { resultTextures.Add(targetTexture, new List<TwoDimensionalMap<Color>>() { new TwoDimensionalMap<Color>(TransColor.GetColorArray(atlasTex.Array), atlasTex.MapSize) }); }
                else { resultTextures[targetTexture].Add(new TwoDimensionalMap<Color>(TransColor.GetColorArray(atlasTex.Array), atlasTex.MapSize)); }
            }

            return resultTextures;
        }
        public static List<Vector3> GetWorldSpaceVertices(Renderer Target)
        {
            List<Vector3> Vertices = new List<Vector3>();
            switch (Target)
            {
                case SkinnedMeshRenderer SMR:
                    {
                        Mesh mesh = new Mesh();
                        SMR.BakeMesh(mesh);
                        mesh.GetVertices(Vertices);
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
                        ConvertVerticesInMatrix(matrix, Vertices, Vector3.zero);
                        break;
                    }
                case MeshRenderer MR:
                    {
                        MR.GetComponent<MeshFilter>().sharedMesh.GetVertices(Vertices);
                        ConvertVerticesInMatrix(MR.localToWorldMatrix, Vertices, Vector3.zero);
                        break;
                    }
                default:
                    {
                        throw new System.ArgumentException("Rendererが対応したタイプではないか、TargetRendererが存在しません。");
                    }
            }
            return Vertices;
        }
        public static (List<Vector2>, List<List<TriangleIndex>>) RendererMeshToGetUVAndTriangle(Renderer Target)
        {
            Mesh mesh;
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
                    {
                        throw new System.ArgumentException("Rendererが対応したタイプではありません。");
                    }
            }
            List<Vector2> UV = new List<Vector2>();
            mesh.GetUVs(0, UV);
            List<List<TriangleIndex>> triangleIndex2DList = new List<List<TriangleIndex>>();
            foreach (var Index in Enumerable.Range(0, mesh.subMeshCount))
            {
                List<TriangleIndex> triangleIndexList = mesh.GetSubTriangleIndex(Index);
                triangleIndex2DList.Add(triangleIndexList);
            }
            return (UV, triangleIndex2DList);
        }
        public static List<Vector3> ConvertVerticesInMatrix(Matrix4x4 matrix, IEnumerable<Vector3> Vertices, Vector3 Offset)
        {
            var convertVertices = new List<Vector3>();
            foreach (var vert in Vertices)
            {
                var pos = matrix.MultiplyPoint3x4(vert) + Offset;
                convertVertices.Add(pos);
            }
            return convertVertices;
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
        public static List<Vector2> QuadNormalize(IReadOnlyList<Vector2> Quad, List<Vector2> TargetPoss)
        {
            List<Vector2> NormalizedPos = new List<Vector2>(TargetPoss.Count);
            foreach (var targetPos in TargetPoss)
            {
                NormalizedPos.Add(QuadNormalize(Quad, targetPos));
            }
            return NormalizedPos;
        }


        public static List<(int, Renderer)> FindAtRenderer(Matrix4x4 WorldToLocal, GameObject FindRoot = null)
        {
            var ResultList = new List<(int, Renderer)>();
            var Renderers = FindRoot != null ? FindRoot.GetComponentsInChildren<Renderer>() : UnityEngine.Object.FindObjectsOfType<Renderer>();
            foreach (var rd in Renderers)
            {
                if (!(rd is SkinnedMeshRenderer || rd is MeshRenderer)) { continue; }
                if (rd.GetMesh() == null) { continue; }
                var vert = GetWorldSpaceVertices(rd);
                var count = 0;
                for (var i = 0; vert.Count > i; i += 1)
                {
                    var pos = WorldToLocal.MultiplyPoint3x4(vert[i]);
                    pos.z -= 0.5f;
                    if (Mathf.Abs(pos.x) < 0.5f && Mathf.Abs(pos.y) < 0.5f && Mathf.Abs(pos.z) < 0.5f)
                    {
                        count += 1;
                    }
                }
                if (count > 0)
                {
                    ResultList.Add((count, rd));
                }
            }
            return ResultList;
        }
    }

    public enum PolygonCulling
    {
        Vertex,
        Edge,
        EdgeAndCenterRay,
    }
}