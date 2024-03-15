using System.Collections.ObjectModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using net.rs64.TexTransCore.TransTextureCore;
using net.rs64.TexTransCore.TransTextureCore.Utils;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine.Pool;
using UnityEngine.Profiling;

namespace net.rs64.TexTransCore.Decal
{
    public static class DecalUtility
    {
        public interface IConvertSpace<UVDimension>: IDisposable
        where UVDimension : struct
        {
            void Input(MeshData meshData);
            NativeArray<UVDimension> OutPutUV();
        }
        public interface ITrianglesFilter<SpaceConverter>
        {
            List<TriangleIndex> Filtering(SpaceConverter space, List<TriangleIndex> triangles, List<TriangleIndex> output = null);
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
            var meshData = targetRenderer.Memo(GetMeshData);
            Profiler.EndSample();
            
            var targetMesh = targetRenderer.GetMesh();
            
            Profiler.BeginSample("GetUVs");
            var tUV = meshData.VertexUV;
            Profiler.EndSample();
            
            Profiler.BeginSample("GetPooledSubTriangle");
            var trianglesSubMesh = targetMesh.GetPooledSubTriangle();
            Profiler.EndSample();

            Profiler.BeginSample("convertSpace.Input");
            convertSpace.Input(meshData);
            Profiler.EndSample();
            
            var sUVPooled = ListPool<UVDimension>.Get();
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
                if (filter != null)
                {
                    Profiler.BeginSample("Filtering");
                    filteredTriangle = filter.Filtering(convertSpace, triangle, filteredTrianglePooled);
                    Profiler.EndSample();
                }
                else { filteredTriangle = triangle; }

                if (filteredTriangle.Any() == false) { continue; }

                if (!renderTextures.ContainsKey(targetMat))
                {
                    var tempRt = RenderTexture.GetTemporary(targetTexSize.x, targetTexSize.y, 32); tempRt.Clear();
                    renderTextures.Add(targetMat, tempRt);
                }

                var sUV = convertSpace.OutPutUV();
                
                var nativeFilteredTriangle = new NativeArray<TriangleIndex>(filteredTriangle.Count, Allocator.TempJob);
                for (int t = 0; t < filteredTriangle.Count; t++)
                {
                    nativeFilteredTriangle[t] = filteredTriangle[t];
                }
                
                Profiler.BeginSample("TransTexture.ForTrans");
                TransTexture.ForTrans(
                    renderTextures[targetMat],
                    sousTextures,
                    new TransTexture.TransData<UVDimension>(nativeFilteredTriangle, tUV, sUV),
                    defaultPadding,
                    textureWarp,
                    highQualityPadding,
                    useDepthOrInvert
                );
                Profiler.EndSample();
                nativeFilteredTriangle.Dispose();
                
                ListPool<TriangleIndex>.Release(filteredTrianglePooled);
            }
            
            ListPool<UVDimension>.Release(sUVPooled);
            ReleasePooledSubTriangle(trianglesSubMesh);

            return renderTextures;
        }
        internal static MeshData GetMeshData(Renderer target)
        {
            MeshData result;
            switch (target)
            {
                case SkinnedMeshRenderer smr:
                    {
                        Mesh mesh = new Mesh();
                        smr.BakeMesh(mesh);
                        
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

                        result = new MeshData(mesh, matrix);

                        UnityEngine.Object.DestroyImmediate(mesh);
                        break;
                    }
                case MeshRenderer mr:
                    {
                        return new MeshData(mr.GetComponent<MeshFilter>().sharedMesh, mr.localToWorldMatrix);
                        break;
                    }
                default:
                    {
                        throw new System.ArgumentException("Rendererが対応したタイプではないか、TargetRendererが存在しません。");
                    }
            }
            return result;
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

        [BurstCompile]
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