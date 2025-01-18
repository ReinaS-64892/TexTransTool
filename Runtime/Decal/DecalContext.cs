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

    internal interface IFilteredTriangleHolder : IDisposable { NativeArray<TriangleIndex>[][] GetTriangles(); }


    internal class DecalContext<SpaceConvertor, TDecalSpace, TrianglesFilter, TFilteringOutput>
    where SpaceConvertor : ISpaceConverter<TDecalSpace>
    where TDecalSpace : IDecalSpace
    where TrianglesFilter : ITrianglesFilter<TDecalSpace, TFilteringOutput>
    where TFilteringOutput : IFilteredTriangleHolder
    {
        ITexTransToolForUnity _ttce4u;
        SpaceConvertor _spaceConvertor;
        TrianglesFilter _trianglesFilter;


        public string TargetPropertyName { get; set; } = "_MainTex";
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

        internal Dictionary<KeyTexture, TTRenderTexWithDistance>? WriteDecalTexture<KeyTexture>(IEnumerable<Renderer> targetRenderers, ITTRenderTexture sourceTexture)
        where KeyTexture : Texture
        {
            targetRenderers = targetRenderers.Where(i => i is SkinnedMeshRenderer or MeshRenderer).Where(i => i.GetMesh() != null).ToArray();

            if (targetRenderers.Any() is false) { return null; }


            Profiler.BeginSample("GetMeshData");
            var meshData = targetRenderers.Select(i => i.GetToMemorizedMeshData()).ToArray();
            Profiler.EndSample();

            Profiler.BeginSample("ConvertSpace");
            using var decalSpace = _spaceConvertor.ConvertSpace(meshData);
            Profiler.EndSample();

            Profiler.BeginSample("Filtering");
            using var filteredTriangles = _trianglesFilter.Filtering(decalSpace);
            Profiler.EndSample();

            Profiler.BeginSample("WriteDepth");
            DepthBufferHolder? depthBuffer;
            switch (decalSpace, UseDepthOrInvert.HasValue)
            {
                case (IDecalSpaceWith3D ds3D, true):
                    {
                        var depthBufferSize = sourceTexture.Size();

                        // 最低保証
                        depthBufferSize.x = Math.Max(depthBufferSize.x, 2048);
                        depthBufferSize.y = Math.Max(depthBufferSize.y, 2048);

                        depthBuffer = DepthBufferHolder.Create(_ttce4u, depthBufferSize);

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


                                using var pBuf = _ttce4u.UploadStorageBuffer<int>(MemoryMarshal.Cast<TriangleIndex, int>(fromTri.AsSpan()));
                                _ttce4u.WriteDepth(depthBuffer, vertexBuf, depthVBuf, pBuf, fromTri.Length);
                            }
                        }
                        Profiler.EndSample();
                        break;
                    }
                default: { depthBuffer = null; break; }
            }

            Profiler.EndSample();

            Profiler.BeginSample("Write Decals");
            var renderTextureHolders = new Dictionary<KeyTexture, TTRenderTexWithDistance>();

            for (var r = 0; meshData.Length > r; r += 1)
            {
                var md = meshData[r];
                var materials = md.ReferenceRenderer.sharedMaterials;
                var validSlot = Math.Min(md.TriangleIndex.Length, materials.Length);

                var transToVertex = MemoryMarshal.Cast<Vector2, System.Numerics.Vector2>(md.VertexUV);
                var transFromVertex = decalSpace.OutputUV()[r];
                using var transToVertexBuf = _ttce4u.UploadStorageBuffer<System.Numerics.Vector2>(transToVertex);
                using var transFromVertexBuf = _ttce4u.UploadStorageBuffer<Vector2>(transFromVertex);
                ITTStorageBuffer? transFromDepth = null;
                try
                {
                    if (depthBuffer is not null && decalSpace is IDecalSpaceWith3D decalSpaceWith3D)
                    {
                        transFromDepth = _ttce4u.UploadStorageBuffer<float>(decalSpaceWith3D.OutputDepth()[r]);
                    }


                    for (var s = 0; validSlot > s; s += 1)
                    {
                        var mat = materials[s];
                        if (mat == null) { continue; }
                        if (mat.HasProperty(TargetPropertyName) is false) { continue; }

                        var decalTargetTexture = mat.GetTexture(TargetPropertyName) as KeyTexture;
                        if (decalTargetTexture == null) { continue; }
                        if (DrawMaskMaterials is not null && DrawMaskMaterials.Contains(mat) is false) { continue; }

                        var triangleIndexes = filteredTriangles.GetTriangles()[r][s];
                        if (triangleIndexes.Length is 0) { continue; }

                        if (renderTextureHolders.ContainsKey(decalTargetTexture) is false)
                        {
                            var newTempRt = renderTextureHolders[decalTargetTexture] = TTRenderTexWithDistance.Create(_ttce4u, (decalTargetTexture.width, decalTargetTexture.height), DecalPadding);
                            newTempRt.Texture.Name = $"{decalTargetTexture.name}-CreateWriteDecalTexture-{newTempRt.Texture.Width}x{newTempRt.Texture.Hight}";
                            newTempRt.DistanceMap.Name = $"{decalTargetTexture.name}-CreateDistanceDecalTexture-{newTempRt.Texture.Width}x{newTempRt.Texture.Hight}";
                        }

                        var writeTarget = renderTextureHolders[decalTargetTexture];
                        using var transMappingHolder = TTTransMappingHolder.Create(_ttce4u, writeTarget.Texture.Size(), sourceTexture.Size(), DecalPadding, depthBuffer is not null);

                        var polygonIndexesSpan = MemoryMarshal.Cast<TriangleIndex, int>(triangleIndexes);
                        var polygonCount = triangleIndexes.Length;
                        if (HighQualityPadding is false)
                        {
                            using var polygonIndexesBuffer = _ttce4u.UploadStorageBuffer<int>(polygonIndexesSpan);
                            _ttce4u.WriteMapping(transMappingHolder, transToVertexBuf, transFromVertexBuf, polygonIndexesBuffer, polygonCount, transFromDepth);
                        }
                        else _ttce4u.WriteMappingHighQuality(transMappingHolder, transToVertex, transToVertexBuf, transFromVertexBuf, polygonIndexesSpan, transFromDepth);

                        if (depthBuffer is not null) { _ttce4u.DepthCulling(transMappingHolder, depthBuffer, UseDepthOrInvert!.Value); }

                        if (IsTextureStretch) _ttce4u.TransWarpModifierWithStretch(transMappingHolder);
                        else _ttce4u.TransWarpModifierWithNone(transMappingHolder);

                        _ttce4u.TransWrite(transMappingHolder, writeTarget, sourceTexture, _ttce4u.StandardComputeKey.DefaultSampler);
                    }
                }
                finally
                {
                    transFromDepth?.Dispose();
                }
            }
            Profiler.EndSample();

            return renderTextureHolders;
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
        internal static IEnumerable<Renderer> FilterDecalTarget(IEnumerable<Renderer> targetRenderers, string targetPropertyName)
        {
            foreach (var tr in targetRenderers)
            {
                if (tr is not (SkinnedMeshRenderer or MeshRenderer)) { continue; }
                if (tr.GetMesh() == null) { continue; }
                foreach (var mat in tr.sharedMaterials)
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
