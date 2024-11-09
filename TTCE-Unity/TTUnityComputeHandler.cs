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
        Dictionary<int, GraphicsBuffer> _buffers;
        public TTUnityComputeHandler(ComputeShader compute)
        {
            _compute = compute;
            _buffers = new();
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

        public void UploadConstantsBuffer<T>(int id, ReadOnlySpan<T> bytes) where T : unmanaged
        {
            UploadBufferImpl(id, bytes, true);
        }
        public void UploadStorageBuffer<T>(int id, ReadOnlySpan<T> bytes) where T : unmanaged
        {
            UploadBufferImpl(id, bytes, false);
        }

        private void UploadBufferImpl<T>(int id, ReadOnlySpan<T> bytes, bool isConstants) where T : unmanaged
        {
            if (_buffers.ContainsKey(id) is false)
            { _buffers[id] = new GraphicsBuffer(isConstants ? GraphicsBuffer.Target.Constant : GraphicsBuffer.Target.Structured, 1, bytes.Length * UnsafeUtility.SizeOf<T>()); }
            using var na = new NativeArray<byte>(_buffers[id].stride, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

            MemoryMarshal.Cast<T, byte>(bytes).CopyTo(na.AsSpan());
            _buffers[id].SetData(na);

            if (isConstants) _compute.SetConstantBuffer(id, _buffers[id], 0, _buffers[id].stride);
            else _compute.SetBuffer(0, id, _buffers[id]);
        }


        public void Dispose()
        {
            foreach (var buf in _buffers.Values) { buf.Dispose(); }
            _buffers.Clear();
        }

    }
}
