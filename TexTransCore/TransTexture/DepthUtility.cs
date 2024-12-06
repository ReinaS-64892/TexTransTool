#nullable enable
using System;

namespace net.rs64.TexTransCore.TransTexture
{
    public static class DepthUtility
    {
        public static void DepthCulling<TTCE>(this TTCE engine, TTTransMappingHolder transMappingHolder, ITTStorageBuffer polygons, uint polygonCount, bool depthInverse = false, float depthOffset = 0.01f)
        where TTCE : ITexTransComputeKeyQuery, ITexTransGetComputeHandler, ITexTransDriveStorageBufferHolder
        {
            using var depthRendererHandler = engine.GetComputeHandler(engine.TransTextureComputeKey.DepthRenderer);
            using var cullingDepthHandler = engine.GetComputeHandler(engine.TransTextureComputeKey.CullingDepth);

            var drGvID = depthRendererHandler.NameToID("gv");
            var drPolygonID = depthRendererHandler.NameToID("Polygons");
            var drDepthBufferID = depthRendererHandler.NameToID("DepthBuffer");

            var cdGvID = cullingDepthHandler.NameToID("gv");
            var cdDepthBufferID = cullingDepthHandler.NameToID("DepthBuffer");
            var cdTransMapID = cullingDepthHandler.NameToID("TransMap");
            var cdAdditionalDataMapID = cullingDepthHandler.NameToID("AdditionalDataMap");
            var cdDistanceMapID = cullingDepthHandler.NameToID("DistanceMap");


            Span<uint> drGV = stackalloc uint[4];
            drGV[0] = (uint)transMappingHolder.SourceSize.x;
            drGV[1] = (uint)transMappingHolder.SourceSize.y;
            depthRendererHandler.UploadConstantsBuffer<uint>(drGvID, drGV);
            depthRendererHandler.SetStorageBuffer(drPolygonID, polygons);
            using var depthBuffer = engine.SetStorageBufferFromAllocate(depthRendererHandler, drDepthBufferID, transMappingHolder.SourceSize.x * transMappingHolder.SourceSize.y * 4);

            depthRendererHandler.Dispatch(polygonCount, 1, 1);


            Span<byte> cdGV = stackalloc byte[32];
            BitConverter.TryWriteBytes(cdGV.Slice(0, 4), transMappingHolder.SourceSize.x);
            BitConverter.TryWriteBytes(cdGV.Slice(4, 4), transMappingHolder.SourceSize.y);
            BitConverter.TryWriteBytes(cdGV.Slice(8, 4), depthInverse ? 1 : 0);
            BitConverter.TryWriteBytes(cdGV.Slice(12, 4), transMappingHolder.MaxDistance);
            BitConverter.TryWriteBytes(cdGV.Slice(16, 4), depthOffset);
            BitConverter.TryWriteBytes(cdGV.Slice(20, 4), 0);
            BitConverter.TryWriteBytes(cdGV.Slice(24, 4), 0);
            BitConverter.TryWriteBytes(cdGV.Slice(28, 4), 0);
            cullingDepthHandler.UploadConstantsBuffer<byte>(cdGvID, cdGV);

            cullingDepthHandler.SetStorageBuffer(cdDepthBufferID, depthBuffer);

            cullingDepthHandler.SetTexture(cdTransMapID, transMappingHolder.TransMap);
            cullingDepthHandler.SetTexture(cdAdditionalDataMapID, transMappingHolder.AdditionalDataMap);
            cullingDepthHandler.SetTexture(cdDistanceMapID, transMappingHolder.DistanceMap);

            cullingDepthHandler.DispatchWithTextureSize(transMappingHolder.TransMap);
        }

    }
}
