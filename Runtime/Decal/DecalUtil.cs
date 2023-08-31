#if UNITY_EDITOR
using System.Collections.ObjectModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace net.rs64.TexTransTool.Decal
{
    public static class DecalUtil
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
            public IReadOnlyList<Vector3> Vertex;
            public IReadOnlyList<Vector2> UV;
            public IReadOnlyList<IReadOnlyList<TriangleIndex>> TrianglesSubMesh;

            public MeshData(List<Vector3> vertex, List<Vector2> uV, List<List<TriangleIndex>> trianglesSubMesh)
            {
                Vertex = vertex;
                UV = uV;
                TrianglesSubMesh = trianglesSubMesh.Cast<IReadOnlyList<TriangleIndex>>().ToList();
            }
        }
        public static Dictionary<KeyTexture, RenderTexture> CreateDecalTexture<KeyTexture, SpaceConverter>(
            Renderer TargetRenderer,
            Dictionary<KeyTexture, RenderTexture> RenderTextures,
            Texture SousTextures,
            SpaceConverter ConvertSpace,
            ITrianglesFilter<SpaceConverter> Filter,
            string TargetPropertyName = "_MainTex",
            Vector2? TextureOutRange = null,
            //TexWrapMode TexWrapMode = TexWrapMode.NotWrap,
            float DefaultPadding = 0.5f
        )
        where KeyTexture : Texture
        where SpaceConverter : IConvertSpace
        {
            if (RenderTextures == null) RenderTextures = new Dictionary<KeyTexture, RenderTexture>();

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
                var targetTexture = targetMat.GetTexture(TargetPropertyName) as KeyTexture;
                if (targetTexture == null) { continue; }
                var targetTexSize = targetTexture is Texture2D tex2d ? tex2d.NativeSize() : new Vector2Int(targetTexture.width, targetTexture.height);

                var filteredTriangle = Filter != null ? Filter.Filtering(ConvertSpace, triangle) : triangle;
                if (filteredTriangle.Any() == false) { continue; }




                if (!RenderTextures.ContainsKey(targetTexture))
                {
                    var rendererTexture = new RenderTexture(targetTexSize.x, targetTexSize.y, 32, UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_SRGB);
                    RenderTextures.Add(targetTexture, rendererTexture);
                }

                TransTexture.TransTextureToRenderTexture(
                    RenderTextures[targetTexture],
                    SousTextures,
                    new TransTexture.TransUVData(filteredTriangle, tUV, sUV),
                    DefaultPadding,
                    TextureOutRange
                );


            }

            return RenderTextures;
        }
        public static Dictionary<Texture2D, List<Texture2D>> CreateDecalTextureCS<SpaceConverter>(
            Renderer TargetRenderer,
            Texture2D SousTextures,
            SpaceConverter ConvertSpace,
            ITrianglesFilter<SpaceConverter> Filter,
            string TargetPropertyName = "_MainTex",
            Vector2? TextureOutRange = null,
            float DefaultPadding = 1f
        )
        where SpaceConverter : IConvertSpace
        {
            var resultTextures = new Dictionary<Texture2D, List<Texture2D>>();

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
                var targetTexSize = targetTexture.NativeSize();

                var filteredTriangle = Filter != null ? Filter.Filtering(ConvertSpace, triangle) : triangle;
                if (filteredTriangle.Any() == false) { continue; }


                var atlasTex = new TransTargetTexture(Utils.CreateFillTexture(targetTexSize, new Color(0, 0, 0, 0)), new TwoDimensionalMap<float>(TransTexture.CSPadding(DefaultPadding), targetTexSize));
                TransTexture.TransTextureUseCS(atlasTex, SousTextures, new TransTexture.TransUVData(filteredTriangle, tUV, sUV), DefaultPadding, TextureOutRange);


                if (resultTextures.ContainsKey(targetTexture) == false) { resultTextures.Add(targetTexture, new List<Texture2D>() { atlasTex.Texture2D }); }
                else { resultTextures[targetTexture].Add(atlasTex.Texture2D); }
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
                List<TriangleIndex> triangleIndexList = Utils.ToList(mesh.GetTriangles(Index));
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
            var oneNearPoint = TransMapper.NearPoint(Quad[0], Quad[2], TargetPos);
            var oneCross = Vector3.Cross(Quad[2] - Quad[0], TargetPos - Quad[0]).z > 0 ? -1 : 1;

            var twoNearPoint = TransMapper.NearPoint(Quad[0], Quad[1], TargetPos);
            var twoCross = Vector3.Cross(Quad[1] - Quad[0], TargetPos - Quad[0]).z > 0 ? 1 : -1;

            var threeNearPoint = TransMapper.NearPoint(Quad[1], Quad[3], TargetPos);
            var threeCross = Vector3.Cross(Quad[3] - Quad[1], TargetPos - Quad[1]).z > 0 ? 1 : -1;

            var forNearPoint = TransMapper.NearPoint(Quad[2], Quad[3], TargetPos);
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
    }

    public enum PolygonCulling
    {
        Vertex,
        Edge,
        EdgeAndCenterRay,
    }
}
#endif
