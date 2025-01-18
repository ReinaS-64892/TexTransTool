#nullable enable
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using net.rs64.TexTransTool.IslandSelector;
using net.rs64.TexTransCore;

namespace net.rs64.TexTransTool.Decal
{
    internal class SingleGradientConvertor : ISpaceConverter<SingleGradientSpace>
    {
        Matrix4x4 _world2LocalMatrix;
        public SingleGradientConvertor(Matrix4x4 w2l) { _world2LocalMatrix = w2l; }

        public SingleGradientSpace ConvertSpace(MeshData[] meshData)
        {
            var handleArray = new JobHandle[meshData.Length];
            var uvArray = new NativeArray<Vector2>[meshData.Length];

            for (var i = 0; meshData.Length > i; i += 1)
            {
                var md = meshData[i];
                var uvNa = uvArray[i] = new NativeArray<Vector2>(md.Vertices.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
                var convertJob = new ConvertJob()
                {
                    World2Local = _world2LocalMatrix,
                    worldVerticals = md.Vertices,
                    uv = uvNa
                };
                var convertJobHandle = convertJob.Schedule(uvNa.Length, 32);
                md.AddJobDependency(convertJobHandle);
                handleArray[i] = convertJobHandle;
            }

            return new SingleGradientSpace(meshData, uvArray, handleArray);
        }

        [BurstCompile]
        struct ConvertJob : IJobParallelFor
        {
            [ReadOnly] public NativeArray<Vector3> worldVerticals;
            [ReadOnly] public Matrix4x4 World2Local;
            [WriteOnly] public NativeArray<Vector2> uv;
            public void Execute(int index) { uv[index] = new Vector2(World2Local.MultiplyPoint3x4(worldVerticals[index]).y, 0.5f); }
        }

    }

    internal class SingleGradientSpace : IDecalSpace
    {
        // not owned
        internal readonly MeshData[] _meshData;

        // owned
        NativeArray<Vector2>[] _uv;
        JobHandle[] _uvCalculateJobHandles;

        public SingleGradientSpace(MeshData[] meshData, NativeArray<Vector2>[] uv, JobHandle[] uvCalculateJobHandles)
        {
            _meshData = meshData;
            _uv = uv;
            _uvCalculateJobHandles = uvCalculateJobHandles;
        }

        public NativeArray<Vector2>[] OutputUV()
        {
            foreach (var jh in _uvCalculateJobHandles) { jh.Complete(); }
            return _uv;
        }
        public void Dispose()
        {
            for (var i = 0; _uvCalculateJobHandles.Length > i; i += 1)
            {
                _uvCalculateJobHandles[i].Complete();
                _uv[i].Dispose();
            }
        }
    }

    internal class IslandSelectFilter : ITrianglesFilter<SingleGradientSpace, SingleGradientFilteredTrianglesHolder>
    {
        IIslandSelector? _islandSelector;
        OriginEqual _originEqual;

        public IslandSelectFilter(IIslandSelector? islandSelector, OriginEqual originEqual)
        {
            _islandSelector = islandSelector;
            _originEqual = originEqual;
        }


        public SingleGradientFilteredTrianglesHolder Filtering(SingleGradientSpace space)
        {
            return new(IslandSelectToPPFilter.IslandSelectExecute(_islandSelector, space._meshData, _originEqual));
        }
    }
    internal class SingleGradientFilteredTrianglesHolder : IFilteredTriangleHolder
    {
        NativeArray<TriangleIndex>[][] _triangles;
        public SingleGradientFilteredTrianglesHolder(NativeArray<TriangleIndex>[][] triangles)
        {
            _triangles = triangles;
        }
        public NativeArray<TriangleIndex>[][] GetTriangles()
        {
            return _triangles;
        }
        public void Dispose()
        {
            for (var i = 0; _triangles.Length > i; i += 1)
                for (var s = 0; _triangles[i].Length > s; s += 1)
                    _triangles[i][s].Dispose();
        }
    }

}
