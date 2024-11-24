#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace net.rs64.TexTransCore.TransTexture
{
    public class TransTextureContext<TTCE> : IDisposable
    where TTCE : ITexTransCreateTexture
    , ITexTransComputeKeyQuery
    , ITexTransGetComputeHandler
    {
        TTCE _engine;

        ITTComputeHandler _computeHandler;
        ITTRenderTexture _distanceAndScaleMap, _transMap;
        (int x, int y) _sourceSize;

        public float Padding { get; private set; }


        public TransTextureContext(TTCE engine, (int x, int y) targetSize, (int x, int y) sourceSize, float padding)
        {
            _engine = engine;
            _distanceAndScaleMap = _engine.CreateRenderTexture(targetSize.x, targetSize.y, TexTransCoreTextureChannel.RG);
            _transMap = _engine.CreateRenderTexture(targetSize.x, targetSize.y, TexTransCoreTextureChannel.RG);
            _sourceSize = sourceSize;
            Padding = padding;

            _computeHandler = engine.GetComputeHandler(engine.StandardComputeKey.TransMapping);

            _engine.FillRG(_distanceAndScaleMap, new(Padding, 0));

            _gvBufId = _computeHandler.NameToID("gv");

            _transMapID = _computeHandler.NameToID("TransMap");
            _distanceAndScalingID = _computeHandler.NameToID("DistanceAndScaling");

            _polygonBufferID = _computeHandler.NameToID("Polygons");

            Span<byte> gvBuf = stackalloc byte[32];
            BitConverter.TryWriteBytes(gvBuf.Slice(0, 4), _distanceAndScaleMap.Width);
            BitConverter.TryWriteBytes(gvBuf.Slice(4, 4), _distanceAndScaleMap.Hight);
            BitConverter.TryWriteBytes(gvBuf.Slice(8, 4), _sourceSize.x);
            BitConverter.TryWriteBytes(gvBuf.Slice(12, 4), _sourceSize.y);
            BitConverter.TryWriteBytes(gvBuf.Slice(16, 4), Padding);
            gvBuf[20..].Fill(0);
            _computeHandler.UploadConstantsBuffer<byte>(_gvBufId, gvBuf);

            _computeHandler.SetTexture(_transMapID, _transMap);
            _computeHandler.SetTexture(_distanceAndScalingID, _distanceAndScaleMap);
        }
        private int _gvBufId, _transMapID, _distanceAndScalingID, _polygonBufferID;

        public void WriteMapping(Span<Vector4> transSourcePolygons)
        {
            _computeHandler.UploadStorageBuffer<Vector4>(_polygonBufferID, transSourcePolygons);
            _computeHandler.Dispatch((uint)(transSourcePolygons.Length / 3), 1, 1);
        }

        public void TransWrite(TTRenderTexWithDistance dist, ITTRenderTexture source, ITTSamplerKey samplerKey)
        {
            if (dist.Texture.EqualSize(_distanceAndScaleMap) is false || source.Width != _sourceSize.x || source.Hight != _sourceSize.y) { throw new ArgumentException(); }

            using var sampleCompute = _engine.GetComputeHandler(_engine.TransSamplerKey[samplerKey]);

            var readTexID = sampleCompute.NameToID("ReadTex");
            var readTextureParmBufId = sampleCompute.NameToID("ReadTextureParm");


            var transMapID = sampleCompute.NameToID("TransMap");
            var distanceAndScalingID = sampleCompute.NameToID("DistanceAndScaling");

            var targetTexID = sampleCompute.NameToID("TargetTex");
            var targetDistanceMapID = sampleCompute.NameToID("TargetDistanceMap");


            sampleCompute.SetTexture(readTexID, source);
            Span<uint> readTextureBuf = stackalloc uint[4];
            readTextureBuf[0] = (uint)source.Width;
            readTextureBuf[1] = (uint)source.Hight;
            sampleCompute.UploadConstantsBuffer<uint>(readTextureParmBufId, readTextureBuf);

            sampleCompute.SetTexture(transMapID, _transMap);
            sampleCompute.SetTexture(distanceAndScalingID, _distanceAndScaleMap);

            sampleCompute.SetTexture(targetTexID, dist.Texture);
            sampleCompute.SetTexture(targetDistanceMapID, dist.DistanceMap);


            sampleCompute.DispatchWithTextureSize(dist.Texture);
        }


        public void Dispose()
        {
            _computeHandler?.Dispose();
            _computeHandler = null!;
            _distanceAndScaleMap?.Dispose();
            _distanceAndScaleMap = null!;
            _transMap?.Dispose();
            _transMap = null!;
        }
    }

    public class TTRenderTexWithDistance : IDisposable
    {
        public ITTRenderTexture Texture;
        public ITTRenderTexture DistanceMap;
        public TTRenderTexWithDistance(ITTRenderTexture tex, ITexTransCreateTexture ttce)
        {
            Texture = tex;
            DistanceMap = ttce.CreateRenderTexture(Texture.Width, Texture.Hight, TexTransCoreTextureChannel.R);
        }
        public TTRenderTexWithDistance(ITTRenderTexture tex, ITTRenderTexture distance)
        {
            Texture = tex;
            DistanceMap = distance;
            if (Texture.EqualSize(DistanceMap) is false) { throw new ArgumentException(); }
        }
        public void InitializeDistanceMap<TTCE>(TTCE ttce, float maxDistance)
        where TTCE : ITexTransComputeKeyQuery, ITexTransGetComputeHandler
        { ttce.FillR(DistanceMap, maxDistance); }
        public void Dispose()
        {
            DistanceMap?.Dispose();
            DistanceMap = null!;
        }
    }


}
