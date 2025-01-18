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
            var ppsOutUVArray = new NativeArray<Vector2>[meshData.Length];
            var ppsDepthArray = new NativeArray<float>[meshData.Length];

            for (var i = 0; meshData.Length > i; i += 1)
            {
                var md = meshData[i];
                ppsUVArray[i] = ConvertVerticesInMatrix(_parallelProjectionMatrix, md, offset, out var jobHandle);

                var (outUV, jobHandle1) = ConvertVerticesToOutUV(ppsUVArray[i], jobHandle);
                ppsOutUVArray[i] = outUV;

                var (outDepthUV, jobHandle2) = ConvertVerticesToDepthUV(ppsUVArray[i], jobHandle);
                ppsDepthArray[i] = outDepthUV;

                md.AddJobDependency(jobHandle);
                handleArray[i] = JobHandle.CombineDependencies(jobHandle1, jobHandle2);
            }

            return new(meshData, ppsUVArray, ppsOutUVArray, ppsDepthArray, handleArray);
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
        internal static (NativeArray<Vector2>, JobHandle) ConvertVerticesToOutUV(NativeArray<Vector3> input, JobHandle inputJobHandle)
        {
            var array = new NativeArray<Vector2>(input.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

            var outJobHandle = new CopyVertexUV()
            {
                InputVertices = input,
                OutUVVertices = array,
            }.Schedule(input.Length, 64, inputJobHandle);


            return (array, outJobHandle);
        }
        internal static (NativeArray<float>, JobHandle) ConvertVerticesToDepthUV(NativeArray<Vector3> input, JobHandle inputJobHandle)
        {
            var array = new NativeArray<float>(input.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

            var outJobHandle = new CopyDepth()
            {
                InputVertices = input,
                OutDepthVertices = array,
            }.Schedule(input.Length, 64, inputJobHandle);

            return (array, outJobHandle);
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
        private struct CopyVertexUV : IJobParallelFor
        {
            [ReadOnly] public NativeArray<Vector3> InputVertices;
            [WriteOnly] public NativeArray<Vector2> OutUVVertices;

            public void Execute(int index)
            {
                OutUVVertices[index] = InputVertices[index];
            }
        }
        [BurstCompile]
        private struct CopyDepth : IJobParallelFor
        {
            [ReadOnly] public NativeArray<Vector3> InputVertices;
            [WriteOnly] public NativeArray<float> OutDepthVertices;

            public void Execute(int index)
            {
                OutDepthVertices[index] = InputVertices[index].z;
            }
        }
    }
    internal class ParallelProjectionSpace : IDecalSpaceWith3D
    {
        // not owned
        internal readonly MeshData[] _meshData;

        // owned
        internal NativeArray<Vector3>[] _ppsUVArray;
        internal NativeArray<Vector2>[] _ppsOutUVArray;
        internal NativeArray<float>[] _ppsDepthUVArray;
        internal JobHandle[] _uvCalculateJobHandles;

        public ParallelProjectionSpace(MeshData[] meshData, NativeArray<Vector3>[] ppsUVArray, NativeArray<Vector2>[] ppsOutUVArray, NativeArray<float>[] ppsDepthArray, JobHandle[] uvCalculateJobHandles)
        {
            _meshData = meshData;
            _ppsUVArray = ppsUVArray;
            _ppsOutUVArray = ppsOutUVArray;
            _ppsDepthUVArray = ppsDepthArray;
            _uvCalculateJobHandles = uvCalculateJobHandles;
        }

        NativeArray<Vector2>[] IDecalSpace.OutputUV()
        {
            foreach (var jh in _uvCalculateJobHandles) { jh.Complete(); }
            return _ppsOutUVArray;
        }
        public NativeArray<float>[] OutputDepth()
        {
            foreach (var jh in _uvCalculateJobHandles) { jh.Complete(); }
            return _ppsDepthUVArray;
        }
        public void Dispose()
        {
            for (var i = 0; _uvCalculateJobHandles.Length > i; i += 1)
            {
                _uvCalculateJobHandles[i].Complete();
                _ppsUVArray[i].Dispose();
                _ppsOutUVArray[i].Dispose();
                _ppsDepthUVArray[i].Dispose();
            }
        }

    }
}

