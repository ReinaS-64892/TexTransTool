#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace net.rs64.TexTransCore
{
    public class TTCEInterfaceDebug : ITexTransCoreEngine, IDisposable
    {
        ITexTransCoreEngine _texTransCoreEngine;
        protected readonly Action<string> _debugCallBack;
        HashSet<Tracer> _tracers = new();

        public TTCEInterfaceDebug(ITexTransCoreEngine texTransCoreEngine, Action<string> debugCallBack)
        {
            _texTransCoreEngine = texTransCoreEngine;
            _debugCallBack = debugCallBack;
        }

        public ITexTransStandardComputeKey StandardComputeKey => _texTransCoreEngine.StandardComputeKey;

        public TExKeyQ GetExKeyQuery<TExKeyQ>() where TExKeyQ : ITTExtraComputeKeyQuery
        {
            return _texTransCoreEngine.GetExKeyQuery<TExKeyQ>();
        }


        public void CopyRenderTexture(ITTRenderTexture target, ITTRenderTexture source)
        {
            _texTransCoreEngine.CopyRenderTexture(PassingTracer(target), PassingTracer(source));
        }
        public ITTRenderTexture CreateRenderTexture(int width, int height, TexTransCoreTextureChannel channel = TexTransCoreTextureChannel.RGBA)
        {
            return Tracing(_texTransCoreEngine.CreateRenderTexture(width, height, channel));
        }



        public void DownloadBuffer<T>(Span<T> dist, ITTStorageBuffer takeToFrom) where T : unmanaged
        {
            _texTransCoreEngine.DownloadBuffer(dist, PassingTracer(takeToFrom));
        }

        public void DownloadTexture<T>(Span<T> dataDist, TexTransCoreTextureFormat format, ITTRenderTexture renderTexture) where T : unmanaged
        {
            _texTransCoreEngine.DownloadTexture(dataDist, format, PassingTracer(renderTexture));
        }



        public ITTComputeHandler GetComputeHandler(ITTComputeKey computeKey)
        {
            return Tracing(_texTransCoreEngine.GetComputeHandler(computeKey));
        }


        public ITTStorageBuffer AllocateStorageBuffer(int length, bool downloadable = false)
        {
            return Tracing(_texTransCoreEngine.AllocateStorageBuffer(length, downloadable));
        }

        public ITTStorageBuffer UploadStorageBuffer<T>(ReadOnlySpan<T> data, bool downloadable = false) where T : unmanaged
        {
            return Tracing(_texTransCoreEngine.UploadStorageBuffer(data, downloadable));
        }


        public void LoadTexture(ITTRenderTexture writeTarget, ITTDiskTexture diskTexture)
        {
            _texTransCoreEngine.LoadTexture(PassingTracer(writeTarget), PassingTracer(diskTexture));
        }
        public void UploadTexture<T>(ITTRenderTexture uploadTarget, ReadOnlySpan<T> bytes, TexTransCoreTextureFormat format) where T : unmanaged
        {
            _texTransCoreEngine.UploadTexture(PassingTracer(uploadTarget), bytes, format);
        }

        public void Dispose()
        {
            foreach (var leaked in _tracers)
            {
                _debugCallBack($"leaked !!! {leaked.GetType().Name}: {leaked.CreatePoint}");
            }
            _tracers.Clear();
        }
        class Tracer : IDisposable
        {
            public readonly StackTrace CreatePoint;
            protected TTCEInterfaceDebug _debug;
            public Tracer(TTCEInterfaceDebug debug)
            {
                _debug = debug;
                _debug._tracers.Add(this);
                CreatePoint = new StackTrace();
            }

            public virtual void Dispose()
            {
                if (_debug._tracers.Remove(this) is false) { _debug._debugCallBack("double free:" + CreatePoint.ToString()); }
            }
        }
        protected ITTRenderTexture Tracing(ITTRenderTexture target)
        {
            if (target is RenderTextureTracer) { return target; }
            return new RenderTextureTracer(this, target);
        }
        protected ITTDiskTexture Tracing(ITTDiskTexture target)
        {
            if (target is DiskTextureTracer) { return target; }
            return new DiskTextureTracer(this, target);
        }
        protected ITTStorageBuffer Tracing(ITTStorageBuffer target)
        {
            if (target is StorageBufferTracer) { return target; }
            return new StorageBufferTracer(this, target);
        }
        protected ITTComputeHandler Tracing(ITTComputeHandler target)
        {
            if (target is ComputeHandlerTracer) { return target; }
            return new ComputeHandlerTracer(this, target);
        }
        protected ITT PassingTracer<ITT>(ITT tracedITT)
        {
            switch (tracedITT)
            {
                default: { return tracedITT; }
                case RenderTextureTracer rtTracer: { return (ITT)rtTracer.SourceTexture; }
                case DiskTextureTracer dtTracer: { return (ITT)dtTracer.SourceTexture; }
                case StorageBufferTracer sbTracer: { return (ITT)sbTracer.SourceBuffer; }
                case ComputeHandlerTracer chTracer: { return (ITT)chTracer.SourceComputeHandler; }
            }
        }
        class RenderTextureTracer : Tracer, ITTRenderTexture
        {
            public ITTRenderTexture SourceTexture;
            public RenderTextureTracer(TTCEInterfaceDebug debug, ITTRenderTexture sourceTexture) : base(debug)
            {
                SourceTexture = sourceTexture;
            }
            public ITTRenderTexture Source => SourceTexture;

            public TexTransCoreTextureChannel ContainsChannel => SourceTexture.ContainsChannel;

            public int Width => SourceTexture.Width;

            public int Hight => SourceTexture.Hight;

            public string Name { get => SourceTexture.Name; set => SourceTexture.Name = value; }

            public override void Dispose()
            {
                base.Dispose();
                SourceTexture.Dispose();
            }
        }
        class DiskTextureTracer : Tracer, ITTDiskTexture
        {
            public ITTDiskTexture SourceTexture;
            public DiskTextureTracer(TTCEInterfaceDebug debug, ITTDiskTexture sourceTexture) : base(debug)
            {
                SourceTexture = sourceTexture;
            }

            public int Width => SourceTexture.Width;

            public int Hight => SourceTexture.Hight;

            public string Name { get => SourceTexture.Name; set => SourceTexture.Name = value; }

            public override void Dispose()
            {
                base.Dispose();
                SourceTexture.Dispose();
            }
        }
        class StorageBufferTracer : Tracer, ITTStorageBuffer
        {
            public ITTStorageBuffer SourceBuffer;
            public StorageBufferTracer(TTCEInterfaceDebug debug, ITTStorageBuffer sourceBuffer) : base(debug)
            {
                SourceBuffer = sourceBuffer;
            }

            public string Name { get => SourceBuffer.Name; set => SourceBuffer.Name = value; }
            public override void Dispose()
            {
                base.Dispose();
                SourceBuffer.Dispose();
            }
        }
        class ComputeHandlerTracer : Tracer, ITTComputeHandler
        {
            public ITTComputeHandler SourceComputeHandler;
            public ComputeHandlerTracer(TTCEInterfaceDebug debug, ITTComputeHandler sourceComputeHandler) : base(debug)
            {
                SourceComputeHandler = sourceComputeHandler;
            }

            public (uint x, uint y, uint z) WorkGroupSize => SourceComputeHandler.WorkGroupSize;

            public string Name { get => SourceComputeHandler.Name; set => SourceComputeHandler.Name = value; }

            public void Dispatch(uint x, uint y, uint z)
            {
                SourceComputeHandler.Dispatch(x, y, z);
            }

            public override void Dispose()
            {
                base.Dispose();
                SourceComputeHandler.Dispose();
            }

            public int NameToID(string name)
            {
                return SourceComputeHandler.NameToID(name);
            }

            public void SetStorageBuffer(int id, ITTStorageBuffer bufferHolder)
            {
                SourceComputeHandler.SetStorageBuffer(id, _debug.PassingTracer(bufferHolder));
            }

            public void SetTexture(int id, ITTRenderTexture tex)
            {
                SourceComputeHandler.SetTexture(id, _debug.PassingTracer(tex));
            }

            public void UploadConstantsBuffer<T>(int id, ReadOnlySpan<T> bytes) where T : unmanaged
            {
                SourceComputeHandler.UploadConstantsBuffer(id, bytes);
            }
        }









    }
}
