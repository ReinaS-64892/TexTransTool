#nullable enable
using UnityEngine;
using System.Collections.Generic;
using net.rs64.TexTransCoreEngineForUnity;
using net.rs64.TexTransTool.Utils;
using Unity.Collections;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine.Profiling;
using Unity.Jobs;
using net.rs64.TexTransCore;

namespace net.rs64.TexTransTool.Decal
{
    internal class ParallelProjectionFilter : ITrianglesFilter<ParallelProjectionSpace, IFilteredTriangleHolder>
    {
        JobChain<FilterTriangleJobInput<NativeArray<Vector3>>>[] _filters;
        public ParallelProjectionFilter(JobChain<FilterTriangleJobInput<NativeArray<Vector3>>>[] filters) { _filters = filters; }
        public IFilteredTriangleHolder Filtering(ParallelProjectionSpace space)
        {
            var filteredBitJobs = new JobResult<NativeArray<bool>>[space._meshData.Length][];
            for (var i = 0; filteredBitJobs.Length > i; i += 1)
            {
                var md = space._meshData[i];
                var ppsVertex = space._ppsUVArray[i];
                var filteredBit = filteredBitJobs[i] = new JobResult<NativeArray<bool>>[md.SubMeshCount];
                for (var s = 0; filteredBit.Length > s; s += 1)
                {
                    var triNa = md.TriangleIndex[s];
                    var jobResult = filteredBit[s] = TriangleFilterUtility.FilteringTriangle(triNa, ppsVertex, _filters, space._uvCalculateJobHandles[i]);

                    space._uvCalculateJobHandles[i] = jobResult.GetHandle;
                }
            }
            return new JobChainedFilteredTrianglesHolder(space._meshData, filteredBitJobs);
        }
    }

    internal class JobChainedFilteredTrianglesHolder : IFilteredTriangleHolder
    {
        // not owned
        MeshData[] _meshData;
        // owned
        JobResult<NativeArray<bool>>[][] _filteredBitJobResults;
        NativeArray<TriangleIndex>[][]? _filteredTriangles;
        public JobChainedFilteredTrianglesHolder(MeshData[] meshData, JobResult<NativeArray<bool>>[][] filteredBitJobs)
        {
            _meshData = meshData;
            _filteredBitJobResults = filteredBitJobs;
        }
        public NativeArray<TriangleIndex>[][] GetTriangles()
        {
            if (_filteredTriangles is null)
            {
                _filteredTriangles = new NativeArray<TriangleIndex>[_filteredBitJobResults.Length][];
                for (var i = 0; _filteredBitJobResults.Length > i; i += 1)
                {
                    var md = _meshData[i];
                    var bitJobResult = _filteredBitJobResults[i];

                    var triangles = _filteredTriangles[i] = new NativeArray<TriangleIndex>[bitJobResult.Length];
                    for (var s = 0; bitJobResult.Length > s; s += 1)
                    {
                        triangles[s] = FilteringExecute(md.TriangleIndex[s], bitJobResult[s].GetResult);
                    }
                }
            }
            return _filteredTriangles;
        }
        internal static NativeArray<TriangleIndex> FilteringExecute(NativeArray<TriangleIndex> triangles, NativeArray<bool> FilterBit)
        {
            Profiler.BeginSample("ParallelProjectionFilter.FilteringExecute");
            using var filteredTriFullArrayNa = new NativeArray<TriangleIndex>(FilterBit.Length, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            var filteredTriFullArray = filteredTriFullArrayNa.AsSpan();

            var writeCount = 0;
            for (var i = 0; FilterBit.Length > i; i += 1)
            {
                if (FilterBit[i] is false)
                {
                    filteredTriFullArray[writeCount] = triangles[i];
                    writeCount += 1;
                }
            }

            var filtered = new NativeArray<TriangleIndex>(writeCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            Unsafe.UnsafeNativeArrayUtility.MemCpy(filtered, filteredTriFullArrayNa, writeCount);

            Profiler.EndSample();
            return filtered;
        }
        public void Dispose()
        {
            if (_filteredTriangles is not null)
            {
                for (var i = 0; _filteredTriangles.Length > i; i += 1)
                {
                    var st = _filteredTriangles[i];
                    for (var s = 0; st.Length > s; s += 1)
                    {
                        st[s].Dispose();
                    }
                }
            }
            for (var i = 0; _filteredBitJobResults.Length > i; i += 1)
            {
                var sb = _filteredBitJobResults[i];
                for (var s = 0; sb.Length > s; s += 1)
                {
                    sb[s].GetResult.Dispose();
                }
            }

        }

    }
}

