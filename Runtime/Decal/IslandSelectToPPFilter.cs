using System.Collections.Generic;
using UnityEngine;
using net.rs64.TexTransTool.Decal.Cylindrical;
using net.rs64.TexTransCore.Decal;
using net.rs64.TexTransTool.Utils;
using UnityEngine.Serialization;
using net.rs64.TexTransCore.Island;
using net.rs64.TexTransCore.TransTextureCore;
using net.rs64.TexTransTool.IslandSelector;
using System.Linq;
using UnityEngine.Profiling;

namespace net.rs64.TexTransTool.Decal
{
    internal class IslandSelectToPPFilter : DecalUtility.ITrianglesFilter<ParallelProjectionSpace>
    {
        public List<TriangleFilterUtility.ITriangleFiltering<IList<Vector3>>> Filters;
        public IIslandSelector IslandSelector;


        public IslandSelectToPPFilter(IIslandSelector islandSelector, List<TriangleFilterUtility.ITriangleFiltering<IList<Vector3>>> filters)
        {
            IslandSelector = islandSelector;
            Filters = filters;
        }

        ParallelProjectionSpace _ppSpace;

        public void SetSpace(ParallelProjectionSpace space) { _ppSpace = space; }

        //これSetされた瞬間からマルチスレッドでにフィルタリングを走らせるのもありなのではないか？

        public List<TriangleIndex> GetFilteredSubTriangle(int subMeshIndex)
        {
            if (_ppSpace is null) { return null; }

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
            var linkList = new LinkedList<TriangleIndex>();
            for (var i = 0; islands.Length > i; i += 1)
            {
                if (!bitArray[i]) { continue; }
                var island = islands[i];
                var triCount = island.triangles.Count;
                for (var triIndex = 0; triCount > triIndex; triIndex += 1)
                {
                    linkList.AddLast(island.triangles[triIndex]);
                }
            }
            Profiler.EndSample();

            return TriangleFilterUtility.FilteringTriangle(linkList.ToList(), _ppSpace.GetPPSVert.AsList(), Filters);

            static Island[] GetIslands((int subMeshIndex, MeshData meshData) pair)
            {
                return IslandUtility.UVtoIsland(pair.meshData.TriangleIndex[pair.subMeshIndex].AsList(), pair.meshData.UVList).ToArray();
            }
        }
    }
}
