using System;
using UnityEngine;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using net.rs64.TexTransUnityCore.Utils;

namespace net.rs64.TexTransUnityCore.Decal
{
    public class ParallelProjectionSpace : IConvertSpace<Vector3>
    {
        internal Matrix4x4 ParallelProjectionMatrix;

        internal MeshData MeshData;
        private JobHandle _jobHandle;
        private NativeArray<Vector3> PPSVert;
        internal NativeArray<Vector3> GetPPSVert { get { _jobHandle.Complete(); return PPSVert; } }

        internal ParallelProjectionSpace(Matrix4x4 parallelProjectionMatrix)
        {
            ParallelProjectionMatrix = parallelProjectionMatrix;

        }
        public void Input(MeshData meshData)
        {
            MeshData = meshData;
            PPSVert = VectorUtility.ConvertVerticesInMatrix(ParallelProjectionMatrix, meshData, new Vector3(0.5f, 0.5f, 0), out _jobHandle);
        }

        public NativeArray<Vector3> OutPutUV() => GetPPSVert;

        public void Dispose()
        {
            MeshData = null;
            _jobHandle.Complete();
            PPSVert.Dispose();
            _jobHandle = default;
            PPSVert = default;
        }
    }
}

