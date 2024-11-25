#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace net.rs64.TexTransCore.TransTexture
{
    public static class TransMappingUtility
    {
        // Vector4 の Span を愚直に作ると事故るので気を付けて...
        public static void WriteMapping<TTCE>(this TTCE engine, TTTransMappingHolder transMappingHolder, Span<Vector4> transSourcePolygons)
        where TTCE : ITexTransCreateTexture
        , ITexTransComputeKeyQuery
        , ITexTransGetComputeHandler
        {
            using var computeHandler = engine.GetComputeHandler(engine.StandardComputeKey.TransMapping);

            var gvBufId = computeHandler.NameToID("gv");
            var transMapID = computeHandler.NameToID("TransMap");
            var distanceAndScalingID = computeHandler.NameToID("DistanceAndScaling");
            var polygonBufferID = computeHandler.NameToID("Polygons");

            Span<byte> gvBuf = stackalloc byte[32];
            BitConverter.TryWriteBytes(gvBuf.Slice(0, 4), transMappingHolder.DistanceAndScaleMap.Width);
            BitConverter.TryWriteBytes(gvBuf.Slice(4, 4), transMappingHolder.DistanceAndScaleMap.Hight);
            BitConverter.TryWriteBytes(gvBuf.Slice(8, 4), transMappingHolder.SourceSize.x);
            BitConverter.TryWriteBytes(gvBuf.Slice(12, 4), transMappingHolder.SourceSize.y);
            BitConverter.TryWriteBytes(gvBuf.Slice(16, 4), 0);
            gvBuf[20..].Fill(0);
            computeHandler.UploadConstantsBuffer<byte>(gvBufId, gvBuf);

            computeHandler.SetTexture(transMapID, transMappingHolder.TransMap);
            computeHandler.SetTexture(distanceAndScalingID, transMappingHolder.DistanceAndScaleMap);

            var polygonCount = (uint)(transSourcePolygons.Length / 3);
            computeHandler.UploadStorageBuffer<Vector4>(polygonBufferID, transSourcePolygons);
            computeHandler.Dispatch(polygonCount, 1, 1);// MaxDistance を 0 にして三角形の内側を絶対に塗る。

            BitConverter.TryWriteBytes(gvBuf.Slice(16, 4), transMappingHolder.MaxDistance);
            computeHandler.UploadConstantsBuffer<byte>(gvBufId, gvBuf);
            computeHandler.Dispatch(polygonCount, 1, 2); // 通常のパディング生成、z が 2 なのは並列による競合の緩和のため、完ぺきな解決手段があるなら欲しいものだ。
        }
        public static void TransWrite<TTCE>(this TTCE engine, TTTransMappingHolder transMappingHolder, TTRenderTexWithDistance dist, ITTRenderTexture source, ITTSamplerKey samplerKey)
        where TTCE : ITexTransCreateTexture
        , ITexTransComputeKeyQuery
        , ITexTransGetComputeHandler
        {
            if (dist.Texture.EqualSize(transMappingHolder.DistanceAndScaleMap) is false || source.Width != transMappingHolder.SourceSize.x || source.Hight != transMappingHolder.SourceSize.y) { throw new ArgumentException(); }

            using var sampleCompute = engine.GetComputeHandler(engine.TransSamplerKey[samplerKey]);

            var readTexID = sampleCompute.NameToID("ReadTex");
            var readTextureParmBufId = sampleCompute.NameToID("ReadTextureParm");

            var transMapID = sampleCompute.NameToID("TransMap");
            var distanceAndScalingID = sampleCompute.NameToID("DistanceAndScaling");

            var targetTexID = sampleCompute.NameToID("TargetTex");
            var targetDistanceMapID = sampleCompute.NameToID("TargetDistanceMap");


            Span<uint> readTextureBuf = stackalloc uint[4];
            readTextureBuf[0] = (uint)source.Width;
            readTextureBuf[1] = (uint)source.Hight;
            sampleCompute.UploadConstantsBuffer<uint>(readTextureParmBufId, readTextureBuf);
            sampleCompute.SetTexture(readTexID, source);

            sampleCompute.SetTexture(transMapID, transMappingHolder.TransMap);
            sampleCompute.SetTexture(distanceAndScalingID, transMappingHolder.DistanceAndScaleMap);

            sampleCompute.SetTexture(targetTexID, dist.Texture);
            sampleCompute.SetTexture(targetDistanceMapID, dist.DistanceMap);

            sampleCompute.DispatchWithTextureSize(dist.Texture);
        }
    }

    public class TTTransMappingHolder : IDisposable
    {
        public ITTRenderTexture DistanceAndScaleMap, TransMap;
        public readonly (int x, int y) TargetSize, SourceSize;
        public readonly float MaxDistance;
        private TTTransMappingHolder(ITTRenderTexture distanceAndScaleMap, ITTRenderTexture transMap, (int x, int y) targetsize, (int x, int y) sourceSize, float maxDistance)
        {
            DistanceAndScaleMap = distanceAndScaleMap;
            TransMap = transMap;
            MaxDistance = maxDistance;
            TargetSize = targetsize;
            SourceSize = sourceSize;
        }
        public static TTTransMappingHolder Create<TTCE>(TTCE engine, (int x, int y) targetsize, (int x, int y) sourceSize, float maxDistance)
        where TTCE : ITexTransCreateTexture
        , ITexTransComputeKeyQuery
        , ITexTransGetComputeHandler
        {
            var distanceAndScaleMap = engine.CreateRenderTexture(targetsize.x, targetsize.y, TexTransCoreTextureChannel.RG);
            var transMap = engine.CreateRenderTexture(targetsize.x, targetsize.y, TexTransCoreTextureChannel.RG);
            engine.FillRG(distanceAndScaleMap, new(maxDistance, 0));
            return new(distanceAndScaleMap, transMap, targetsize, sourceSize, maxDistance);
        }
        public void Dispose()
        {
            DistanceAndScaleMap?.Dispose();
            DistanceAndScaleMap = null!;
            TransMap?.Dispose();
            TransMap = null!;
        }
    }

    public class TTRenderTexWithDistance : IDisposable
    {
        public ITTRenderTexture Texture;
        public ITTRenderTexture DistanceMap;
        public readonly (int x, int y) Size;
        public readonly float MaxDistance;
        private TTRenderTexWithDistance(ITTRenderTexture texture, ITTRenderTexture distanceMap, (int x, int y) size, float maxDistance)
        {
            Texture = texture;
            Size = size;
            DistanceMap = distanceMap;
            MaxDistance = maxDistance;
        }
        public static TTRenderTexWithDistance Create<TTCE>(TTCE engine, (int x, int y) size, float maxDistance)
        where TTCE : ITexTransCreateTexture
        , ITexTransComputeKeyQuery
        , ITexTransGetComputeHandler
        {
            var texture = engine.CreateRenderTexture(size.x, size.y);
            var distanceMap = engine.CreateRenderTexture(size.x, size.y, TexTransCoreTextureChannel.R);
            engine.FillR(distanceMap, maxDistance);
            return new(texture, distanceMap, size, maxDistance);
        }
        public void Dispose()
        {
            Texture?.Dispose();
            Texture = null!;
            DistanceMap?.Dispose();
            DistanceMap = null!;
        }
    }


}
