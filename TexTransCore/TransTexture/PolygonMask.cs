#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;

namespace net.rs64.TexTransCore.TransTexture
{
    public static class PolygonMask
    {
        public static void Write<TTCE>(TTCE engine
        , TTRenderTexWithPaddingDistance dist
        , ITTStorageBuffer uvVertex
        , ITTStorageBuffer polygonIndexesBuffer
        , int polygonCount
        )
        where TTCE : ITexTransComputeKeyQuery, ITexTransGetComputeHandler
        {
            var computeHandler = engine.GetComputeHandler(engine.GetExKeyQuery<IQuayGeneraleComputeKey>().GenealCompute["PolygonMask"]);

            var gvBufID = computeHandler.NameToID("gv");
            var maskMapID = computeHandler.NameToID("MaskMap");
            var paddingDistanceMapID = computeHandler.NameToID("PaddingDistanceMap");
            var polygonIndexID = computeHandler.NameToID("PolygonIndex");
            var uvVertexID = computeHandler.NameToID("UVVertex");

            Span<byte> gvBuf = stackalloc byte[16];
            BitConverter.TryWriteBytes(gvBuf.Slice(0, 4), dist.Size.x);
            BitConverter.TryWriteBytes(gvBuf.Slice(4, 4), dist.Size.y);
            BitConverter.TryWriteBytes(gvBuf.Slice(8, 4), dist.MaxPaddingDistance);
            BitConverter.TryWriteBytes(gvBuf.Slice(12, 4), 0);


            computeHandler.SetTexture(maskMapID, dist.Texture);
            computeHandler.SetTexture(paddingDistanceMapID, dist.PaddingDistanceMap);

            computeHandler.SetStorageBuffer(polygonIndexID, polygonIndexesBuffer);
            computeHandler.SetStorageBuffer(uvVertexID, uvVertex);

            foreach (var (dispatchCount, indexOffset) in TransMappingUtility.SliceDispatch((uint)polygonCount, ushort.MaxValue))
            {
                BitConverter.TryWriteBytes(gvBuf.Slice(12, 4), indexOffset);
                computeHandler.UploadConstantsBuffer<byte>(gvBufID, gvBuf);
                computeHandler.Dispatch(dispatchCount, 1, 1);
            }
        }
    }
}
