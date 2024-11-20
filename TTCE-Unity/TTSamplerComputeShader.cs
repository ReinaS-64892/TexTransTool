using UnityEngine;
using System;
using System.Collections.Generic;
using net.rs64.TexTransCore;

namespace net.rs64.TexTransCoreEngineForUnity
{
    public class TTSamplerComputeShader : TTComputeUnityObject, ITTSamplerKey
    {
        public override TTComputeType ComputeType => TTComputeType.Sampler;
        public ComputeShader ResizingCompute;

        public ComputeKeyHolder GetResizingComputeKey => new(ResizingCompute);
    }
    public class ComputeKeyHolder : ITTComputeKey
    {
        public ComputeShader ComputeShader;
        public ComputeKeyHolder(ComputeShader computeShader)
        {
            ComputeShader = computeShader;
        }
    }
}
