using UnityEngine;
using System.Collections.Generic;
using net.rs64.TexTransCoreEngineForUnity;
using net.rs64.TexTransTool.Utils;
using Unity.Collections;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine.Profiling;
using Unity.Jobs;

namespace net.rs64.TexTransCoreEngineForUnity.Decal
{
    internal class ParallelProjectionFilter : ITrianglesFilter<ParallelProjectionSpace>
    {
        public JobChain<FilterTriangleJobInput<NativeArray<Vector3>>>[] Filters;
        ParallelProjectionSpace _parallelProjectionSpace;
        private JobResult<NativeArray<bool>>[] _filteredBit;
        private NativeArray<TriangleIndex>[] _filteredTriangles;

        public void SetSpace(ParallelProjectionSpace space)
        {
            _parallelProjectionSpace = space;

            var smCount = _parallelProjectionSpace.MeshData.TriangleIndex.Length;
            _filteredBit = new JobResult<NativeArray<bool>>[smCount];
            _filteredTriangles = new NativeArray<TriangleIndex>[smCount];
            for (var i = 0; smCount > i; i += 1)
            {
                var triNa = _parallelProjectionSpace.MeshData.TriangleIndex[i];
                var ppsVert = _parallelProjectionSpace.GetPPSVert;
                _filteredBit[i] = TriangleFilterUtility.FilteringTriangle(triNa, ppsVert, Filters);
            }
        }

        public ParallelProjectionFilter(JobChain<FilterTriangleJobInput<NativeArray<Vector3>>>[] filters) { Filters = filters; }
        NativeArray<TriangleIndex> ITrianglesFilter<ParallelProjectionSpace>.GetFilteredSubTriangle(int subMeshIndex)
        {
            if (_parallelProjectionSpace is null) { return default; }
            if (_filteredTriangles[subMeshIndex].IsCreated) { return _filteredTriangles[subMeshIndex]; }
            var filteredTriangle = _filteredTriangles[subMeshIndex] = FilteringExecute(_parallelProjectionSpace.MeshData.TriangleIndex[subMeshIndex], _filteredBit[subMeshIndex].GetResult);
            return filteredTriangle;
        }
        internal static NativeArray<TriangleIndex> FilteringExecute(NativeArray<TriangleIndex> triangles, NativeArray<bool> FilterBit)
        {
            Profiler.BeginSample("ParallelProjectionFilter.FilteringExecute");
            var filteredTriFullArray = new NativeArray<TriangleIndex>(FilterBit.Length, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

            var writePos = 0;
            for (var i = 0; FilterBit.Length > i; i += 1)
            {
                if (!FilterBit[i])
                {
                    filteredTriFullArray[writePos] = triangles[i];
                    writePos += 1;
                }
            }

            var filtered = new NativeArray<TriangleIndex>(writePos, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            net.rs64.TexTransCoreEngineForUnity.Unsafe.UnsafeNativeArrayUtility.MemCpy(filteredTriFullArray, filtered, writePos);
            filteredTriFullArray.Dispose();

            Profiler.EndSample();
            return filtered;
        }
        public void Dispose()
        {
            foreach (var na in _filteredBit) { na.GetResult.Dispose(); }
            _filteredBit = null;
            foreach (var na in _filteredTriangles) { na.Dispose(); }
            _filteredTriangles = null;
        }
    }
}

