#nullable enable
using System;
using System.Numerics;

namespace net.rs64.TexTransCore.TransTexture
{
    public static class DepthUtility
    {
        public static void WriteDepth<TTCE>(this TTCE engine, DepthBufferHolder depthBuffer, ITTStorageBuffer vertex, ITTStorageBuffer depthVertex, ITTStorageBuffer polygons, int polygonCount)
        where TTCE : ITexTransComputeKeyQuery, ITexTransGetComputeHandler, ITexTransDriveStorageBufferHolder
        {
            using var depthRendererHandler = engine.GetComputeHandler(engine.TransTextureComputeKey.DepthRenderer);

            var drGvID = depthRendererHandler.NameToID("gv");
            var drPolygonID = depthRendererHandler.NameToID("Polygons");
            var drVertexID = depthRendererHandler.NameToID("Vertex");
            var drVertexDepthID = depthRendererHandler.NameToID("VertexDepth");
            var drDepthBufferID = depthRendererHandler.NameToID("DepthBuffer");

            Span<uint> drGV = stackalloc uint[4];
            drGV[0] = (uint)depthBuffer.DepthBufferSize.x;
            drGV[1] = (uint)depthBuffer.DepthBufferSize.y;
            depthRendererHandler.SetStorageBuffer(drVertexID, vertex);
            depthRendererHandler.SetStorageBuffer(drVertexDepthID, depthVertex);
            depthRendererHandler.SetStorageBuffer(drPolygonID, polygons);
            depthRendererHandler.SetStorageBuffer(drDepthBufferID, depthBuffer.DepthBuffer);

            foreach (var (dispatchCount, indexOffset) in TransMappingUtility.SliceDispatch((uint)polygonCount, ushort.MaxValue))
            {
                drGV[2] = indexOffset;
                depthRendererHandler.UploadConstantsBuffer<uint>(drGvID, drGV);
                depthRendererHandler.Dispatch(dispatchCount, 1, 1);
            }
        }
        public static void DepthCulling<TTCE>(this TTCE engine, TTTransMappingHolder transMappingHolder, DepthBufferHolder depthBuffer, bool depthInverse = false, float depthOffset = 0.01f)
        where TTCE : ITexTransComputeKeyQuery, ITexTransGetComputeHandler, ITexTransDriveStorageBufferHolder
        {
            if (transMappingHolder.DepthMap is null) { throw new NullReferenceException(); }
            using var cullingDepthHandler = engine.GetComputeHandler(engine.TransTextureComputeKey.CullingDepth);

            var cdGvID = cullingDepthHandler.NameToID("gv");
            var cdDepthBufferID = cullingDepthHandler.NameToID("DepthBuffer");
            var cdTransMapID = cullingDepthHandler.NameToID("TransMap");
            var cdTransDepthMapID = cullingDepthHandler.NameToID("TransDepthMap");
            var cdDistanceMapID = cullingDepthHandler.NameToID("DistanceMap");

            Span<byte> cdGV = stackalloc byte[32];
            BitConverter.TryWriteBytes(cdGV.Slice(0, 4), depthBuffer.DepthBufferSize.x);
            BitConverter.TryWriteBytes(cdGV.Slice(4, 4), depthBuffer.DepthBufferSize.y);
            BitConverter.TryWriteBytes(cdGV.Slice(8, 4), depthInverse ? 1 : 0);
            BitConverter.TryWriteBytes(cdGV.Slice(12, 4), transMappingHolder.MaxDistance);
            BitConverter.TryWriteBytes(cdGV.Slice(16, 4), depthOffset);
            BitConverter.TryWriteBytes(cdGV.Slice(20, 4), 0);
            BitConverter.TryWriteBytes(cdGV.Slice(24, 4), 0);
            BitConverter.TryWriteBytes(cdGV.Slice(28, 4), 0);
            cullingDepthHandler.UploadConstantsBuffer<byte>(cdGvID, cdGV);

            cullingDepthHandler.SetStorageBuffer(cdDepthBufferID, depthBuffer.DepthBuffer);

            cullingDepthHandler.SetTexture(cdTransMapID, transMappingHolder.TransMap);
            cullingDepthHandler.SetTexture(cdTransDepthMapID, transMappingHolder.DepthMap!);
            cullingDepthHandler.SetTexture(cdDistanceMapID, transMappingHolder.DistanceMap);

            cullingDepthHandler.DispatchWithTextureSize(transMappingHolder.TransMap);
        }

    }

    public class DepthBufferHolder : IDisposable
    {
        public ITTStorageBuffer DepthBuffer;
        public (int x, int y) DepthBufferSize;
        public DepthBufferHolder(ITTStorageBuffer depthBuffer, (int x, int y) sourceSize)
        {
            DepthBuffer = depthBuffer;
            DepthBufferSize = sourceSize;
        }
        public static DepthBufferHolder Create<TTCE>(TTCE engine, (int x, int y) sourceSize)
        where TTCE : ITexTransDriveStorageBufferHolder
        { return new(engine.AllocateStorageBuffer(sourceSize.x * sourceSize.y * 4), sourceSize); }
        public void Dispose() { DepthBuffer = null!; }
    }
}
