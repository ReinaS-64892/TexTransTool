using System.Collections.ObjectModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using net.rs64.TexTransCore.TransTextureCore;
using net.rs64.TexTransCore.TransTextureCore.Utils;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine.Pool;
using UnityEngine.Profiling;

namespace net.rs64.TexTransCore.Decal
{
    public static class DecalUtility
    {
        public interface IConvertSpace<UVDimension> : IDisposable
        where UVDimension : struct
        {
            void Input(MeshData meshData);
            NativeArray<UVDimension> OutPutUV();
        }
        public interface ITrianglesFilter<SpaceConverter>
        {
            void SetSpace(SpaceConverter space);
            List<TriangleIndex> GetFilteredSubTriangle(int subMeshIndex);
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

            Profiler.BeginSample("GetMeshData");
            var meshData = targetRenderer.Memo(GetMeshData, i => i.Dispose());
            Profiler.EndSample();

            Profiler.BeginSample("GetUVs");
            var tUV = meshData.VertexUV;
            Profiler.EndSample();

            Profiler.BeginSample("convertSpace.Input");
            convertSpace.Input(meshData);
            Profiler.EndSample();

            filter.SetSpace(convertSpace);

            var materials = targetRenderer.sharedMaterials;

            for (int i = 0; i < meshData.Triangles.Length; i++)
            {
                var targetMat = materials[i];

                if (!targetMat.HasProperty(targetPropertyName)) { continue; };
                var targetTexture = targetMat.GetTexture(targetPropertyName);
                if (targetTexture == null) { continue; }

                Profiler.BeginSample("GetFilteredSubTriangle");
                var filteredTriangle = filter.GetFilteredSubTriangle(i);
                Profiler.EndSample();
                if (filteredTriangle.Any() == false) { continue; }

                if (!renderTextures.ContainsKey(targetMat))
                {
                    renderTextures[targetMat] = RenderTexture.GetTemporary(targetTexture.width, targetTexture.height, 32);
                    renderTextures[targetMat].Clear();
                }
                var sUV = convertSpace.OutPutUV();

                Profiler.BeginSample("TransTexture.ForTrans");
                TransTexture.ForTrans(
                    renderTextures[targetMat],
                    sousTextures,
                    new TransTexture.TransData<UVDimension>(filteredTriangle, tUV, sUV),
                    defaultPadding,
                    textureWarp,
                    highQualityPadding,
                    useDepthOrInvert
                );
                Profiler.EndSample();
            }
            convertSpace.Dispose();//convertSpaceの解放責任はこっちにある

            return renderTextures;
        }
        public static MeshData GetMeshData(Renderer renderer) => new MeshData(renderer);
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

        internal static NativeArray<Vector3> ConvertVerticesInMatrix(Matrix4x4 matrix, MeshData meshData, Vector3 offset, out JobHandle jobHandle)
        {
            var array = new NativeArray<Vector3>(meshData.Vertices.Length, Allocator.TempJob);

            jobHandle = new ConvertVerticesJob()
            {
                InputVertices = meshData.Vertices,
                OutputVertices = array,
                Matrix = matrix,
                Offset = offset
            }.Schedule(meshData.Vertices.Length, 64);

            meshData.AddJobDependency(jobHandle);

            return array;
        }

        private struct ConvertVerticesJob : IJobParallelFor
        {
            [ReadOnly] public NativeArray<Vector3> InputVertices;
            [WriteOnly] public NativeArray<Vector3> OutputVertices;
            public Matrix4x4 Matrix;
            public Vector3 Offset;

            public void Execute(int index)
            {
                OutputVertices[index] = Matrix.MultiplyPoint3x4(InputVertices[index]) + Offset;
            }
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
