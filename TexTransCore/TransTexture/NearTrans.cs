#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;

namespace net.rs64.TexTransCore.TransTexture
{
    public static class NearTrans
    {
        public static void NearTransTexture<TTCE>(this TTCE engine
            , ITTRenderTexture sourceTex // read only
            , ITTRenderTexture targetTex // write only 内容が書き込まれていた場合の動作は未定義

            , ReadOnlySpan<TriangleIndex> sourceTriangles
            , ReadOnlySpan<TTVector4> sourceVertex // w は使用されない aliment の都合
            , ReadOnlySpan<Vector2> sourceUVVertex

            , ReadOnlySpan<TriangleIndex> targetTriangles
            , ReadOnlySpan<TTVector4> targetVertex
            , ReadOnlySpan<Vector2> targetUVVertex

            , float fadeStart
            , float maxDistance // fadeEnd でもある
            , float padding
            ) where TTCE : ITexTransCreateTexture
            , ITexTransComputeKeyQuery
            , ITexTransGetComputeHandler
            , ITexTransDriveStorageBufferHolder
        {
            var transMap = engine.CreateRenderTexture(targetTex.Width, targetTex.Hight, TexTransCoreTextureChannel.RG);
            var polygonToDistanceMap = engine.CreateRenderTexture(targetTex.Width, targetTex.Hight, TexTransCoreTextureChannel.R);

            var keyQ = engine.GetExKeyQuery<INearTransComputeKey>();
            var pixelCount = targetTex.Width * targetTex.Hight;
            var pdMapBufferLen = pixelCount * 4 * 4;// w * h * sizeof(float4)
            using var positionPaddingDistanceMapBuffer = engine.AllocateStorageBuffer(pdMapBufferLen);



            using var fillerHandler = engine.GetComputeHandler(keyQ.FilleFloat4StorageBuffer);
            var fillTarget = fillerHandler.NameToID("FillTarget");
            var fhBufId = fillerHandler.NameToID("gv");
            Span<float> gvFloats = stackalloc float[4];
            gvFloats[3] = padding;
            fillerHandler.UploadConstantsBuffer<float>(fhBufId, gvFloats);
            fillerHandler.SetStorageBuffer(fillTarget, positionPaddingDistanceMapBuffer);

            fillerHandler.DispatchWithSize((pixelCount, 1));



            using var positionMapperHandler = engine.GetComputeHandler(keyQ.PositionMapper);
            var pmGvID = positionMapperHandler.NameToID("gv");
            var pmPositionPaddingDistanceMapID = positionMapperHandler.NameToID("PositionPaddingDistanceMap");
            var pmPolygonIndexID = positionMapperHandler.NameToID("PolygonIndex");
            var pmVertexID = positionMapperHandler.NameToID("Vertex");
            var pmUVVertexID = positionMapperHandler.NameToID("UVVertex");

            Span<byte> pmGvBuf = stackalloc byte[16];
            BitConverter.TryWriteBytes(pmGvBuf.Slice(0, 4), targetTex.Width);
            BitConverter.TryWriteBytes(pmGvBuf.Slice(4, 4), targetTex.Hight);
            BitConverter.TryWriteBytes(pmGvBuf.Slice(8, 4), padding);
            BitConverter.TryWriteBytes(pmGvBuf.Slice(12, 4), 0);

            positionMapperHandler.SetStorageBuffer(pmPositionPaddingDistanceMapID, positionPaddingDistanceMapBuffer);
            using var pmTriBuf = engine.SetStorageBufferFromUpload(positionMapperHandler, pmPolygonIndexID, MemoryMarshal.Cast<TriangleIndex, int>(targetTriangles));
            using var pmVertBuf = engine.SetStorageBufferFromUpload(positionMapperHandler, pmVertexID, targetVertex);
            using var pmUVVertBuf = engine.SetStorageBufferFromUpload(positionMapperHandler, pmUVVertexID, targetUVVertex);

            BitConverter.TryWriteBytes(pmGvBuf.Slice(8, 4), 0f);
            positionMapperHandler.UploadConstantsBuffer<byte>(pmGvID, pmGvBuf);
            positionMapperHandler.Dispatch((uint)targetTriangles.Length, 1, 1);

            BitConverter.TryWriteBytes(pmGvBuf.Slice(8, 4), padding);
            positionMapperHandler.UploadConstantsBuffer<byte>(pmGvID, pmGvBuf);
            positionMapperHandler.Dispatch((uint)targetTriangles.Length, 1, 2);



            using var nearTransTextureHandler = engine.GetComputeHandler(keyQ.NearTransTexture);
            var ntGvID = nearTransTextureHandler.NameToID("gv");
            var ntPositionPaddingDistanceMapID = nearTransTextureHandler.NameToID("PositionPaddingDistanceMap");
            var ntTransMapID = nearTransTextureHandler.NameToID("TransMap");
            var ntPolygonToDistanceID = nearTransTextureHandler.NameToID("PolygonToDistance");
            var ntPolygonIndexID = nearTransTextureHandler.NameToID("PolygonIndex");
            var ntVertexID = nearTransTextureHandler.NameToID("Vertex");
            var ntUVVertexID = nearTransTextureHandler.NameToID("UVVertex");

            Span<byte> ntGvBuf = stackalloc byte[32];
            BitConverter.TryWriteBytes(ntGvBuf.Slice(0, 4), targetTex.Width);
            BitConverter.TryWriteBytes(ntGvBuf.Slice(4, 4), targetTex.Hight);
            BitConverter.TryWriteBytes(ntGvBuf.Slice(8, 4), maxDistance);
            BitConverter.TryWriteBytes(ntGvBuf.Slice(12, 4), padding);
            BitConverter.TryWriteBytes(ntGvBuf.Slice(16, 4), 0);
            BitConverter.TryWriteBytes(ntGvBuf.Slice(20, 4), sourceTriangles.Length);
            BitConverter.TryWriteBytes(ntGvBuf.Slice(24, 4), 0f);
            BitConverter.TryWriteBytes(ntGvBuf.Slice(28, 4), 0f);

            nearTransTextureHandler.UploadConstantsBuffer<byte>(ntGvID, ntGvBuf);

            nearTransTextureHandler.SetStorageBuffer(ntPositionPaddingDistanceMapID, positionPaddingDistanceMapBuffer);

            nearTransTextureHandler.SetTexture(ntTransMapID, transMap);
            nearTransTextureHandler.SetTexture(ntPolygonToDistanceID, polygonToDistanceMap);

            using var ntTriBuf = engine.SetStorageBufferFromUpload(nearTransTextureHandler, ntPolygonIndexID, MemoryMarshal.Cast<TriangleIndex, int>(sourceTriangles));
            using var ntVertBuf = engine.SetStorageBufferFromUpload(nearTransTextureHandler, ntVertexID, sourceVertex);
            using var ntUVVertBuf = engine.SetStorageBufferFromUpload(nearTransTextureHandler, ntUVVertexID, sourceUVVertex);

            nearTransTextureHandler.DispatchWithTextureSize(transMap);




            using var distanceMap = engine.CreateRenderTexture(targetTex.Width, targetTex.Hight, TexTransCoreTextureChannel.R);
            using var scaleMap = engine.CreateRenderTexture(targetTex.Width, targetTex.Hight, TexTransCoreTextureChannel.R);
            engine.FillR(scaleMap, 1.0f);
            using var TargetDistanceMap = engine.CreateRenderTexture(targetTex.Width, targetTex.Hight, TexTransCoreTextureChannel.R);
            engine.FillR(TargetDistanceMap, padding + 0.01f);

            using var sampleCompute = engine.GetComputeHandler(engine.GetExKeyQuery<ISamplerComputeKey>().TransSamplerKey[engine.StandardComputeKey.DefaultSampler]);

            var scReadTexID = sampleCompute.NameToID("ReadTex");
            var scReadTextureParmBufId = sampleCompute.NameToID("ReadTextureParm");

            var scTransMapID = sampleCompute.NameToID("TransMap");
            var scDistanceMapID = sampleCompute.NameToID("DistanceMap");
            var scScalingMapID = sampleCompute.NameToID("ScalingMap");

            var scTargetTexID = sampleCompute.NameToID("TargetTex");
            var scTargetDistanceMapID = sampleCompute.NameToID("TargetDistanceMap");


            Span<uint> scReadTextureBuf = stackalloc uint[4];
            scReadTextureBuf[0] = (uint)sourceTex.Width;
            scReadTextureBuf[1] = (uint)sourceTex.Hight;
            sampleCompute.UploadConstantsBuffer<uint>(scReadTextureParmBufId, scReadTextureBuf);
            sampleCompute.SetTexture(scReadTexID, sourceTex);

            sampleCompute.SetTexture(scTransMapID, transMap);
            sampleCompute.SetTexture(scDistanceMapID, distanceMap);
            sampleCompute.SetTexture(scScalingMapID, scaleMap);

            sampleCompute.SetTexture(scTargetTexID, targetTex);
            sampleCompute.SetTexture(scTargetDistanceMapID, TargetDistanceMap);

            sampleCompute.DispatchWithTextureSize(targetTex);



            using var nearDistanceFadeWriteHandler = engine.GetComputeHandler(keyQ.NearDistanceFadeWrite);
            var nfBufId = nearDistanceFadeWriteHandler.NameToID("gv");
            var nfTextureID = nearDistanceFadeWriteHandler.NameToID("Texture");
            var nfPolygonToDistanceID = nearDistanceFadeWriteHandler.NameToID("PolygonToDistance");
            Span<float> nfFloats = stackalloc float[4];
            nfFloats[0] = fadeStart;
            nfFloats[1] = maxDistance;
            nearDistanceFadeWriteHandler.UploadConstantsBuffer<float>(nfBufId, nfFloats);
            nearDistanceFadeWriteHandler.SetTexture(nfTextureID, targetTex);
            nearDistanceFadeWriteHandler.SetTexture(nfPolygonToDistanceID, polygonToDistanceMap);

            nearDistanceFadeWriteHandler.DispatchWithTextureSize(targetTex);

        }
    }
}
