#nullable enable
using UnityEngine;
using net.rs64.TexTransTool.IslandSelector;
using UnityEngine.Profiling;
using Unity.Collections;
using System.Collections;
using net.rs64.TexTransCore;
using System.Collections.Generic;
using System.Linq;
using System;
using net.rs64.TexTransCore.UVIsland;
using net.rs64.TexTransTool.UVIsland;

namespace net.rs64.TexTransTool.Decal
{
    internal class IslandSelectToPPFilter : ITrianglesFilter<ParallelProjectionSpace, IFilteredTriangleHolder>
    {
        JobChain<FilterTriangleJobInput<NativeArray<Vector3>>>[] _filters;
        IIslandSelector _islandSelector;
        IDomainReferenceViewer _targeting;


        public IslandSelectToPPFilter(IIslandSelector islandSelector, JobChain<FilterTriangleJobInput<NativeArray<Vector3>>>[] filters, IDomainReferenceViewer targeting)
        {
            _islandSelector = islandSelector;

            _filters = filters;
            _targeting = targeting;
        }

        public IFilteredTriangleHolder Filtering(ParallelProjectionSpace space)
        {
            var islandSelectedTriangles = IslandSelectExecute(_islandSelector, space._meshData, _targeting).Take();

            var filteredBitJobs = new JobResult<NativeArray<bool>>[space._meshData.Length][];
            for (var i = 0; filteredBitJobs.Length > i; i += 1)
            {
                var md = space._meshData[i];
                var filteredBit = new JobResult<NativeArray<bool>>[md.SubMeshCount];
                for (var s = 0; md.SubMeshCount > s; s += 1)
                {
                    var triNa = islandSelectedTriangles[i][s];
                    var jobResult = filteredBit[s] = TriangleFilterUtility.FilteringTriangle(triNa, space._ppsUVArray[i], _filters, space._uvCalculateJobHandles[i]);

                    space._uvCalculateJobHandles[i] = jobResult.GetHandle;
                }
                filteredBitJobs[i] = filteredBit;
            }
            return new IslandSelectedJobChainedFilteredTrianglesHolder(islandSelectedTriangles, filteredBitJobs);
        }
        internal static IslandSelectedTriangleHolder IslandSelectExecute(IIslandSelector? islandSelector, MeshData[] meshData, IDomainReferenceViewer targeting)
        {
            if (islandSelector == null)
            {
                var tri = new NativeArray<TriangleVertexIndices>[meshData.Length][];
                for (var i = 0; tri.Length > i; i += 1)
                {
                    var md = meshData[i];
                    var tr = tri[i] = new NativeArray<TriangleVertexIndices>[md.SubMeshCount];
                    for (var s = 0; tr.Length > s; s += 1)
                    {
                        tr[s] = new NativeArray<TriangleVertexIndices>(md.TriangleVertexIndices[s], Allocator.TempJob);
                    }
                }
                return new(tri);
            }
            var islandsArray = IslandsArrayFromMeshData(meshData);

            Profiler.BeginSample("IslandSelect");
            var bitArray = islandSelector.IslandSelect(new(islandsArray.flattenIslands, islandsArray.flattenIslandDescription, targeting));
            Profiler.EndSample();

            Profiler.BeginSample("FilterTriangle");
            var triList = IslandSelectToTriangleVertexIndices(islandsArray.allMeshIslands, islandsArray.islandToIndex, bitArray);
            Profiler.EndSample();

            return triList;
        }
        internal struct IslandSelectedTriangleHolder : IDisposable
        {
            NativeArray<TriangleVertexIndices>[][]? Triangles;
            public IslandSelectedTriangleHolder(NativeArray<TriangleVertexIndices>[][] triangles)
            {
                Triangles = triangles;
            }
            public NativeArray<TriangleVertexIndices>[][] Ref()
            {
                if (Triangles is null) { throw new InvalidProgramException(); }
                return Triangles;
            }
            public NativeArray<TriangleVertexIndices>[][] Take()
            {
                if (Triangles is null) { throw new InvalidProgramException(); }
                var tri = Triangles;
                Triangles = null;
                return tri;
            }
            public void Dispose()
            {
                if (Triangles is null) { return; }
                for (var ri = 0; Triangles.Length > ri; ri += 1)
                {
                    var subMeshTriangles = Triangles[ri];
                    for (var si = 0; subMeshTriangles.Length > si; si += 1)
                    {
                        subMeshTriangles[si].Dispose();
                    }
                }
                Triangles = null!;
            }
        }
        internal struct IslandsArray
        {
            public Island[][][] allMeshIslands;
            public Island[] flattenIslands;
            public Dictionary<Island, int> islandToIndex;
            public IslandDescription[] flattenIslandDescription;
        }
        internal static IslandsArray IslandsArrayFromMeshData(MeshData[] meshData)
        {
            var allMeshIslands = new Island[meshData.Length][][];
            for (var i = 0; allMeshIslands.Length > i; i += 1)
            {
                var md = meshData[i];
                var mi = allMeshIslands[i] = new Island[md.TriangleVertexIndices.Length][];
                for (var s = 0; mi.Length > s; s += 1) { mi[s] = UnityIslandUtility.UVtoIsland(md, s).ToArray(); }
            }

            var flattenIslands = allMeshIslands.SelectMany(i => i.SelectMany(i2 => i2)).ToArray();
            var islandToIndex = new Dictionary<Island, int>(flattenIslands.Length);
            {
                var index = 0;
                foreach (var subIslands in allMeshIslands)
                    foreach (var islands in subIslands)
                        foreach (var island in islands)
                        {
                            islandToIndex[island] = index;
                            index += 1;
                        }
            }


            Profiler.BeginSample("CreateIslandDescription");
            var flattenIslandDescriptions = new IslandDescription[meshData.Length][];

            var sharedMaterialsCache = new Dictionary<Renderer, Material[]>();

            for (var i = 0; flattenIslandDescriptions.Length > i; i += 1)
            {
                var md = meshData[i];
                var id = flattenIslandDescriptions[i] = new IslandDescription[md.TriangleVertexIndices.Length];
                for (var s = 0; id.Length > s; s += 1)
                {
                    if (sharedMaterialsCache.TryGetValue(md.ReferenceRenderer, out var rMats) is false)
                        rMats = sharedMaterialsCache[md.ReferenceRenderer] = md.ReferenceRenderer.sharedMaterials;

                    id[s] = new IslandDescription(md.Vertices, md.VertexUV, md.ReferenceRenderer, rMats, s);
                }
            }

            var flattenIslandDescription = new IslandDescription[flattenIslands.Length];
            var writeIndex = 0;
            for (var m = 0; allMeshIslands.Length > m; m += 1)
            {
                var mgIslands = allMeshIslands[m];
                for (var s = 0; mgIslands.Length > s; s += 1)
                {
                    var subMeshIslands = mgIslands[s];
                    var writeSpan = flattenIslandDescription.AsSpan(writeIndex, subMeshIslands.Length);
                    writeSpan.Fill(flattenIslandDescriptions[m][s]);
                    writeIndex += writeSpan.Length;
                }
            }
            Profiler.EndSample();

            return new()
            {
                allMeshIslands = allMeshIslands,
                flattenIslands = flattenIslands,
                flattenIslandDescription = flattenIslandDescription,
                islandToIndex = islandToIndex,
            };
        }

