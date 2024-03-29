using System;
using UnityEngine;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;

namespace net.rs64.TexTransCore.Decal
{
    public class ParallelProjectionSpace : DecalUtility.IConvertSpace<Vector3>
    {
        internal Matrix4x4 ParallelProjectionMatrix;
        internal MeshData MeshData;

        private JobHandle _jobHandle;
        private NativeArray<Vector3> PPSVert;
        
        internal ParallelProjectionSpace(Matrix4x4 parallelProjectionMatrix)
        {
            ParallelProjectionMatrix = parallelProjectionMatrix;

        }
        public void Input(MeshData meshData)
        {
            MeshData = meshData;
            PPSVert = DecalUtility.ConvertVerticesInMatrix(ParallelProjectionMatrix, meshData, new Vector3(0.5f, 0.5f, 0), out _jobHandle);
        }

        public NativeArray<Vector3> OutPutUV()
        {
            _jobHandle.Complete();
            return PPSVert;
        } 

        public void Dispose()
        {
            MeshData?.Dispose();
        }
    }
}

