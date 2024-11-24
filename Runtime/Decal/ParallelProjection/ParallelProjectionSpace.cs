using System;
using UnityEngine;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using net.rs64.TexTransTool.Utils;

namespace net.rs64.TexTransTool.Decal
{
    public class ParallelProjectionSpace : IConvertSpace
    {
        internal Matrix4x4 ParallelProjectionMatrix;

        internal MeshData MeshData;
        private JobHandle _jobHandle;
        private NativeArray<Vector3> PPSVert;
        private NativeArray<Vector2> PPSVertWithUV;
        internal NativeArray<Vector3> GetPPSVert { get { _jobHandle.Complete(); return PPSVert; } }
        internal NativeArray<Vector2> GetUVVert { get { _jobHandle.Complete(); return PPSVertWithUV; } }

        internal ParallelProjectionSpace(Matrix4x4 parallelProjectionMatrix)
        {
            ParallelProjectionMatrix = parallelProjectionMatrix;

        }
        public void Input(MeshData meshData)
        {
            MeshData = meshData;
            PPSVert = VectorUtility.ConvertVerticesInMatrix(ParallelProjectionMatrix, meshData, new Vector3(0.5f, 0.5f, 0), out _jobHandle);
            PPSVertWithUV = VectorUtility.ConvertVerticesToUV(PPSVert, ref _jobHandle);

        }

        public NativeArray<Vector2> OutPutUV() => GetUVVert;

        public void Dispose()
        {
            MeshData = null;
            _jobHandle.Complete();
            PPSVert.Dispose();
            PPSVertWithUV.Dispose();
            _jobHandle = default;
            PPSVert = default;
            PPSVertWithUV = default;
        }
    }
}

