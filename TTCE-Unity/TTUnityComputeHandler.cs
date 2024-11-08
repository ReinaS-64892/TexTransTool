using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using net.rs64.TexTransCore;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace net.rs64.TexTransCoreEngineForUnity
{
    internal class TTUnityComputeHandler : ITTComputeHandler
    {
        ComputeShader _compute;
        Dictionary<int, GraphicsBuffer> _constantsBuffers;
        public TTUnityComputeHandler(ComputeShader compute)
        {
            _compute = compute;
            _constantsBuffers = new();
        }
        public (uint x, uint y, uint z) WorkGroupSize
        {
            get
            {
                _compute.GetKernelThreadGroupSizes(0, out var x, out var y, out var z);
                return (x, y, z);
            }
        }

        public void Dispatch(uint x, uint y, uint z)
        {
            _compute.Dispatch(0, (int)x, (int)y, (int)z);
        }


        public int NameToID(string name) { return Shader.PropertyToID(name); }

        public void SetTexture(int id, ITTRenderTexture tex)
        {
            _compute.SetTexture(0, id, tex.Unwrap());
        }

        public void UploadCBuffer<T>(int id, ReadOnlySpan<T> bytes) where T : unmanaged
        {
            if (_constantsBuffers.ContainsKey(id) is false)
            { _constantsBuffers[id] = new GraphicsBuffer(GraphicsBuffer.Target.Constant, 1, bytes.Length * UnsafeUtility.SizeOf<T>()); }

            using var na = new NativeArray<byte>(_constantsBuffers[id].stride, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

            MemoryMarshal.Cast<T, byte>(bytes).CopyTo(na.AsSpan());
            _constantsBuffers[id].SetData(na);

            _compute.SetConstantBuffer(id, _constantsBuffers[id], 0, _constantsBuffers[id].stride);
        }

        public void Dispose()
        {
            foreach (var buf in _constantsBuffers.Values) { buf.Dispose(); }
            _constantsBuffers.Clear();
        }
    }
}
