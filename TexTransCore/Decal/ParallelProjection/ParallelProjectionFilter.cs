using UnityEngine;
using System.Collections.Generic;
using net.rs64.TexTransCore.TransTextureCore;

namespace net.rs64.TexTransCore.Decal
{
    internal class ParallelProjectionFilter<UVDimension> : DecalUtility.ITrianglesFilter<ParallelProjectionSpace>
    where UVDimension : struct
    {
        public List<TriangleFilterUtility.ITriangleFiltering<List<Vector3>>> Filters;

        public ParallelProjectionFilter(List<TriangleFilterUtility.ITriangleFiltering<List<Vector3>>> filters)
        {
            Filters = filters;
        }

        public virtual List<TriangleIndex> Filtering(ParallelProjectionSpace space, List<TriangleIndex> triangles, List<TriangleIndex> output = null)
        {
            return TriangleFilterUtility.FilteringTriangle(triangles, space.PPSVert, Filters, output);
        }
    }
}

