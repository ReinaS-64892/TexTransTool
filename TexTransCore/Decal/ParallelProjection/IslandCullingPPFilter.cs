using UnityEngine;
using System.Collections.Generic;
using net.rs64.TexTransCore.TransTextureCore;
using net.rs64.TexTransCore.Island;

namespace net.rs64.TexTransCore.Decal
{
    public class IslandCullingPPFilter<UVDimension> : ParallelProjectionFilter<UVDimension>
    where UVDimension : struct
    {
        public List<IslandSelector> IslandSelectors;
        public IIslandCache IslandCache;

        public IslandCullingPPFilter(List<TriangleFilterUtility.ITriangleFiltering<List<Vector3>>> Filters, List<IslandSelector> IslandSelectors, IIslandCache islandCache = null) : base(Filters)
        {
            this.IslandSelectors = IslandSelectors;
            this.IslandCache = islandCache;
        }

        public override List<TriangleIndex> Filtering(ParallelProjectionSpace Space, List<TriangleIndex> Triangles)
        {
            Triangles = Island.IslandCulling.Culling(IslandSelectors, Space.MeshData.Vertex, Space.MeshData.UV, Triangles, IslandCache);
            return base.Filtering(Space, Triangles);
        }

    }
}


