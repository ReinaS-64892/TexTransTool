using System.Collections.Generic;
using UnityEngine;
using net.rs64.TexTransCore.Decal;
using net.rs64.TexTransTool.Utils;
using net.rs64.TexTransCore.Island;
using net.rs64.TexTransCore.TransTextureCore;
using net.rs64.TexTransTool.IslandSelector;
using System.Linq;
using UnityEngine.Profiling;
using net.rs64.TexTransCore;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Collections;

namespace net.rs64.TexTransTool.Decal
{
    internal class IslandSelectToPPFilter : ITrianglesFilter<ParallelProjectionSpace>
    {
        public JobChain<FilterTriangleJobInput<NativeArray<Vector3>>>[] Filters;
        public IIslandSelector IslandSelector;


        public IslandSelectToPPFilter(IIslandSelector islandSelector, JobChain<FilterTriangleJobInput<NativeArray<Vector3>>>[] filters)
        {
            IslandSelector = islandSelector;
            Filters = filters;
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
                var islandSelected = _islandSelectedTriangles[i] = IslandSelectExecute(i);
                var ppsVert = _ppSpace.GetPPSVert;
                _filteredBit[i] = TriangleFilterUtility.FilteringTriangle(islandSelected, ppsVert, Filters);
            }
        }
        NativeArray<TriangleIndex> ITrianglesFilter<ParallelProjectionSpace>.GetFilteredSubTriangle(int subMeshIndex)
        {
            if (_ppSpace is null) { return default; }
            var filteredTriangle = _filteredTriangles[subMeshIndex] = ParallelProjectionFilter.FilteringExecute(_ppSpace.MeshData.TriangleIndex[subMeshIndex], _filteredBit[subMeshIndex].GetResult);
            return filteredTriangle;
        }

        private NativeArray<TriangleIndex> IslandSelectExecute(int subMeshIndex)
        {
            var meshData = _ppSpace.MeshData;
            Island[] islands = (subMeshIndex, meshData).Memo(GetIslands);

            Profiler.BeginSample("CreateIslandDescription");
            var description = new IslandDescription(meshData.Vertices, meshData.VertexUV, meshData.ReferenceRenderer, subMeshIndex);
            var islandDescription = new IslandDescription[islands.Length];
            for (var i = 0; islands.Length > i; i += 1) { islandDescription[i] = description; }
            Profiler.EndSample();

            Profiler.BeginSample("IslandSelect");
            var bitArray = IslandSelector.IslandSelect(islands, islandDescription);
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
            foreach (var na in _filteredBit) { na.GetResult.Dispose(); }
            foreach (var na in _islandSelectedTriangles) { na.Dispose(); }
            foreach (var na in _filteredTriangles) { na.Dispose(); }
        }
    }
}
