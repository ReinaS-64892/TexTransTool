using UnityEngine;
using System.Collections.Generic;

namespace net.rs64.TexTransCore.Decal
{
    public class ParallelProjectionSpace : DecalUtility.IConvertSpace<Vector3>
    {
        internal Matrix4x4 ParallelProjectionMatrix;
        internal List<Vector3> PPSVert;
        internal DecalUtility.MeshData MeshData;
        internal ParallelProjectionSpace(Matrix4x4 parallelProjectionMatrix)
        {
            ParallelProjectionMatrix = parallelProjectionMatrix;

        }
        public void Input(DecalUtility.MeshData meshData)
        {
            MeshData = meshData;
            PPSVert = DecalUtility.ConvertVerticesInMatrix(ParallelProjectionMatrix, meshData.Vertex as IEnumerable<Vector3>, new Vector3(0.5f, 0.5f, 0));
        }

        public List<Vector3> OutPutUV(List<Vector3> output = null) => PPSVert;

    }
}

