#nullable enable
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
        internal ComputeShader _compute;
        internal Dictionary<int, GraphicsBuffer> _constantsBuffers;
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

        public string Name { get => _compute.name; set => _compute.name = value; }

        public void Dispatch(uint x, uint y, uint z)
        {
            _compute.Dispatch(0, (int)x, (int)y, (int)z);
        }


        public int NameToID(string name) { return Shader.PropertyToID(name); }

        public void SetTexture(int id, ITTRenderTexture tex)
        {
            if (tex is not UnityRenderTexture urtS) { throw new InvalidOperationException(); }
            _compute.SetTexture(0, id, urtS.RenderTexture);
        }

        public void UploadConstantsBuffer<T>(int id, ReadOnlySpan<T> bytes) where T : unmanaged
        {
            if (_constantsBuffers.ContainsKey(id) is false)
            {
                var length = TTMath.NormalizeOf4Multiple(bytes.Length * UnsafeUtility.SizeOf<T>());
                _constantsBuffers[id] = new GraphicsBuffer(GraphicsBuffer.Target.Constant, 1, length);
            }
            using var na = new NativeArray<byte>(_constantsBuffers[id].stride, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

            MemoryMarshal.Cast<T, byte>(bytes).CopyTo(na.AsSpan());
            _constantsBuffers[id].SetData(na);

            _compute.SetConstantBuffer(id, _constantsBuffers[id], 0, _constantsBuffers[id].stride);
        }
        public void SetStorageBuffer(int id, ITTStorageBuffer bufferHolder)
        {
            var unitySBH = (TTUnityStorageBuffer)bufferHolder;
            if (unitySBH._buffer is null) { throw new NullReferenceException(); }
            _compute.SetBuffer(0, id, unitySBH._buffer);
        }

        public void Dispose()
        {
            foreach (var buf in _constantsBuffers.Values) { buf.Dispose(); }
            _constantsBuffers.Clear();
        }

#nullable enable
        internal class TTUnityStorageBuffer : ITTStorageBuffer
        {
            internal GraphicsBuffer? _buffer;
            public bool Owned => _buffer is not null;
            public string Name { get; set; } = "TTUnityStorageBufferHolder";
            internal bool _downloadable;

            public TTUnityStorageBuffer(int length, bool downloadable)
            {
                _buffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, TTMath.NormalizeOf4Multiple(length) / 4, 4);
                _downloadable = downloadable;
            }
            public void Dispose()
            {
                _buffer?.Dispose();
                _buffer = null;
            }
        }

    }
}
