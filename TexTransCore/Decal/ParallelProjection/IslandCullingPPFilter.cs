using UnityEngine;
using System.Collections.Generic;
using net.rs64.TexTransCore.TransTextureCore;
using net.rs64.TexTransCore.Island;
using UnityEngine.Pool;

namespace net.rs64.TexTransCore.Decal
{
    internal class IslandCullingPPFilter<UVDimension> : ParallelProjectionFilter<UVDimension>
    where UVDimension : struct
    {
        public List<IslandSelector> IslandSelectors;
        public IIslandCache IslandCache;

        public IslandCullingPPFilter(List<TriangleFilterUtility.ITriangleFiltering<List<Vector3>>> Filters, List<IslandSelector> IslandSelectors, IIslandCache islandCache = null) : base(Filters)
        {
            this.IslandSelectors = IslandSelectors;
            this.IslandCache = islandCache;
        }

        public override List<TriangleIndex> Filtering(ParallelProjectionSpace Space, List<TriangleIndex> Triangles, List<TriangleIndex> output = null)
        {
            var cullied = ListPool<TriangleIndex>.Get();
            Triangles = Island.IslandCulling.Culling(IslandSelectors, Space.MeshData.Vertex, Space.MeshData.UV, Triangles, IslandCache);
            var result = base.Filtering(Space, Triangles, output);
            ListPool<TriangleIndex>.Release(cullied);
            return result;
        }

    }
}


