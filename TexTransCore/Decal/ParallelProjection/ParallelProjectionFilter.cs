using UnityEngine;
using System.Collections.Generic;
using net.rs64.TexTransCore.TransTextureCore;

namespace net.rs64.TexTransCore.Decal
{
    internal class ParallelProjectionFilter : DecalUtility.ITrianglesFilter<ParallelProjectionSpace>
    {
        public List<TriangleFilterUtility.ITriangleFiltering<List<Vector3>>> Filters;
        ParallelProjectionSpace _parallelProjectionSpace;

        public ParallelProjectionFilter(List<TriangleFilterUtility.ITriangleFiltering<List<Vector3>>> filters) { Filters = filters; }
        public void SetSpace(ParallelProjectionSpace space) { _parallelProjectionSpace = space; }

        public List<TriangleIndex> GetFilteredSubTriangle(int subMeshIndex)
        {
            if (_parallelProjectionSpace is null) { return null; }
            return TriangleFilterUtility.FilteringTriangle(_parallelProjectionSpace.MeshData.TrianglesSubMesh[subMeshIndex], _parallelProjectionSpace.PPSVert, Filters);
        }

    }
}

