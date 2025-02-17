#nullable enable
using UnityEngine;
using System;
using System.Collections.Generic;
using net.rs64.TexTransCore;

namespace net.rs64.TexTransCoreEngineForUnity
{
    public class TTSamplerComputeShader : TTComputeUnityObject, ITTSamplerKey
    {
        public override TTComputeType ComputeType => TTComputeType.Sampler;
        public ComputeShader ResizingCompute = null!;
        public ComputeShader TransSamplerCompute = null!;

        private ComputeKeyHolder? _resizingComputeKey;
        public ComputeKeyHolder GetResizingComputeKey => _resizingComputeKey ??= new(ResizingCompute);

        private ComputeKeyHolder? _transSamplerComputeKey;
        public ComputeKeyHolder GetTransSamplerComputeKey => _transSamplerComputeKey ??= new(TransSamplerCompute);
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
