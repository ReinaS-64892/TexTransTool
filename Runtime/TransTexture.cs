using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using JetBrains.Annotations;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using System;
using net.rs64.TexTransCore;
using net.rs64.TexTransCoreEngineForUnity;

namespace net.rs64.TexTransTool
{
    internal static class TransTexture
    {
        public struct TransData
        {
            public NativeArray<TriangleIndex> TrianglesToIndex;
            public NativeArray<Vector2> TargetUV;
            public NativeArray<Vector2> SourceUV;

            public TransData(
                IEnumerable<TriangleIndex> trianglesToIndex,
                IEnumerable<Vector2> targetUV,
                IEnumerable<Vector2> sourceUV
            )
            {
                // TODO - このコンストラクタを呼び出してるところをNativeArrayに切り替える
                TrianglesToIndex = new NativeArray<TriangleIndex>(trianglesToIndex.ToArray(), Allocator.TempJob);
                TargetUV = new NativeArray<Vector2>(targetUV.ToArray(), Allocator.TempJob);
                SourceUV = new NativeArray<Vector2>(sourceUV.ToArray(), Allocator.TempJob);

                var self = this;
                TexTransCoreRuntime.NextUpdateCall += () =>
                {
                    self.TrianglesToIndex.Dispose();
                    self.TargetUV.Dispose();
                    self.SourceUV.Dispose();
                };
            }

            public TransData(NativeArray<TriangleIndex> trianglesToIndex, NativeArray<Vector2> targetUV, NativeArray<Vector2> sourceUV)
            {
                TrianglesToIndex = trianglesToIndex;
                TargetUV = targetUV;
                SourceUV = sourceUV;
            }

            public Mesh GenerateTransMesh()
            {
                var mda = Mesh.AllocateWritableMeshData(1);
                var mda_mesh = mda[0];

                mda_mesh.SetVertexBufferParams(
                    TargetUV.Length,
                    new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
                    new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, UnsafeUtility.SizeOf<Vector2>() / 4, stream: 1)
                );
                mda_mesh.SetIndexBufferParams(TrianglesToIndex.Length * 3, IndexFormat.UInt32);

                var pos_array = mda_mesh.GetVertexData<Vector3>(0);
                var uv_array = mda_mesh.GetVertexData<Vector2>(1);
                var dst_triangles = mda_mesh.GetIndexData<int>();

                var job1 = new CopyPos { Source = TargetUV, Destination = pos_array }.Schedule(TargetUV.Length, 64);
                var job2 = new CopyJob<Vector2> { Source = SourceUV, Destination = uv_array }.Schedule(SourceUV.Length, 64, job1);
                var job3 = new UnpackTriangleJob { Source = TrianglesToIndex, Destination = dst_triangles }.Schedule(dst_triangles.Length, 64, job2);

                var mesh = new Mesh();

                job3.Complete();

                mda_mesh.subMeshCount = 1;
                mda_mesh.SetSubMesh(0, new SubMeshDescriptor(0, dst_triangles.Length, MeshTopology.Triangles));

                Mesh.ApplyAndDisposeWritableMeshData(mda, mesh);

                return mesh;
            }
        }

        public static NativeArray<Vector4> PackingTrianglesForFrom(Span<TriangleIndex> triangle, Span<Vector3> fromUV, Allocator allocator)
        {
            var na = new NativeArray<Vector4>(triangle.Length * 3, allocator, NativeArrayOptions.UninitializedMemory);
            var sp = na.AsSpan();
            for (var i = 0; triangle.Length > i; i += 1)
            {
                var tri = triangle[i];
                var i1 = i * 3;
                var i2 = i1 + 1;
                var i3 = i1 + 2;

                sp[i1].x = fromUV[tri[0]].x;
                sp[i1].y = fromUV[tri[0]].y;
                sp[i1].z = fromUV[tri[0]].z;
                sp[i1].w = 0f;

                sp[i2].x = fromUV[tri[1]].x;
                sp[i2].y = fromUV[tri[1]].y;
                sp[i2].z = fromUV[tri[1]].z;
                sp[i2].w = 0f;

                sp[i3].x = fromUV[tri[2]].x;
                sp[i3].y = fromUV[tri[2]].y;
                sp[i3].z = fromUV[tri[2]].z;
                sp[i3].w = 0f;
            }
            return na;
        }
        public static NativeArray<Vector4> PackingTrianglesForFrom(Span<TriangleIndex> triangle, Span<Vector2> fromUV, Allocator allocator)
        {
            var na = new NativeArray<Vector4>(triangle.Length * 3, allocator, NativeArrayOptions.UninitializedMemory);
            var sp = na.AsSpan();
            for (var i = 0; triangle.Length > i; i += 1)
            {
                var tri = triangle[i];
                var i1 = i * 3;
                var i2 = i1 + 1;
                var i3 = i1 + 2;

                sp[i1].x = fromUV[tri[0]].x;
                sp[i1].y = fromUV[tri[0]].y;
                sp[i1].z = 0f;
                sp[i1].w = 0f;

                sp[i2].x = fromUV[tri[1]].x;
                sp[i2].y = fromUV[tri[1]].y;
                sp[i2].z = 0f;
                sp[i2].w = 0f;

                sp[i3].x = fromUV[tri[2]].x;
                sp[i3].y = fromUV[tri[2]].y;
                sp[i3].z = 0f;
                sp[i3].w = 0f;
            }
            return na;
        }
        public static NativeArray<Vector2> PackingTrianglesForTo(Span<TriangleIndex> triangle, Span<Vector2> toUV, Allocator allocator)
        {
            var na = new NativeArray<Vector2>(triangle.Length * 3, allocator, NativeArrayOptions.UninitializedMemory);
            var sp = na.AsSpan();
            for (var i = 0; triangle.Length > i; i += 1)
            {
                var tri = triangle[i];
                var i1 = i * 3;
                var i2 = i1 + 1;
                var i3 = i1 + 2;

                sp[i1].x = toUV[tri[0]].x;
                sp[i1].y = toUV[tri[0]].y;

                sp[i2].x = toUV[tri[1]].x;
                sp[i2].y = toUV[tri[1]].y;

                sp[i3].x = toUV[tri[2]].x;
                sp[i3].y = toUV[tri[2]].y;
            }
            return na;
        }
        [UsedImplicitly]
        private static void BurstInstantiate()
        {
            new CopyJob<Vector2>().Schedule(1, 1);
            new CopyJob<Vector3>().Schedule(1, 1);
            new CopyJob<Vector4>().Schedule(1, 1);
        }

