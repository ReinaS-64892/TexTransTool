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
        internal Dictionary<int, (GraphicsBuffer buf, bool downloadable)> _storageBuffers;
        public TTUnityComputeHandler(ComputeShader compute)
        {
            _compute = compute;
            _constantsBuffers = new();
            _storageBuffers = new();
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
        public void MoveBuffer(int id, ITTStorageBufferHolder bufferHolder)
        {
            var unitySBH = (TTUnityStorageBufferHolder)bufferHolder;
            if (unitySBH._buffer is null) { throw new NullReferenceException(); }

            if (_storageBuffers.Remove(id, out var bBuf)) { bBuf.buf.Dispose(); }

            _storageBuffers[id] = (unitySBH._buffer, unitySBH._downloadable);
            _compute.SetBuffer(0, id, _storageBuffers[id].buf);
            unitySBH._buffer = null;
        }
        public ITTStorageBufferHolder? TakeBuffer(int id)
        {
            if (_storageBuffers.Remove(id, out var buffer)) { return new TTUnityStorageBufferHolder(buffer.buf, buffer.downloadable); }
            return null;
        }



        public void Dispose()
        {
            foreach (var buf in _constantsBuffers.Values) { buf.Dispose(); }
            _constantsBuffers.Clear();
            foreach (var buf in _storageBuffers.Values) { buf.buf.Dispose(); }
            _storageBuffers.Clear();
        }

#nullable enable
        internal class TTUnityStorageBufferHolder : ITTStorageBufferHolder
        {
            internal GraphicsBuffer? _buffer;
            public bool Owned => _buffer is not null;
            public string Name { get; set; } = "TTUnityStorageBufferHolder";
            internal bool _downloadable;

            public TTUnityStorageBufferHolder(int length, bool downloadable)
            {
                _buffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, TTMath.NormalizeOf4Multiple(length) / 4, 4);
                _downloadable = downloadable;
            }
            public TTUnityStorageBufferHolder(GraphicsBuffer buffer, bool downloadable)
            {
                _buffer = buffer;
                _downloadable = downloadable;
            }
            public TTUnityStorageBufferHolder(bool downloadable)
            {
                _buffer = null;
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
