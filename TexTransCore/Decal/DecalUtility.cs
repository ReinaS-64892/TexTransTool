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
    public static class DecalUtility
    {
        public interface IConvertSpace<UVDimension>
        where UVDimension : struct
        {
            void Input(MeshData meshData);
            List<UVDimension> OutPutUV(List<UVDimension> output = null);
        }
        public interface ITrianglesFilter<SpaceConverter>
        {
            List<TriangleIndex> Filtering(SpaceConverter space, List<TriangleIndex> triangles, List<TriangleIndex> output = null);
        }
        public class MeshData
        {
            internal readonly List<Vector3> Vertex;
            internal readonly List<Vector2> UV;
            internal readonly List<List<TriangleIndex>> TrianglesSubMesh;

            internal MeshData(List<Vector3> vertex, List<Vector2> uV, List<List<TriangleIndex>> trianglesSubMesh)
            {
                Vertex = vertex;
                UV = uV;
                TrianglesSubMesh = trianglesSubMesh;
            }
        }
        internal static Dictionary<Material, RenderTexture> CreateDecalTexture<SpaceConverter, UVDimension>(
            Renderer targetRenderer,
            Dictionary<Material, RenderTexture> renderTextures,
            Texture sousTextures,
            SpaceConverter convertSpace,
            ITrianglesFilter<SpaceConverter> filter,
            string targetPropertyName = "_MainTex",
            TextureWrap? textureWarp = null,
            float defaultPadding = 0.5f,
            bool highQualityPadding = false,
            bool? useDepthOrInvert = null
        )
        where SpaceConverter : IConvertSpace<UVDimension>
        where UVDimension : struct
        {
            if (renderTextures == null) renderTextures = new();

            var vertices = ListPool<Vector3>.Get(); GetWorldSpaceVertices(targetRenderer, vertices);
            var targetMesh = targetRenderer.GetMesh();
            var tUV = ListPool<Vector2>.Get(); targetMesh.GetUVs(0, tUV);
            var trianglesSubMesh = targetMesh.GetPooledSubTriangle();

            convertSpace.Input(new MeshData(vertices, tUV, trianglesSubMesh));
            var sUVPooled = ListPool<UVDimension>.Get();
            var sUV = convertSpace.OutPutUV(sUVPooled);

            var materials = targetRenderer.sharedMaterials;

            for (int i = 0; i < trianglesSubMesh.Count; i++)
            {
                var triangle = trianglesSubMesh[i];
                var targetMat = materials[i];

                if (!targetMat.HasProperty(targetPropertyName)) { continue; };
                var targetTexture = targetMat.GetTexture(targetPropertyName);
                if (targetTexture == null) { continue; }
                var targetTexSize = new Vector2Int(targetTexture.width, targetTexture.height);

                List<TriangleIndex> filteredTriangle;
                var filteredTrianglePooled = ListPool<TriangleIndex>.Get();
                if (filter != null) { filteredTriangle = filter.Filtering(convertSpace, triangle, filteredTrianglePooled); }
                else { filteredTriangle = triangle; }

                if (filteredTriangle.Any() == false) { continue; }

                if (!renderTextures.ContainsKey(targetMat))
                {
                    renderTextures.Add(targetMat, new(targetTexSize.x, targetTexSize.y, 32));
                }

                TransTexture.ForTrans(
                    renderTextures[targetMat],
                    sousTextures,
                    new TransTexture.TransData<UVDimension>(filteredTriangle, tUV, sUV),
                    defaultPadding,
                    textureWarp,
                    highQualityPadding,
                    useDepthOrInvert
                );


                ListPool<TriangleIndex>.Release(filteredTrianglePooled);
            }
            ListPool<Vector3>.Release(vertices);
            ListPool<Vector2>.Release(tUV);
            ListPool<UVDimension>.Release(sUVPooled);
            ReleasePooledSubTriangle(trianglesSubMesh);

            return renderTextures;
        }
        internal static List<Vector3> GetWorldSpaceVertices(Renderer target, List<Vector3> outPut = null)
        {
            outPut?.Clear(); outPut ??= new List<Vector3>();
            switch (target)
            {
                case SkinnedMeshRenderer smr:
                    {
                        Mesh mesh = new Mesh();
                        smr.BakeMesh(mesh);
                        mesh.GetVertices(outPut);
                        Matrix4x4 matrix;
                        if (smr.bones.Any())
                        {
                            matrix = Matrix4x4.TRS(smr.transform.position, smr.transform.rotation, Vector3.one);
                        }
                        else if (smr.rootBone == null)
                        {
                            matrix = smr.localToWorldMatrix;
                        }
                        else
                        {
                            matrix = smr.rootBone.localToWorldMatrix;
                        }
                        ConvertVerticesInMatrix(matrix, outPut, Vector3.zero);
                        break;
                    }
                case MeshRenderer mr:
                    {
                        mr.GetComponent<MeshFilter>().sharedMesh.GetVertices(outPut);
                        ConvertVerticesInMatrix(mr.localToWorldMatrix, outPut, Vector3.zero);
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
        internal static void ReleasePooledSubTriangle(List<List<TriangleIndex>> subTriList)
        {
            foreach (var subTri in subTriList)
            {
                ListPool<TriangleIndex>.Release(subTri);
            }
            ListPool<List<TriangleIndex>>.Release(subTriList);
        }

        internal static List<Vector3> ConvertVerticesInMatrix(Matrix4x4 matrix, IEnumerable<Vector3> vertices, Vector3 offset, List<Vector3> outPut = null)
        {
            outPut?.Clear(); outPut ??= new List<Vector3>();
            foreach (var vert in vertices)
            {
                var pos = matrix.MultiplyPoint3x4(vert) + offset;
                outPut.Add(pos);
            }
            return outPut;
        }
        internal static void ConvertVerticesInMatrix(Matrix4x4 matrix, List<Vector3> vertices, Vector3 Offset)
        {
            for (int i = 0; i < vertices.Count; i++)
            {
                vertices[i] = matrix.MultiplyPoint3x4(vertices[i]) + Offset;
            }
        }
        internal static Vector2 QuadNormalize(IReadOnlyList<Vector2> quad, Vector2 targetPos)
        {
            var oneNearPoint = VectorUtility.NearPoint(quad[0], quad[2], targetPos);
            var oneCross = Vector3.Cross(quad[2] - quad[0], targetPos - quad[0]).z > 0 ? -1 : 1;

            var twoNearPoint = VectorUtility.NearPoint(quad[0], quad[1], targetPos);
            var twoCross = Vector3.Cross(quad[1] - quad[0], targetPos - quad[0]).z > 0 ? 1 : -1;

            var threeNearPoint = VectorUtility.NearPoint(quad[1], quad[3], targetPos);
            var threeCross = Vector3.Cross(quad[3] - quad[1], targetPos - quad[1]).z > 0 ? 1 : -1;

            var forNearPoint = VectorUtility.NearPoint(quad[2], quad[3], targetPos);
            var forCross = Vector3.Cross(quad[3] - quad[2], targetPos - quad[2]).z > 0 ? -1 : 1;

            var oneDistance = Vector2.Distance(oneNearPoint, targetPos) * oneCross;
            var towDistance = Vector2.Distance(twoNearPoint, targetPos) * twoCross;
            var threeDistance = Vector2.Distance(threeNearPoint, targetPos) * threeCross;
            var forDistance = Vector2.Distance(forNearPoint, targetPos) * forCross;

            var x = oneDistance / (oneDistance + threeDistance);
            var y = towDistance / (towDistance + forDistance);

            return new Vector2(x, y);
        }
        internal static List<Vector2> QuadNormalize(IReadOnlyList<Vector2> quad, List<Vector2> targetPoss, List<Vector2> outPut = null)
        {
            outPut?.Clear(); outPut ??= new List<Vector2>(targetPoss.Count);
            foreach (var targetPos in targetPoss)
            {
                outPut.Add(QuadNormalize(quad, targetPos));
            }
            return outPut;
        }
    }

    public enum PolygonCulling
    {
        Vertex,
        Edge,
        EdgeAndCenterRay,
    }
}