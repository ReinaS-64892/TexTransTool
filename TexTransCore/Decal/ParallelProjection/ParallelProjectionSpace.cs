using UnityEngine;
using System.Collections.Generic;

namespace net.rs64.TexTransCore.Decal
{
    public class ParallelProjectionSpace : DecalUtility.IConvertSpace
    {
        public Matrix4x4 ParallelProjectionMatrix;
        public List<Vector3> PPSVert;
        public DecalUtility.MeshData MeshData;
        public ParallelProjectionSpace(Matrix4x4 ParallelProjectionMatrix)
        {
            this.ParallelProjectionMatrix = ParallelProjectionMatrix;

        }
        public void Input(DecalUtility.MeshData meshData)
        {
            MeshData = meshData;
            PPSVert = DecalUtility.ConvertVerticesInMatrix(ParallelProjectionMatrix, meshData.Vertex as IEnumerable<Vector3>, new Vector3(0.5f, 0.5f, 0));
        }

        public List<Vector2> OutPutUV()
        {
            var UV = new List<Vector2>(PPSVert.Capacity);
            foreach (var vert in PPSVert)
            {
                UV.Add(vert);
            }
            return UV;
        }

    }
}

