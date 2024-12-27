using UnityEngine;
using net.rs64.TexTransTool.Utils;
using net.rs64.TexTransCoreEngineForUnity;
using net.rs64.TexTransTool.IslandSelector;
using UnityEngine.Profiling;
using Unity.Collections;
using System.Collections;
using net.rs64.TexTransCore;
using net.rs64.TexTransTool.UVIsland;

namespace net.rs64.TexTransTool.Decal
{
    internal class IslandSelectToPPFilter : ITrianglesFilter<ParallelProjectionSpace>
    {
        public JobChain<FilterTriangleJobInput<NativeArray<Vector3>>>[] Filters;
        public IIslandSelector IslandSelector;
        public OriginEqual OriginEqual;


        public IslandSelectToPPFilter(IIslandSelector islandSelector, JobChain<FilterTriangleJobInput<NativeArray<Vector3>>>[] filters, OriginEqual originEqual)
        {
            IslandSelector = islandSelector;
            Filters = filters;
            OriginEqual = originEqual;
        }

        ParallelProjectionSpace _ppSpace;

        NativeArray<TriangleIndex>[] _islandSelectedTriangles;
        JobResult<NativeArray<bool>>[] _filteredBit;
        NativeArray<TriangleIndex>[] _filteredTriangles;

        public void SetSpace(ParallelProjectionSpace space)
        {
            _ppSpace = space;

            var smCount = _ppSpace.MeshData.TriangleIndex.Length;
            _islandSelectedTriangles = new NativeArray<TriangleIndex>[smCount];
            _filteredBit = new JobResult<NativeArray<bool>>[smCount];
            _filteredTriangles = new NativeArray<TriangleIndex>[smCount];
            for (var i = 0; smCount > i; i += 1)
            {
                var islandSelected = _islandSelectedTriangles[i] = IslandSelectExecute(IslandSelector, _ppSpace.MeshData, i, OriginEqual);
                if (islandSelected.Length == 0) { continue; }
                var ppsVert = _ppSpace.GetPPSVert;
                _filteredBit[i] = TriangleFilterUtility.FilteringTriangle(islandSelected, ppsVert, Filters);
            }
        }
        NativeArray<TriangleIndex> ITrianglesFilter<ParallelProjectionSpace>.GetFilteredSubTriangle(int subMeshIndex)
        {
            if (_ppSpace is null) { return default; }
            if (_islandSelectedTriangles[subMeshIndex].Length == 0) { return default; }
            if (_filteredTriangles[subMeshIndex].IsCreated) { return _filteredTriangles[subMeshIndex]; }
            var filteredTriangle = _filteredTriangles[subMeshIndex] = ParallelProjectionFilter.FilteringExecute(_islandSelectedTriangles[subMeshIndex], _filteredBit[subMeshIndex].GetResult);
            return filteredTriangle;
        }

        internal static NativeArray<TriangleIndex> IslandSelectExecute(IIslandSelector islandSelector, MeshData meshData, int subMeshIndex, OriginEqual originEqual)
        {
            if (islandSelector == null) { return new NativeArray<TriangleIndex>(meshData.TriangleIndex[subMeshIndex], Allocator.TempJob); }
            Island[] islands = (subMeshIndex, meshData).Memo(GetIslands);

            Profiler.BeginSample("CreateIslandDescription");
            var description = new IslandDescription(meshData.Vertices, meshData.VertexUV, meshData.ReferenceRenderer, subMeshIndex);
            var islandDescription = new IslandDescription[islands.Length];
            for (var i = 0; islands.Length > i; i += 1) { islandDescription[i] = description; }
            Profiler.EndSample();

            Profiler.BeginSample("IslandSelect");
            var bitArray = islandSelector.IslandSelect(new(islands, islandDescription, originEqual));
            Profiler.EndSample();

            Profiler.BeginSample("FilterTriangle");
            var triList = IslandSelectToTriangleIndex(islands, bitArray);
            Profiler.EndSample();

            return triList;
        }

        private static NativeArray<TriangleIndex> IslandSelectToTriangleIndex(Island[] islands, BitArray bitArray)
        {
            var triCount = 0;
            for (var i = 0; islands.Length > i; i += 1) { if (bitArray[i]) { triCount += islands[i].triangles.Count; } }
            if (triCount == 0) { return default; }
            var list = new NativeArray<TriangleIndex>(triCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

            var writePos = 0;
            for (var i = 0; islands.Length > i; i += 1)
            {
                if (!bitArray[i]) { continue; }
                foreach (var tri in islands[i].triangles)
                {
                    list[writePos] = tri;
                    writePos += 1;
                }
            }
            return list;
        }

        static Island[] GetIslands((int subMeshIndex, MeshData meshData) pair)
        {
            return IslandUtility.UVtoIsland(pair.meshData.TriangleIndex[pair.subMeshIndex].AsList(), pair.meshData.VertexUV.AsList()).ToArray();
        }

        public void Dispose()
        {
            foreach (var na in _filteredBit) { na?.GetResult.Dispose(); }
            _filteredBit = null;
            foreach (var na in _islandSelectedTriangles) { na.Dispose(); }
            _islandSelectedTriangles = null;
            foreach (var na in _filteredTriangles) { na.Dispose(); }
            _filteredTriangles = null;
        }
    }
}
