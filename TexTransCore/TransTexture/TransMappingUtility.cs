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
        public static void WriteMapping<TTCE>(this TTCE engine, TTTransMappingHolder transMappingHolder, Span<Vector2> transToPolygons, Span<Vector4> transFromPolygons)
        where TTCE : ITexTransComputeKeyQuery, ITexTransGetComputeHandler
        {
            using var computeHandler = engine.GetComputeHandler(engine.TransTextureComputeKey.TransMapping);

            var gvBufId = computeHandler.NameToID("gv");

            var transMapID = computeHandler.NameToID("TransMap");
            var distanceMapID = computeHandler.NameToID("DistanceMap");
            var scalingMapID = computeHandler.NameToID("ScalingMap");
            var additionalDataMapID = computeHandler.NameToID("AdditionalDataMap");

            var fromPolygonsID = computeHandler.NameToID("FromPolygons");
            var toPolygonID = computeHandler.NameToID("ToPolygons");

            Span<byte> gvBuf = stackalloc byte[32];
            BitConverter.TryWriteBytes(gvBuf.Slice(0, 4), transMappingHolder.TargetSize.x);
            BitConverter.TryWriteBytes(gvBuf.Slice(4, 4), transMappingHolder.TargetSize.y);
            BitConverter.TryWriteBytes(gvBuf.Slice(8, 4), transMappingHolder.SourceSize.x);
            BitConverter.TryWriteBytes(gvBuf.Slice(12, 4), transMappingHolder.SourceSize.y);
            BitConverter.TryWriteBytes(gvBuf.Slice(16, 4), 0);
            gvBuf[20..].Fill(0);
            computeHandler.UploadConstantsBuffer<byte>(gvBufId, gvBuf);

            computeHandler.SetTexture(transMapID, transMappingHolder.TransMap);
            computeHandler.SetTexture(distanceMapID, transMappingHolder.DistanceMap);
            computeHandler.SetTexture(scalingMapID, transMappingHolder.ScalingMap);
            computeHandler.SetTexture(additionalDataMapID, transMappingHolder.AdditionalDataMap);

            var polygonCount = (uint)(transFromPolygons.Length / 3);
            computeHandler.UploadStorageBuffer<Vector4>(fromPolygonsID, transFromPolygons);
            computeHandler.UploadStorageBuffer<Vector2>(toPolygonID, transToPolygons);

            computeHandler.Dispatch(polygonCount, 1, 1);// MaxDistance を 0 にして三角形の内側を絶対に塗る。

            BitConverter.TryWriteBytes(gvBuf.Slice(16, 4), transMappingHolder.MaxDistance);
            computeHandler.UploadConstantsBuffer<byte>(gvBufId, gvBuf);
            computeHandler.Dispatch(polygonCount, 1, 2); // 通常のパディング生成、z が 2 なのは並列による競合の緩和のため、完ぺきな解決手段があるなら欲しいものだ。
        }
        public static void WriteMappingHighQuality<TTCE>(this TTCE engine, TTTransMappingHolder transMappingHolder, Span<Vector2> transToPolygons, Span<Vector4> transFromPolygons)
        where TTCE : ITexTransComputeKeyQuery, ITexTransGetComputeHandler
        {
            using var computeHandler = engine.GetComputeHandler(engine.TransTextureComputeKey.TransMappingHighQuality);

            var gvBufId = computeHandler.NameToID("gv");

            var transMapID = computeHandler.NameToID("TransMap");
            var distanceMapID = computeHandler.NameToID("DistanceMap");
            var scalingMapID = computeHandler.NameToID("ScalingMap");
            var additionalDataMapID = computeHandler.NameToID("AdditionalDataMap");

            var fromPolygonsID = computeHandler.NameToID("FromPolygons");
            var toPolygonID = computeHandler.NameToID("ToPolygons");

            Span<byte> gvBuf = stackalloc byte[32];
            BitConverter.TryWriteBytes(gvBuf.Slice(0, 4), transMappingHolder.TargetSize.x);
            BitConverter.TryWriteBytes(gvBuf.Slice(4, 4), transMappingHolder.TargetSize.y);
            BitConverter.TryWriteBytes(gvBuf.Slice(8, 4), transMappingHolder.SourceSize.x);
            BitConverter.TryWriteBytes(gvBuf.Slice(12, 4), transMappingHolder.SourceSize.y);
            BitConverter.TryWriteBytes(gvBuf.Slice(16, 4), transMappingHolder.MaxDistance);
            gvBuf[20..].Fill(0);
            computeHandler.UploadConstantsBuffer<byte>(gvBufId, gvBuf);

            computeHandler.SetTexture(transMapID, transMappingHolder.TransMap);
            computeHandler.SetTexture(distanceMapID, transMappingHolder.DistanceMap);
            computeHandler.SetTexture(scalingMapID, transMappingHolder.ScalingMap);
            computeHandler.SetTexture(additionalDataMapID, transMappingHolder.AdditionalDataMap);

            computeHandler.UploadStorageBuffer<Vector4>(fromPolygonsID, transFromPolygons);
            computeHandler.UploadStorageBuffer<Vector2>(toPolygonID, transToPolygons);

            var polygonCount = (uint)(transFromPolygons.Length / 3);
            var (dX, dY, _) = computeHandler.WorkGroupSize;
            computeHandler.Dispatch((uint)((transMappingHolder.TargetSize.x + (dX - 1)) / dX), (uint)((transMappingHolder.TargetSize.y + (dY - 1)) / dY), polygonCount);
        }
        public static void TransWrite<TTCE>(this TTCE engine, TTTransMappingHolder transMappingHolder, TTRenderTexWithDistance dist, ITTRenderTexture source, ITTSamplerKey samplerKey)
        where TTCE : ITexTransComputeKeyQuery
        , ITexTransGetComputeHandler
        {
            if (dist.Texture.EqualSize(transMappingHolder.TransMap) is false || source.Width != transMappingHolder.SourceSize.x || source.Hight != transMappingHolder.SourceSize.y) { throw new ArgumentException(); }

            using var sampleCompute = engine.GetComputeHandler(engine.TransSamplerKey[samplerKey]);

            var readTexID = sampleCompute.NameToID("ReadTex");
            var readTextureParmBufId = sampleCompute.NameToID("ReadTextureParm");

            var transMapID = sampleCompute.NameToID("TransMap");
            var distanceMapID = sampleCompute.NameToID("DistanceMap");
            var scalingMapID = sampleCompute.NameToID("ScalingMap");

            var targetTexID = sampleCompute.NameToID("TargetTex");
            var targetDistanceMapID = sampleCompute.NameToID("TargetDistanceMap");


            Span<uint> readTextureBuf = stackalloc uint[4];
            readTextureBuf[0] = (uint)source.Width;
            readTextureBuf[1] = (uint)source.Hight;
            sampleCompute.UploadConstantsBuffer<uint>(readTextureParmBufId, readTextureBuf);
            sampleCompute.SetTexture(readTexID, source);

            sampleCompute.SetTexture(transMapID, transMappingHolder.TransMap);
            sampleCompute.SetTexture(distanceMapID, transMappingHolder.DistanceMap);
            sampleCompute.SetTexture(scalingMapID, transMappingHolder.ScalingMap);

            sampleCompute.SetTexture(targetTexID, dist.Texture);
            sampleCompute.SetTexture(targetDistanceMapID, dist.DistanceMap);

            sampleCompute.DispatchWithTextureSize(dist.Texture);
        }

        public static void TransWarpModifierWithNone<TTCE>(this TTCE engine, TTTransMappingHolder transMappingHolder)
        where TTCE : ITexTransComputeKeyQuery, ITexTransGetComputeHandler
        {
            using var computeHandler = engine.GetComputeHandler(engine.TransTextureComputeKey.TransWarpNone);

            var gvBufId = computeHandler.NameToID("gv");
            var transMapID = computeHandler.NameToID("TransMap");
            var distanceMapID = computeHandler.NameToID("DistanceMap");

            Span<float> buf = stackalloc float[4];
            buf[0] = transMappingHolder.MaxDistance;
            computeHandler.UploadConstantsBuffer<float>(gvBufId, buf);

            computeHandler.SetTexture(transMapID, transMappingHolder.TransMap);
            computeHandler.SetTexture(distanceMapID, transMappingHolder.DistanceMap);

            computeHandler.DispatchWithTextureSize(transMappingHolder.TransMap);
        }
        public static void TransWarpModifierWithStretch<TTCE>(this TTCE engine, TTTransMappingHolder transMappingHolder)
        where TTCE : ITexTransComputeKeyQuery, ITexTransGetComputeHandler
        {
            using var computeHandler = engine.GetComputeHandler(engine.TransTextureComputeKey.TransWarpNone);

            var transMapID = computeHandler.NameToID("TransMap");
            computeHandler.SetTexture(transMapID, transMappingHolder.TransMap);

            computeHandler.DispatchWithTextureSize(transMappingHolder.TransMap);
        }
    }

    public class TTTransMappingHolder : IDisposable
    {
        public ITTRenderTexture TransMap;
        public ITTRenderTexture DistanceMap, ScalingMap;
        public ITTRenderTexture AdditionalDataMap;
        public readonly (int x, int y) TargetSize, SourceSize;
        public readonly float MaxDistance;
        private TTTransMappingHolder(ITTRenderTexture transMap, ITTRenderTexture distanceMap, ITTRenderTexture scalingMap, ITTRenderTexture additionalDataMap, (int x, int y) targetsize, (int x, int y) sourceSize, float maxDistance)
        {
            TransMap = transMap;
            DistanceMap = distanceMap;
            ScalingMap = scalingMap;
            AdditionalDataMap = additionalDataMap;
            TargetSize = targetsize;
            SourceSize = sourceSize;
            MaxDistance = maxDistance;
        }
        public static TTTransMappingHolder Create<TTCE>(TTCE engine, (int x, int y) targetsize, (int x, int y) sourceSize, float maxDistance)
        where TTCE : ITexTransCreateTexture
        , ITexTransComputeKeyQuery
        , ITexTransGetComputeHandler
        {
            var transMap = engine.CreateRenderTexture(targetsize.x, targetsize.y, TexTransCoreTextureChannel.RG);
            var distanceMap = engine.CreateRenderTexture(targetsize.x, targetsize.y, TexTransCoreTextureChannel.R);
            var scalingMap = engine.CreateRenderTexture(targetsize.x, targetsize.y, TexTransCoreTextureChannel.R);
            var additionalDataMap = engine.CreateRenderTexture(targetsize.x, targetsize.y, TexTransCoreTextureChannel.RG);
            engine.FillR(distanceMap, maxDistance);
            return new(transMap, distanceMap, scalingMap, additionalDataMap, targetsize, sourceSize, maxDistance);
        }
        public void Dispose()
        {
            TransMap?.Dispose();
            TransMap = null!;
            DistanceMap?.Dispose();
            DistanceMap = null!;
            ScalingMap?.Dispose();
            ScalingMap = null!;
            AdditionalDataMap?.Dispose();
            AdditionalDataMap = null!;
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
