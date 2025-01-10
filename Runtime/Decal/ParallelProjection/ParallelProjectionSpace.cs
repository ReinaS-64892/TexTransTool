#nullable enable
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;

namespace net.rs64.TexTransTool.Decal
{
    internal class ParallelProjectionSpaceConvertor : ISpaceConverter<ParallelProjectionSpace>
    {
        Matrix4x4 _parallelProjectionMatrix;
        static readonly Vector3 offset = new Vector3(0.5f, 0.5f, 0);

        internal ParallelProjectionSpaceConvertor(Matrix4x4 parallelProjectionMatrix) { _parallelProjectionMatrix = parallelProjectionMatrix; }

        public ParallelProjectionSpace ConvertSpace(MeshData[] meshData)
        {
            var handleArray = new JobHandle[meshData.Length];
            var ppsUVArray = new NativeArray<Vector3>[meshData.Length];

            for (var i = 0; meshData.Length > i; i += 1)
            {
                var md = meshData[i];
                ppsUVArray[i] = ConvertVerticesInMatrix(_parallelProjectionMatrix, md, offset, out var jobHandle);
                md.AddJobDependency(jobHandle);
                handleArray[i] = jobHandle;
            }

            return new(meshData, ppsUVArray, handleArray);
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
    }
    internal class ParallelProjectionSpace : IDecalSpaceWith3D
    {
        // not owned
        internal readonly MeshData[] _meshData;

        // owned
        internal NativeArray<Vector3>[] _ppsUVArray;
        internal JobHandle[] _uvCalculateJobHandles;

        public ParallelProjectionSpace(MeshData[] meshData, NativeArray<Vector3>[] ppsUVArray, JobHandle[] uvCalculateJobHandles)
        {
            _meshData = meshData;
            _ppsUVArray = ppsUVArray;
            _uvCalculateJobHandles = uvCalculateJobHandles;
        }

        public NativeArray<Vector3>[] OutputUV()
        {
            foreach (var jh in _uvCalculateJobHandles) { jh.Complete(); }
            return _ppsUVArray;
        }
        public void Dispose()
        {
            for (var i = 0; _uvCalculateJobHandles.Length > i; i += 1)
            {
                _uvCalculateJobHandles[i].Complete();
                _ppsUVArray[i].Dispose();
            }
        }

    }
}

