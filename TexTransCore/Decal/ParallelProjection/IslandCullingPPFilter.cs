using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransCore.TransTextureCore;
using net.rs64.TexTransCore.Island;
using UnityEngine.Pool;

namespace net.rs64.TexTransCore.Decal
{
    internal class IslandCullingPPFilter<UVDimension> : ParallelProjectionFilter<UVDimension>
    where UVDimension : struct
    {
        public List<IslandSelector> IslandSelectors;

        public IslandCullingPPFilter(List<TriangleFilterUtility.ITriangleFiltering<IList<Vector3>>> filters, List<IslandSelector> islandSelectors) : base(filters)
        {
            IslandSelectors = islandSelectors;
        }

        public override List<TriangleIndex> Filtering(ParallelProjectionSpace space, List<TriangleIndex> triangles, List<TriangleIndex> output = null)
        {
            var cullied = ListPool<TriangleIndex>.Get();
            triangles = Island.IslandCulling.Culling(IslandSelectors, space.MeshData).Intersect(triangles).ToList();
            var result = base.Filtering(space, triangles, output);
            ListPool<TriangleIndex>.Release(cullied);
            return result;
        }

    }
}


