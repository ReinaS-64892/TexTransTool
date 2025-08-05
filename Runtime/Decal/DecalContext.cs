#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;
using net.rs64.TexTransCoreEngineForUnity;
using Unity.Collections;
using UnityEngine.Profiling;
using net.rs64.TexTransCore;
using net.rs64.TexTransTool.Utils;
using net.rs64.TexTransCore.TransTexture;
using System.Runtime.InteropServices;
using System.Linq;

namespace net.rs64.TexTransTool.Decal
{
    internal interface ISpaceConverter<TDecalSpace>
    where TDecalSpace : IDecalSpace
    {
        // この MeshData の解放責任は受け取らない
        // そして後に返す OutputUV や Triangles の配列の数は MeshData の数に一致据えるようにね！
        TDecalSpace ConvertSpace(MeshData[] meshData);
    }

    internal interface IDecalSpace : IDisposable { NativeArray<Vector2>[] OutputUV(); }

    // DepthDecal をやりたい場合は 3D 必須
    internal interface IDecalSpaceWith3D : IDecalSpace { NativeArray<float>[] OutputDepth(); }

    internal interface ITrianglesFilter<TDecalSpace, TFilteringOutput>
    where TDecalSpace : IDecalSpace
    where TFilteringOutput : IFilteredTriangleHolder
    {
        TFilteringOutput Filtering(TDecalSpace space);
    }

    internal interface IFilteredTriangleHolder : IDisposable { NativeArray<TriangleVertexIndices>[][] GetTriangles(); }