        [BurstCompile]
        struct UnpackTriangleJob : IJobParallelFor
        {
            [ReadOnly] public NativeArray<TriangleIndex> Source;
            [WriteOnly] public NativeArray<int> Destination;

            public void Execute(int index)
            {
                var tri_index = index / 3;
                var coord = index % 3;

                Destination[index] = Source[tri_index][coord];
            }
        }

        [BurstCompile]
        struct CopyPos : IJobParallelFor
        {
            [ReadOnly] public NativeArray<Vector2> Source;
            [WriteOnly] public NativeArray<Vector3> Destination;

            public void Execute(int index)
            {
                Destination[index] = Source[index];
            }
        }

        [BurstCompile]
        struct CopyJob<T> : IJobParallelFor where T : struct
        {
            [ReadOnly] public NativeArray<T> Source;
            [WriteOnly] public NativeArray<T> Destination;

            public void Execute(int index)
            {
                Destination[index] = Source[index];
            }
        }
        [TexTransInitialize]
        public static void Init()
        {
            s_transShader = Shader.Find(TRANS_SHADER);
        }

        public const string TRANS_SHADER = "Hidden/TransTexture";
        static Shader s_transShader;


        static Material s_transMat;
        static Material s_depthMat;
        public static void ForTrans(
            RenderTexture targetTexture,
            Texture sourceTexture,
            TransData transUVData,
            float? padding = null,
            TextureWrap? argTexWrap = null,
            bool highQualityPadding = false,
            bool? depthInvert = null,
            bool NotTileNormalize = false
            )
        {
            Profiler.BeginSample("GenerateTransMesh");
            var mesh = transUVData.GenerateTransMesh();
            Profiler.EndSample();

            var preWarp = sourceTexture.wrapMode;

            try
            {
                if (argTexWrap == null) { argTexWrap = TextureWrap.NotWrap; }
                var texWrap = argTexWrap.Value;
                sourceTexture.wrapMode = texWrap.ConvertTextureWrapMode;




                Profiler.BeginSample("Material Setup");
                if (s_transMat == null) { s_transMat = new Material(s_transShader); }
                s_transMat.shaderKeywords = Array.Empty<string>();
                s_transMat.SetTexture("_MainTex", sourceTexture);
                if (padding.HasValue) s_transMat.SetFloat("_Padding", padding.Value);

                if (texWrap.WarpRange != null)
                {
                    s_transMat.EnableKeyword("WarpRange");
                    s_transMat.SetFloat("_WarpRangeX", texWrap.WarpRange.Value.x);
                    s_transMat.SetFloat("_WarpRangeY", texWrap.WarpRange.Value.y);
                }

                if (NotTileNormalize)
                {
                    s_transMat.EnableKeyword("UnTileNormalize");
                }
                Profiler.EndSample();

                using (new RTActiveSaver())
                {
                    RenderTexture.active = targetTexture;
                    Profiler.BeginSample("DrawMeshNow");
                    s_transMat.SetPass(0);
                    Graphics.DrawMeshNow(mesh, Matrix4x4.identity);
                    Profiler.EndSample();
                    if (padding != null)
                    {
                        Profiler.BeginSample("DrawMeshNow - padding");
                        s_transMat.SetPass(1);
                        Graphics.DrawMeshNow(mesh, Matrix4x4.identity);
                        Profiler.EndSample();
                    }
                }

            }
            finally
            {
                sourceTexture.wrapMode = preWarp;
                UnityEngine.Object.DestroyImmediate(mesh);
            }
        }
        public static void ForTrans(
            RenderTexture targetTexture,
            Texture sourceTexture,
            IEnumerable<TransData> transUVDataEnumerable,
            float? padding = null,
            TextureWrap? warpRange = null
            )
        {
            foreach (var transUVData in transUVDataEnumerable)
            {
                ForTrans(targetTexture, sourceTexture, transUVData, padding, warpRange);
            }
        }

    }
}
