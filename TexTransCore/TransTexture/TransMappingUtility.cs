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
        public static void WriteMapping<TTCE>(this TTCE engine, TTTransMappingHolder transMappingHolder, ITTStorageBuffer transToVertex, ITTStorageBuffer transFromVertexBuffer, ITTStorageBuffer polygonIndexesBuffer, int polygonCount, ITTStorageBuffer? transFromDepth = null)
        where TTCE : ITexTransComputeKeyQuery, ITexTransGetComputeHandler, ITexTransDriveStorageBufferHolder
        {
            var useDepth = transFromDepth is not null && transMappingHolder.DepthMap is not null;
            using var computeHandler = useDepth ? engine.GetComputeHandler(engine.GetExKeyQuery<ITransTextureComputeKey>().TransMappingWithDepth) : engine.GetComputeHandler(engine.GetExKeyQuery<ITransTextureComputeKey>().TransMapping);

            var gvBufId = computeHandler.NameToID("gv");

            var transMapID = computeHandler.NameToID("TransMap");
            var distanceMapID = computeHandler.NameToID("DistanceMap");
            var scalingMapID = computeHandler.NameToID("ScalingMap");

            var polygonIndexID = computeHandler.NameToID("PolygonIndex");

            var fromVertexID = computeHandler.NameToID("FromVertex");
            var toVertexID = computeHandler.NameToID("ToVertex");

            Span<byte> gvBuf = stackalloc byte[32];
            BitConverter.TryWriteBytes(gvBuf.Slice(0, 4), transMappingHolder.TargetSize.x);
            BitConverter.TryWriteBytes(gvBuf.Slice(4, 4), transMappingHolder.TargetSize.y);
            BitConverter.TryWriteBytes(gvBuf.Slice(8, 4), transMappingHolder.SourceSize.x);
            BitConverter.TryWriteBytes(gvBuf.Slice(12, 4), transMappingHolder.SourceSize.y);
            BitConverter.TryWriteBytes(gvBuf.Slice(16, 4), (float)0);
            BitConverter.TryWriteBytes(gvBuf.Slice(20, 4), (uint)0);
            gvBuf[24..].Fill(0);

            computeHandler.SetTexture(transMapID, transMappingHolder.TransMap);
            computeHandler.SetTexture(distanceMapID, transMappingHolder.DistanceMap);
            computeHandler.SetTexture(scalingMapID, transMappingHolder.ScalingMap);

            computeHandler.SetStorageBuffer(fromVertexID, transFromVertexBuffer);
            computeHandler.SetStorageBuffer(toVertexID, transToVertex);
            computeHandler.SetStorageBuffer(polygonIndexID, polygonIndexesBuffer);

            if (useDepth)
            {
                var depthMapID = computeHandler.NameToID("DepthMap");
                var fromDepthVertexID = computeHandler.NameToID("FromDepthVertex");
                computeHandler.SetTexture(depthMapID, transMappingHolder.DepthMap!);
                computeHandler.SetStorageBuffer(fromDepthVertexID, transFromDepth!);
            }

            // MaxDistance を 0 にして三角形の内側を絶対に塗る。
            foreach (var (dispatchCount, indexOffset) in SliceDispatch((uint)polygonCount, ushort.MaxValue))
            {
                BitConverter.TryWriteBytes(gvBuf.Slice(20, 4), indexOffset);
                computeHandler.UploadConstantsBuffer<byte>(gvBufId, gvBuf);
                computeHandler.Dispatch(dispatchCount, 1, 1);
            }

            if (transMappingHolder.MaxDistance <= 0.0001) { return; }

            BitConverter.TryWriteBytes(gvBuf.Slice(16, 4), transMappingHolder.MaxDistance);

            foreach (var (dispatchCount, indexOffset) in SliceDispatch((uint)polygonCount, ushort.MaxValue))
            {
                BitConverter.TryWriteBytes(gvBuf.Slice(20, 4), indexOffset);
                computeHandler.UploadConstantsBuffer<byte>(gvBufId, gvBuf);
                computeHandler.Dispatch(dispatchCount, 1, 1);
            }
        }

        internal static IEnumerable<(uint dispatchCount, uint offset)> SliceDispatch(uint polygonCount, uint dispatchMax)
        {
            for (uint offset = 0; polygonCount > offset; offset += dispatchMax)
            {
                yield return (Math.Min(polygonCount - offset, dispatchMax), offset);
            }
        }
        public static void WriteMappingHighQuality<TTCE>(this TTCE engine, TTTransMappingHolder transMappingHolder, ReadOnlySpan<Vector2> transToVertex, ITTStorageBuffer transToVertexBuf, ITTStorageBuffer transFromVertexBuf, ReadOnlySpan<int> polygonIndexes, ITTStorageBuffer? transFromDepthBuf = null)
        where TTCE : ITexTransComputeKeyQuery, ITexTransGetComputeHandler, ITexTransDriveStorageBufferHolder
        {
            var polygonCount = polygonIndexes.Length / 3;
            if (transMappingHolder.MaxDistance <= 0.0001)
            {
                using var pBuf = engine.UploadStorageBuffer(polygonIndexes);
                engine.WriteMapping(transMappingHolder, transToVertexBuf, transFromVertexBuf, pBuf, polygonCount, transFromDepthBuf);
                return;
            }
            var useDepth = transFromDepthBuf is not null && transMappingHolder.DepthMap is not null;
            using var computeHandler = useDepth ? engine.GetComputeHandler(engine.GetExKeyQuery<ITransTextureComputeKey>().TransMappingWithDepth) : engine.GetComputeHandler(engine.GetExKeyQuery<ITransTextureComputeKey>().TransMapping);

            var gvBufId = computeHandler.NameToID("gv");

            var transMapID = computeHandler.NameToID("TransMap");
            var distanceMapID = computeHandler.NameToID("DistanceMap");
            var scalingMapID = computeHandler.NameToID("ScalingMap");

            var polygonIndexID = computeHandler.NameToID("PolygonIndex");

            var fromVertexID = computeHandler.NameToID("FromVertex");
            var toVertexID = computeHandler.NameToID("ToVertex");

            Span<byte> gvBuf = stackalloc byte[32];
            BitConverter.TryWriteBytes(gvBuf.Slice(0, 4), transMappingHolder.TargetSize.x);
            BitConverter.TryWriteBytes(gvBuf.Slice(4, 4), transMappingHolder.TargetSize.y);
            BitConverter.TryWriteBytes(gvBuf.Slice(8, 4), transMappingHolder.SourceSize.x);
            BitConverter.TryWriteBytes(gvBuf.Slice(12, 4), transMappingHolder.SourceSize.y);
            BitConverter.TryWriteBytes(gvBuf.Slice(16, 4), transMappingHolder.MaxDistance);
            gvBuf[20..].Fill(0);

            computeHandler.SetTexture(transMapID, transMappingHolder.TransMap);
            computeHandler.SetTexture(distanceMapID, transMappingHolder.DistanceMap);
            computeHandler.SetTexture(scalingMapID, transMappingHolder.ScalingMap);

            computeHandler.SetStorageBuffer(fromVertexID, transFromVertexBuf);
            computeHandler.SetStorageBuffer(toVertexID, transToVertexBuf);

            if (useDepth)
            {
                var depthMapID = computeHandler.NameToID("DepthMap");
                var fromDepthVertexID = computeHandler.NameToID("FromDepthVertex");
                computeHandler.SetTexture(depthMapID, transMappingHolder.DepthMap!);
                computeHandler.SetStorageBuffer(fromDepthVertexID, transFromDepthBuf!);
            }

            var need = new BitArray(polygonCount, true);
            var boxBuffer = new TTVector4[polygonCount];
            var bufferI = new int[polygonIndexes.Length];


            var boxCache = new TTVector4[polygonCount];
            var xP = 1f / transMappingHolder.TargetSize.x * transMappingHolder.MaxDistance;
            var yP = 1f / transMappingHolder.TargetSize.y * transMappingHolder.MaxDistance;
            for (var i = 0; need.Length > i; i += 1)
            {
                var pIndex = i * 3;
                var tp = polygonIndexes.Slice(pIndex, 3);

                TTVector4 box = new TTVector4(transToVertex[tp[0]].X, transToVertex[tp[0]].Y, transToVertex[tp[0]].X, transToVertex[tp[0]].Y);
                for (var t = 1; tp.Length > t; t += 1)
                {
                    box.X = Math.Min(box.X, transToVertex[tp[t]].X);
                    box.Y = Math.Min(box.Y, transToVertex[tp[t]].Y);
                    box.Z = Math.Max(box.Z, transToVertex[tp[t]].X);
                    box.W = Math.Max(box.W, transToVertex[tp[t]].Y);
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
                    var pIndex = polygonIndexes.Slice(rI, 3);

                    var box = boxCache[i];

                    var boxHash = box.GetHashCode();
                    var polyIndexHash = HashCode.Combine(pIndex[0], pIndex[1], pIndex[2]);
                    var polyHash = HashCode.Combine(transToVertex[pIndex[0]], transToVertex[pIndex[1]], transToVertex[pIndex[2]]);

                    var conflict = false;
                    for (var bi = 0; dispatchPolygonCount > bi; bi += 1)
                    {
                        conflict = BoxIntersect(boxBuffer[bi], box);
                        if (conflict)
                        {
                            var bPoly = bufferI.AsSpan(bi * 3, 3);

                            if (polyIndexHash == HashCode.Combine(bPoly[0], bPoly[1], bPoly[2])
                                && polyHash == HashCode.Combine(transToVertex[bPoly[0]], transToVertex[bPoly[1]], transToVertex[bPoly[2]])
                                && boxBuffer[bi].GetHashCode() == boxHash && boxBuffer[bi].Equals(box)
                                && pIndex[0] == bPoly[0] && pIndex[1] == bPoly[1] && pIndex[2] == bPoly[2]
                            )
                            {
                                need[i] = false;//完全一致しているポリゴンは見なかったことにします！
                            }
                            break;
                        }
                    }

                    if (conflict) { continue; }

                    pIndex.CopyTo(bufferI.AsSpan(dispatchPolygonCount * 3, 3));
                    boxBuffer[dispatchPolygonCount] = box;

                    need[i] = false;
                    dispatchPolygonCount += 1;
                }
                if (dispatchPolygonCount == 0) { break; }

                using var indexBuf = engine.SetStorageBufferFromUpload<TTCE, int>(computeHandler, polygonIndexID, bufferI.AsSpan(0, dispatchPolygonCount * 3));

                foreach (var (dispatchCount, indexOffset) in SliceDispatch((uint)dispatchPolygonCount, ushort.MaxValue))
                {
                    BitConverter.TryWriteBytes(gvBuf.Slice(20, 4), indexOffset);
                    computeHandler.UploadConstantsBuffer<byte>(gvBufId, gvBuf);
                    computeHandler.Dispatch(dispatchCount, 1, 1);
                }
            }
        }
        static bool BoxIntersect(TTVector4 a, TTVector4 b)
        {
            return (a.X <= b.Z && a.Z >= b.X) && (a.Y <= b.W && a.W >= b.Y);
        }
        public static void TransWrite<TTCE>(this TTCE engine, TTTransMappingHolder transMappingHolder, TTRenderTexWithPaddingDistance dist, ITTRenderTexture source, ITTSamplerKey samplerKey)
        where TTCE : ITexTransComputeKeyQuery
        , ITexTransGetComputeHandler
        {
            if (dist.Texture.EqualSize(transMappingHolder.TransMap) is false || source.Width != transMappingHolder.SourceSize.x || source.Hight != transMappingHolder.SourceSize.y) { throw new ArgumentException(); }

            using var sampleCompute = engine.GetComputeHandler(engine.GetExKeyQuery<ISamplerComputeKey>().TransSamplerKey[samplerKey]);

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
            sampleCompute.SetTexture(targetDistanceMapID, dist.PaddingDistanceMap);

            sampleCompute.DispatchWithTextureSize(dist.Texture);
        }

        public static void TransWarpModifierWithNone<TTCE>(this TTCE engine, TTTransMappingHolder transMappingHolder)
        where TTCE : ITexTransComputeKeyQuery, ITexTransGetComputeHandler
        {
            using var computeHandler = engine.GetComputeHandler(engine.GetExKeyQuery<ITransTextureComputeKey>().TransWarpNone);

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
            using var computeHandler = engine.GetComputeHandler(engine.GetExKeyQuery<ITransTextureComputeKey>().TransWarpNone);

            var transMapID = computeHandler.NameToID("TransMap");
            computeHandler.SetTexture(transMapID, transMappingHolder.TransMap);

            computeHandler.DispatchWithTextureSize(transMappingHolder.TransMap);
        }
    }

    public class TTTransMappingHolder : IDisposable
    {
        public ITTRenderTexture TransMap;
        public ITTRenderTexture DistanceMap, ScalingMap;
        public ITTRenderTexture? DepthMap;
        public readonly (int x, int y) TargetSize, SourceSize;
        public readonly float MaxDistance;
        private TTTransMappingHolder(ITTRenderTexture transMap, ITTRenderTexture distanceMap, ITTRenderTexture scalingMap, ITTRenderTexture? depthMap, (int x, int y) targetsize, (int x, int y) sourceSize, float maxDistance)
        {
            TransMap = transMap;
            DistanceMap = distanceMap;
            ScalingMap = scalingMap;
            DepthMap = depthMap;
            TargetSize = targetsize;
            SourceSize = sourceSize;
            MaxDistance = maxDistance;
        }
        public static TTTransMappingHolder Create<TTCE>(TTCE engine, (int x, int y) targetsize, (int x, int y) sourceSize, float maxDistance, bool useDepthMap = false)
        where TTCE : ITexTransCreateTexture
        , ITexTransComputeKeyQuery
        , ITexTransGetComputeHandler
        {
            var transMap = engine.CreateRenderTexture(targetsize.x, targetsize.y, TexTransCoreTextureChannel.RG);
            var distanceMap = engine.CreateRenderTexture(targetsize.x, targetsize.y, TexTransCoreTextureChannel.R);
            var scalingMap = engine.CreateRenderTexture(targetsize.x, targetsize.y, TexTransCoreTextureChannel.R);
            var depthMap = useDepthMap ? engine.CreateRenderTexture(targetsize.x, targetsize.y, TexTransCoreTextureChannel.R) : null;
            engine.FillR(distanceMap, maxDistance);
            return new(transMap, distanceMap, scalingMap, depthMap, targetsize, sourceSize, maxDistance);
        }
        public void Dispose()
        {
            TransMap?.Dispose();
            TransMap = null!;
            DistanceMap?.Dispose();
            DistanceMap = null!;
            ScalingMap?.Dispose();
            ScalingMap = null!;
            DepthMap?.Dispose();
            DepthMap = null;
        }
    }

    public class TTRenderTexWithPaddingDistance : IDisposable
    {
        public readonly bool Owned;
        public ITTRenderTexture Texture;
        public ITTRenderTexture PaddingDistanceMap;
        public readonly (int x, int y) Size;
        public readonly float MaxPaddingDistance;
        private TTRenderTexWithPaddingDistance(bool owned, ITTRenderTexture texture, ITTRenderTexture distanceMap, (int x, int y) size, float maxDistance)
        {
            Owned = owned;
            Texture = texture;
            Size = size;
            PaddingDistanceMap = distanceMap;
            MaxPaddingDistance = maxDistance;
        }
        public static TTRenderTexWithPaddingDistance Create<TTCE>(TTCE engine, (int x, int y) size, float maxPaddingDistance)
        where TTCE : ITexTransCreateTexture
        , ITexTransComputeKeyQuery
        , ITexTransGetComputeHandler
        {
            var texture = engine.CreateRenderTexture(size.x, size.y);
            var distanceMap = engine.CreateRenderTexture(size.x, size.y, TexTransCoreTextureChannel.R);
            engine.FillR(distanceMap, maxPaddingDistance);
            return new(true, texture, distanceMap, size, maxPaddingDistance);
        }
        public static TTRenderTexWithPaddingDistance CreateFrom<TTCE>(TTCE engine, ITTRenderTexture renderTexture, float maxPaddingDistance)
        where TTCE : ITexTransCreateTexture
        , ITexTransComputeKeyQuery
        , ITexTransGetComputeHandler
        {
            var distanceMap = engine.CreateRenderTexture(renderTexture.Width, renderTexture.Hight, TexTransCoreTextureChannel.R);
            engine.FillR(distanceMap, maxPaddingDistance);
            return new(false, renderTexture, distanceMap, renderTexture.Size(), maxPaddingDistance);
        }
        public void Dispose()
        {
            if (Owned)
            {
                Texture?.Dispose();
                Texture = null!;
            }
            PaddingDistanceMap?.Dispose();
            PaddingDistanceMap = null!;
        }
    }


}
