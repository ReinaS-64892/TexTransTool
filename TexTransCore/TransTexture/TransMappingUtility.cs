#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;

namespace net.rs64.TexTransCore.TransTexture
{
    public static class TransMappingUtility
    {
        // Vector4 の Span を愚直に作ると事故るので気を付けて...
        public static void WriteMapping<TTCE>(this TTCE engine, TTTransMappingHolder transMappingHolder, Span<Vector2> transToPolygons, Span<TTVector4> transFromPolygons)
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
            computeHandler.UploadStorageBuffer<TTVector4>(fromPolygonsID, transFromPolygons);
            computeHandler.UploadStorageBuffer<Vector2>(toPolygonID, transToPolygons);

            computeHandler.Dispatch(polygonCount, 1, 1);// MaxDistance を 0 にして三角形の内側を絶対に塗る。

            if (transMappingHolder.MaxDistance <= 0.0001) { return; }

            BitConverter.TryWriteBytes(gvBuf.Slice(16, 4), transMappingHolder.MaxDistance);
            computeHandler.UploadConstantsBuffer<byte>(gvBufId, gvBuf);
            computeHandler.Dispatch(polygonCount, 1, 2); // 通常のパディング生成、z が 2 なのは並列による競合の緩和のため、完ぺきな解決手段があるなら欲しいものだ。
        }
        public static void WriteMappingHighQuality<TTCE>(this TTCE engine, TTTransMappingHolder transMappingHolder, Span<Vector2> transToPolygons, Span<TTVector4> transFromPolygons)
        where TTCE : ITexTransComputeKeyQuery, ITexTransGetComputeHandler
        {
            if (transMappingHolder.MaxDistance <= 0.0001) { engine.WriteMapping(transMappingHolder, transToPolygons, transFromPolygons); return; }
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
            BitConverter.TryWriteBytes(gvBuf.Slice(16, 4), transMappingHolder.MaxDistance);
            gvBuf[20..].Fill(0);
            computeHandler.UploadConstantsBuffer<byte>(gvBufId, gvBuf);

            computeHandler.SetTexture(transMapID, transMappingHolder.TransMap);
            computeHandler.SetTexture(distanceMapID, transMappingHolder.DistanceMap);
            computeHandler.SetTexture(scalingMapID, transMappingHolder.ScalingMap);
            computeHandler.SetTexture(additionalDataMapID, transMappingHolder.AdditionalDataMap);

            var polygonCount = transFromPolygons.Length / 3;

            var bufferF = new TTVector4[transFromPolygons.Length];
            var bufferT = new Vector2[transToPolygons.Length];

            var need = new BitArray(polygonCount, true);
            var boxBuffer = new TTVector4[polygonCount];


            var boxCache = new TTVector4[polygonCount];
            var xP = 1f / transMappingHolder.TargetSize.x * transMappingHolder.MaxDistance;
            var yP = 1f / transMappingHolder.TargetSize.y * transMappingHolder.MaxDistance;
            for (var i = 0; need.Length > i; i += 1)
            {
                var rI = i * 3;
                var tp = transToPolygons.Slice(rI, 3);

                TTVector4 box = new TTVector4(tp[0].X, tp[0].Y, tp[0].X, tp[0].Y);
                for (var t = 1; tp.Length > t; t += 1)
                {
                    box.X = Math.Min(box.X, tp[t].X);
                    box.Y = Math.Min(box.Y, tp[t].Y);
                    box.Z = Math.Max(box.Z, tp[t].X);
                    box.W = Math.Max(box.W, tp[t].Y);
                }
                box.X -= xP;
                box.Y -= yP;
                box.Z += xP;
                box.W += yP;
                boxCache[i] = box;
            }


            while (true)
            {
                var dispatchPolygonCount = 0;
                for (var i = 0; need.Length > i; i += 1)
                {
                    if (need[i] is false) { continue; }

                    var rI = i * 3;
                    var tp = transToPolygons.Slice(rI, 3);

                    var box = boxCache[i];
                    var boxHash = box.GetHashCode();
                    var polyHash = HashCode.Combine(tp[0], tp[1], tp[2]);
                    var conflict = false;
                    for (var bi = 0; dispatchPolygonCount > bi; bi += 1)
                    {
                        conflict = BoxIntersect(boxBuffer[bi], box);
                        if (conflict)
                        {
                            var bPoly = bufferT.AsSpan(bi * 3, 3);
                            if (
                                boxBuffer[bi].GetHashCode() == boxHash && boxBuffer[bi].Equals(box)
                                 && polyHash == HashCode.Combine(bPoly[0], bPoly[1], bPoly[2])
                                 && tp[0] == bPoly[0] && tp[1] == bPoly[1] && tp[2] == bPoly[2]
                            )
                            {
                                need[i] = false;//完全一致しているポリゴンは見なかったことにします！
                            }
                            break;
                        }
                    }

                    if (conflict) { continue; }

                    var wI = dispatchPolygonCount * 3;

                    var bf = bufferF.AsSpan(wI, 3);
                    var bt = bufferT.AsSpan(wI, 3);

                    var fp = transFromPolygons.Slice(rI, 3);

                    fp.CopyTo(bf);
                    tp.CopyTo(bt);

                    boxBuffer[dispatchPolygonCount] = box;
                    need[i] = false;
                    dispatchPolygonCount += 1;
                }
                if (dispatchPolygonCount == 0) { break; }

                computeHandler.UploadStorageBuffer<TTVector4>(fromPolygonsID, bufferF.AsSpan(0, dispatchPolygonCount * 3));
                computeHandler.UploadStorageBuffer<Vector2>(toPolygonID, bufferT.AsSpan(0, dispatchPolygonCount * 3));

                computeHandler.Dispatch((uint)dispatchPolygonCount, 1, 1);
            }
        }
        static bool BoxIntersect(TTVector4 a, TTVector4 b)
        {
            return (a.X <= b.Z && a.Z >= b.X) && (a.Y <= b.W && a.W >= b.Y);
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