    internal class DecalContext<SpaceConvertor, TDecalSpace, TrianglesFilter, TFilteringOutput>
    where SpaceConvertor : ISpaceConverter<TDecalSpace>
    where TDecalSpace : IDecalSpace
    where TrianglesFilter : ITrianglesFilter<TDecalSpace, TFilteringOutput>
    where TFilteringOutput : IFilteredTriangleHolder
    {
        ITexTransToolForUnity _ttce4u;
        SpaceConvertor _spaceConvertor;
        TrianglesFilter _trianglesFilter;


        public bool IsTextureStretch { get; set; } = false;
        public float DecalPadding { get; set; } = 5f;
        public bool HighQualityPadding { get; set; } = false;
        public bool? UseDepthOrInvert { get; set; } = null;
        public HashSet<Material>? DrawMaskMaterials = null;

        public DecalContext(ITexTransToolForUnity ttce4u, SpaceConvertor spaceConvertor, TrianglesFilter trianglesFilter)
        {
            _ttce4u = ttce4u;
            _spaceConvertor = spaceConvertor;
            _trianglesFilter = trianglesFilter;
        }

        internal Dictionary<KeyTexture, TTRenderTexWithPaddingDistance>? WriteDecalTexture<KeyTexture>(
        IDomainReferenceViewer targeting
        , IEnumerable<Renderer> targetRenderers
        , ITTRenderTexture sourceTexture
        , string targetPropertyName = "_MainTex"
        )
        where KeyTexture : Texture
        {
            targetRenderers = targetRenderers.Where(i => i is SkinnedMeshRenderer or MeshRenderer).Where(i => i.GetMesh() != null).ToArray();
            if (targetRenderers.Any() is false) { return null; }

            Profiler.BeginSample("GetMeshData and converting and more");
            var meshData = targetRenderers.Select(targeting.GetMeshData).ToArray();
            using var decalSpace = _spaceConvertor.ConvertSpace(meshData);
            using var filteredTriangles = _trianglesFilter.Filtering(decalSpace);
            var depthBuffer = GenerateAndWriteDepthBuffer(sourceTexture, decalSpace, filteredTriangles);
            Profiler.EndSample();

            Profiler.BeginSample("Write Decals");
            var renderTextureHolders = new Dictionary<KeyTexture, TTRenderTexWithPaddingDistance>();

            for (var r = 0; meshData.Length > r; r += 1)
            {
                var md = meshData[r];
                var materials = targeting.GetMaterials(md.ReferenceRenderer);
                var validSlot = Math.Min(md.TriangleVertexIndices.Length, materials.Length);

                using var decalMesh = new DecalMeshHolder(_ttce4u, md, decalSpace, r);

                for (var s = 0; validSlot > s; s += 1)
                {
                    var mat = materials[s];
                    if (mat == null) { continue; }

                    if (mat.HasProperty(targetPropertyName) is false) { continue; }
                    var decalTargetTexture = mat.GetTexture(targetPropertyName) as KeyTexture;

                    if (decalTargetTexture == null) { continue; }
                    if (DrawMaskMaterials is not null && DrawMaskMaterials.Contains(mat) is false) { continue; }

                    var triangleIndexes = filteredTriangles.GetTriangles()[r][s];
                    if (triangleIndexes.Length is 0) { continue; }

                    if (renderTextureHolders.ContainsKey(decalTargetTexture) is false)
                    {
                        var newTempRt = renderTextureHolders[decalTargetTexture] = TTRenderTexWithPaddingDistance.Create(_ttce4u, (decalTargetTexture.width, decalTargetTexture.height), DecalPadding);
                        newTempRt.Texture.Name = $"{decalTargetTexture.name}-CreateWriteDecalTexture-{newTempRt.Texture.Width}x{newTempRt.Texture.Hight}";
                        newTempRt.PaddingDistanceMap.Name = $"{decalTargetTexture.name}-CreateDistanceDecalTexture-{newTempRt.Texture.Width}x{newTempRt.Texture.Hight}";
                    }
                    var writeTarget = renderTextureHolders[decalTargetTexture];

                    WriteDecal(writeTarget, sourceTexture, decalMesh, triangleIndexes, depthBuffer);
                }

            }
            Profiler.EndSample();

            return renderTextureHolders;
        }


        internal bool WriteDecalTextureWithSingleTexture(
            IDomainReferenceViewer targeting
            , IEnumerable<Renderer> targetRenderers
            , ITTRenderTexture decalWriteTarget
            , ITTRenderTexture sourceTexture
            )
        {
            targetRenderers = targetRenderers.Where(i => i is SkinnedMeshRenderer or MeshRenderer).Where(i => i.GetMesh() != null).ToArray();
            if (targetRenderers.Any() is false) { return false; }

            using var writeTarget = TTRenderTexWithPaddingDistance.CreateFrom(_ttce4u, decalWriteTarget, DecalPadding);

            Profiler.BeginSample("GetMeshData and converting and more");
            var meshData = targetRenderers.Select(targeting.GetMeshData).ToArray();
            using var decalSpace = _spaceConvertor.ConvertSpace(meshData);
            using var filteredTriangles = _trianglesFilter.Filtering(decalSpace);
            var depthBuffer = GenerateAndWriteDepthBuffer(sourceTexture, decalSpace, filteredTriangles);
            Profiler.EndSample();

            Profiler.BeginSample("Write Decals");
            for (var r = 0; meshData.Length > r; r += 1)
            {
                var md = meshData[r];
                var materials = targeting.GetMaterials(md.ReferenceRenderer);
                var validSlot = Math.Min(md.TriangleVertexIndices.Length, materials.Length);

                using var decalMesh = new DecalMeshHolder(_ttce4u, md, decalSpace, r);

                for (var s = 0; validSlot > s; s += 1)
                {
                    var mat = materials[s];
                    if (mat == null) { continue; }
                    if (DrawMaskMaterials is not null && DrawMaskMaterials.Contains(mat) is false) { continue; }

                    var triangleIndexes = filteredTriangles.GetTriangles()[r][s];
                    if (triangleIndexes.Length is 0) { continue; }

                    WriteDecal(writeTarget, sourceTexture, decalMesh, triangleIndexes, depthBuffer);
                }

            }
            Profiler.EndSample();
            return true;
        }
        private DepthBufferHolder? GenerateAndWriteDepthBuffer(ITTRenderTexture sourceTexture, TDecalSpace decalSpace, TFilteringOutput filteredTriangles)
        {
            switch (decalSpace, UseDepthOrInvert.HasValue)
            {
                case (IDecalSpaceWith3D ds3D, true):
                    {
                        var depthBufferSize = sourceTexture.Size();

                        // 最低保証
                        depthBufferSize.x = Math.Max(depthBufferSize.x, 2048);
                        depthBufferSize.y = Math.Max(depthBufferSize.y, 2048);

                        var depthBuffer = DepthBufferHolder.Create(_ttce4u, depthBufferSize);

                        Profiler.BeginSample("Write Depth");
                        var outSourceUV = ds3D.OutputUV();
                        var outDepth = ds3D.OutputDepth();
                        for (var i = 0; outSourceUV.Length > i; i += 1)
                        {
                            using var vertexBuf = _ttce4u.UploadStorageBuffer<Vector2>(outSourceUV[i]);
                            using var depthVBuf = _ttce4u.UploadStorageBuffer<float>(outDepth[i]);
                            var triangles = filteredTriangles.GetTriangles()[i];
                            for (var s = 0; triangles.Length > s; s += 1)
                            {
                                var fromTri = triangles[s];
                                if (fromTri.Length is 0) { continue; }


                                using var pBuf = _ttce4u.UploadStorageBuffer<int>(MemoryMarshal.Cast<TriangleVertexIndices, int>(fromTri.AsSpan()));
                                _ttce4u.WriteDepth(depthBuffer, vertexBuf, depthVBuf, pBuf, fromTri.Length);
                            }
                        }
                        Profiler.EndSample();
                        return depthBuffer;
                    }
                default: { return null; }
            }
        }
        private void WriteDecal(TTRenderTexWithPaddingDistance writeTarget, ITTRenderTexture sourceTexture, DecalMeshHolder decalMesh, NativeArray<TriangleVertexIndices> triangleIndexes, DepthBufferHolder? depthBuffer)
        {
            using var transMappingHolder = TTTransMappingHolder.Create(_ttce4u, writeTarget.Texture.Size(), sourceTexture.Size(), DecalPadding, depthBuffer is not null);

            var polygonIndexesSpan = MemoryMarshal.Cast<TriangleVertexIndices, int>(triangleIndexes);
            var polygonCount = triangleIndexes.Length;

            if (HighQualityPadding is false)
            {
                using var polygonIndexesBuffer = _ttce4u.UploadStorageBuffer<int>(polygonIndexesSpan);
                _ttce4u.WriteMapping(transMappingHolder, decalMesh.TransToVertexBuf, decalMesh.TransFromVertexBuf, polygonIndexesBuffer, polygonCount, decalMesh.TransFromDepth);
            }
            else
            {
                var transToVertex = MemoryMarshal.Cast<Vector2, System.Numerics.Vector2>(decalMesh.MeshData.VertexUV);
                _ttce4u.WriteMappingHighQuality(transMappingHolder, transToVertex, decalMesh.TransToVertexBuf, decalMesh.TransFromVertexBuf, polygonIndexesSpan, decalMesh.TransFromDepth);
            }

            if (depthBuffer is not null) { _ttce4u.DepthCulling(transMappingHolder, depthBuffer, UseDepthOrInvert!.Value); }

            if (IsTextureStretch) _ttce4u.TransWarpModifierWithStretch(transMappingHolder);
            else _ttce4u.TransWarpModifierWithNone(transMappingHolder);

            _ttce4u.TransWrite(transMappingHolder, writeTarget, sourceTexture, _ttce4u.StandardComputeKey.DefaultSampler);
        }

        struct DecalMeshHolder : IDisposable
        {
            public readonly MeshData MeshData;
            public ITTStorageBuffer TransToVertexBuf;
            public ITTStorageBuffer TransFromVertexBuf;
            public ITTStorageBuffer? TransFromDepth;
            public DecalMeshHolder(ITexTransToolForUnity engine, MeshData meshData, TDecalSpace decalSpace, int meshIndex)
            {
                MeshData = meshData;
                var transToVertex = MemoryMarshal.Cast<Vector2, System.Numerics.Vector2>(MeshData.VertexUV);
                var transFromVertex = decalSpace.OutputUV()[meshIndex];

                TransToVertexBuf = engine.UploadStorageBuffer<System.Numerics.Vector2>(transToVertex);
                TransFromVertexBuf = engine.UploadStorageBuffer<Vector2>(transFromVertex);

                if (decalSpace is IDecalSpaceWith3D decalSpaceWith3D) TransFromDepth = engine.UploadStorageBuffer<float>(decalSpaceWith3D.OutputDepth()[meshIndex]);
                else TransFromDepth = null;
            }
            public void Dispose()
            {
                TransToVertexBuf?.Dispose();
                TransToVertexBuf = null!;
                TransFromVertexBuf?.Dispose();
                TransFromVertexBuf = null!;
                TransFromDepth?.Dispose();
                TransFromDepth = null;
            }
        }

    }

    public enum PolygonCulling
    {
        Vertex,
        Edge,
        EdgeAndCenterRay,
    }

    internal static class DecalContextUtility
    {
        internal static MeshData GetToMemorizedMeshData(this Renderer r) => r.Memo(MeshData.GetMeshData, i => i.Dispose());
        internal static IEnumerable<Renderer> FilterDecalTarget(IDomainReferenceViewer rendererTargeting, IEnumerable<Renderer> targetRenderers, string targetPropertyName)
        {
            foreach (var tr in targetRenderers)
            {
                if (tr is not (SkinnedMeshRenderer or MeshRenderer)) { continue; }
                if (tr.GetMesh() == null) { continue; }
                foreach (var mat in rendererTargeting.GetMaterials(tr))
                {
                    if (mat == null) { continue; }
                    var targetTex = mat.HasProperty(targetPropertyName) ? mat.GetTexture(targetPropertyName) : null;
                    if (targetTex == null) { continue; }
                    yield return tr;
                    break;
                }
            }
        }
    }
}
