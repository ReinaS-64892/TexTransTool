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

namespace net.rs64.TexTransTool.Decal
{
    internal class IslandSelectToPPFilter : DecalUtility.ITrianglesFilter<ParallelProjectionSpace>
    {
        public List<TriangleFilterUtility.ITriangleFiltering<List<Vector3>>> Filters;
        public AbstractIslandSelector IslandSelector;


        public IslandSelectToPPFilter(AbstractIslandSelector islandSelector, List<TriangleFilterUtility.ITriangleFiltering<List<Vector3>>> filters)
        {
            IslandSelector = islandSelector;
            Filters = filters;
        }

        ParallelProjectionSpace _parallelProjectionSpace;

        public void SetSpace(ParallelProjectionSpace space) { _parallelProjectionSpace = space; }

        public List<TriangleIndex> GetFilteredSubTriangle(int subMeshIndex)
        {
            if (_parallelProjectionSpace is null) { return null; }

            var meshData = _parallelProjectionSpace.MeshData;
            var islands = IslandUtility.UVtoIsland(meshData.TrianglesSubMesh[subMeshIndex], meshData.UV);

            var description = new AbstractIslandSelector.IslandDescription(meshData.Vertex.ToArray(), meshData.UV.ToArray(), meshData.RendererRef, subMeshIndex);

            var islandsCount = islands.Count;
            var islandDict = new Dictionary<int, Island>(islandsCount);
            var islandDescription = new Dictionary<int, AbstractIslandSelector.IslandDescription>(islandsCount);
            for (var i = 0; islandsCount > i; i += 1)
            {
                islandDict[i] = islands[i];
                islandDescription[i] = description;
            }

            var hash = IslandSelector.IslandSelect(islandDict, islandDescription);

            return TriangleFilterUtility.FilteringTriangle(hash.SelectMany(i => islandDict[i].triangles).ToList(), _parallelProjectionSpace.PPSVert, Filters);
        }
    }
}
