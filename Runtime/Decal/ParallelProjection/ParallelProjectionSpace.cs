using System;
using UnityEngine;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using net.rs64.TexTransTool.Utils;
using Unity.Burst;

namespace net.rs64.TexTransTool.Decal
{
    public class ParallelProjectionSpace : ISpaceConverter
    {
        internal Matrix4x4 ParallelProjectionMatrix;

        internal MeshData MeshData;
        private JobHandle _jobHandle;
        private NativeArray<Vector3> PPSVert;
        private NativeArray<Vector2> PPSVertWithUV;
        internal NativeArray<Vector3> GetPPSVert { get { _jobHandle.Complete(); return PPSVert; } }
        internal NativeArray<Vector2> GetUVVert { get { _jobHandle.Complete(); return PPSVertWithUV; } }
        public NativeArray<Vector3> GetPPSVertNoJobComplete() => PPSVert;
        public JobHandle GetPPSVertJobHandle() => _jobHandle;
        public void UpdatePPSVertJobHandle(JobHandle jh) => _jobHandle = jh;

        public bool AllowDepth => true;

        internal ParallelProjectionSpace(Matrix4x4 parallelProjectionMatrix)
        {
            ParallelProjectionMatrix = parallelProjectionMatrix;

        }
        public void Input(MeshData meshData)
        {
            MeshData = meshData;
            PPSVert = ConvertVerticesInMatrix(ParallelProjectionMatrix, meshData, new Vector3(0.5f, 0.5f, 0), out _jobHandle);
            PPSVertWithUV = ConvertVerticesToUV(PPSVert, ref _jobHandle);

        }

        public NativeArray<Vector2> UVOut() => GetUVVert;
        public NativeArray<Vector3> UVOutWithDepth() => GetPPSVert;

        public void Dispose()
        {
            MeshData = null;
            _jobHandle.Complete();
            PPSVert.Dispose();
            PPSVertWithUV.Dispose();
            _jobHandle = default;
            PPSVert = default;
            PPSVertWithUV = default;
        }

        internal static NativeArray<Vector3> ConvertVerticesInMatrix(Matrix4x4 matrix, MeshData meshData, Vector3 offset, out JobHandle jobHandle)
        {
            var array = new NativeArray<Vector3>(meshData.Vertices.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

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
        internal static NativeArray<Vector2> ConvertVerticesToUV(NativeArray<Vector3> vertices, ref JobHandle jobHandle)
        {
            var array = new NativeArray<Vector2>(vertices.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

            jobHandle = new ConvertToUV()
            {
                InputVertices = vertices,
                OutputUV = array,
            }.Schedule(array.Length, 64, jobHandle);

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
        [BurstCompile]
        private struct ConvertToUV : IJobParallelFor
        {
            [ReadOnly] public NativeArray<Vector3> InputVertices;
            [WriteOnly] public NativeArray<Vector2> OutputUV;

            public void Execute(int index)
            {
                OutputUV[index] = InputVertices[index];
            }
        }


    }
}