        internal static IslandSelectedTriangleHolder IslandSelectToTriangleVertexIndices(Island[][][] allMeshIslands, Dictionary<Island, int> islandToIndex, BitArray bitArray)
        {
            var allSelectedTriangles = new NativeArray<TriangleVertexIndices>[allMeshIslands.Length][];
            for (var i = 0; allSelectedTriangles.Length > i; i += 1)
            {
                var mi = allMeshIslands[i];
                var sm = allSelectedTriangles[i] = new NativeArray<TriangleVertexIndices>[mi.Length];
                for (var s = 0; sm.Length > s; s += 1)
                {
                    var selectedIslands = mi[s].Where(island => bitArray[islandToIndex[island]]).ToArray();
                    var triCount = selectedIslands.Sum(i => i.Triangles.Count);

                    if (triCount == 0) { sm[s] = new NativeArray<TriangleVertexIndices>(0, Allocator.TempJob); continue; }

                    var triangles = sm[s] = new NativeArray<TriangleVertexIndices>(triCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

                    var wPos = 0;
                    foreach (var island in selectedIslands)
                    {
                        foreach (var tri in island.Triangles)
                        {
                            triangles[wPos] = tri;
                            wPos += 1;
                        }
                    }

                }
            }

            return new(allSelectedTriangles);
        }

    }

    internal class IslandSelectedJobChainedFilteredTrianglesHolder : IFilteredTriangleHolder
    {
        // owned
        NativeArray<TriangleVertexIndices>[][] _islandSelectedTriangles;
        JobResult<NativeArray<bool>>[][] _filteredBitJobs;
        NativeArray<TriangleVertexIndices>[][]? _filteredTriangles;
        public IslandSelectedJobChainedFilteredTrianglesHolder(NativeArray<TriangleVertexIndices>[][] islandSelectedTriangles, JobResult<NativeArray<bool>>[][] filteredBitJobs)
        {
            _islandSelectedTriangles = islandSelectedTriangles;
            _filteredBitJobs = filteredBitJobs;
        }
        public NativeArray<TriangleVertexIndices>[][] GetTriangles()
        {
            if (_filteredTriangles is null)
            {
                _filteredTriangles = new NativeArray<TriangleVertexIndices>[_filteredBitJobs.Length][];
                for (var i = 0; _filteredBitJobs.Length > i; i += 1)
                {
                    var bitJobs = _filteredBitJobs[i];
                    var triangles = new NativeArray<TriangleVertexIndices>[bitJobs.Length];
                    for (var s = 0; bitJobs.Length > s; s += 1)
                    {
                        triangles[s] = JobChainedFilteredTrianglesHolder.FilteringExecute(_islandSelectedTriangles[i][s], bitJobs[s].GetResult);
                    }
                    _filteredTriangles[i] = triangles;
                }
            }
            return _filteredTriangles;
        }
        public void Dispose()
        {
            for (var i = 0; _islandSelectedTriangles.Length > i; i += 1)
            {
                var st = _islandSelectedTriangles[i];
                for (var s = 0; st.Length > s; s += 1)
                {
                    st[s].Dispose();
                }
            }
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
            for (var i = 0; _filteredBitJobs.Length > i; i += 1)
            {
                var sb = _filteredBitJobs[i];
                for (var s = 0; sb.Length > s; s += 1)
                {
                    sb[s].GetResult.Dispose();
                }
            }

        }

    }
}
