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

    internal interface IDecalSpace : IDisposable { }
    internal interface IDecalSpaceWith2D : IDecalSpace { NativeArray<Vector2>[] OutputUV(); }
    // DepthDecal をやりたい場合は 3D 必須だが、メモリ的に扱いやすいほうを片方だけ実装してよい。
    // どちらであっても、GPU にメモリを転送するときは同じ扱いになるから。
    internal interface IDecalSpaceWith3D : IDecalSpace { NativeArray<Vector3>[] OutputUV(); }

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

            Profiler.BeginSample("to Triangle upload");
            var toTrianglesStorageBuffers = new NativeArray<Vector2>?[meshData.Length][];
            for (var i = 0; toTrianglesStorageBuffers.Length > i; i += 1)
            {
                var refMeshData = meshData[i];
                var triSubBuffer = new NativeArray<Vector2>?[refMeshData.TriangleIndex.Length];
                for (var s = 0; triSubBuffer.Length > s; s += 1)
                {
                    var triangles = filteredTriangles.GetTriangles()[i][s];
                    if (triangles.Length == 0) { triSubBuffer[s] = null; continue; }
                    // TODO : Job を用いた高速化
                    var packedFromTriangle = TransTexture.PackingTrianglesForTo(triangles, refMeshData.VertexUV, Allocator.Temp);
                    triSubBuffer[s] = packedFromTriangle;
                }
                toTrianglesStorageBuffers[i] = triSubBuffer;
            }
            Profiler.EndSample();

            Profiler.BeginSample("from Triangle upload and WriteDepth");
            var fromTrianglesStorageBuffers = new NativeArray<Vector4>?[meshData.Length][];
            switch (decalSpace)
            {
                default: { throw new NotSupportedException(); }
                case IDecalSpaceWith3D decalSpaceWith3D:
                    {
                        var outSourceUV = decalSpaceWith3D.OutputUV();
                        for (var i = 0; fromTrianglesStorageBuffers.Length > i; i += 1)
                        {
                            var refMeshData = meshData[i];
                            var triSubBuffer = new NativeArray<Vector4>?[refMeshData.TriangleIndex.Length];
                            for (var s = 0; triSubBuffer.Length > s; s += 1)
                            {
                                var triangles = filteredTriangles.GetTriangles()[i][s];
                                if (triangles.Length == 0) { triSubBuffer[s] = null; continue; }
                                // TODO : Job を用いた高速化
                                var packedFromTriangle = TransTexture.PackingTrianglesForFrom(triangles, outSourceUV[i], Allocator.Temp);
                                triSubBuffer[s] = packedFromTriangle;

                            }
                            fromTrianglesStorageBuffers[i] = triSubBuffer;
                        }
                        break;
                    }
                case IDecalSpaceWith2D decalSpaceWith2D:
                    {
                        var outSourceUV = decalSpaceWith2D.OutputUV();
                        for (var i = 0; fromTrianglesStorageBuffers.Length > i; i += 1)
                        {
                            var refMeshData = meshData[i];
                            var triSubBuffer = new NativeArray<Vector4>?[refMeshData.TriangleIndex.Length];
                            for (var s = 0; triSubBuffer.Length > s; s += 1)
                            {
                                var triangles = filteredTriangles.GetTriangles()[i][s];
                                if (triangles.Length == 0) { triSubBuffer[s] = null; continue; }
                                // TODO : Job を用いた高速化
                                var packedFromTriangle = TransTexture.PackingTrianglesForFrom(triangles, outSourceUV[i], Allocator.Temp);
                                triSubBuffer[s] = packedFromTriangle;
                            }
                            fromTrianglesStorageBuffers[i] = triSubBuffer;
                        }
                        break;
                    }
            }

            DepthBufferHolder? depthBuffer;
            switch (decalSpace, UseDepthOrInvert.HasValue)
            {
                case (IDecalSpaceWith3D, true):
                    {
                        var depthBufferSize = sourceTexture.Size();

                        // 最低保証
                        depthBufferSize.x = Math.Max(depthBufferSize.x, 2048);
                        depthBufferSize.y = Math.Max(depthBufferSize.y, 2048);

                        depthBuffer = DepthBufferHolder.Create(_ttce4u, depthBufferSize);

                        Profiler.BeginSample("Write Depth");
                        for (var i = 0; fromTrianglesStorageBuffers.Length > i; i += 1)
                        {
                            var sub = fromTrianglesStorageBuffers[i];
                            for (var s = 0; sub.Length > s; s += 1)
                            {
                                var fromTri = sub[s];
                                if (fromTri.HasValue is false) { continue; }

                                _ttce4u.WriteDepth(depthBuffer, MemoryMarshal.Cast<Vector4, TTVector4>(fromTri.Value));
                            }
                        }
                        Profiler.EndSample();
                        break;
                    }
                default: { depthBuffer = null; break; }
            }

            var renderTextureHolders = new Dictionary<KeyTexture, TTRenderTexWithDistance>();
            Profiler.EndSample();
            try
            {
                Profiler.BeginSample("Write Decals");

                for (var r = 0; meshData.Length > r; r += 1)
                {
                    var md = meshData[r];
                    var materials = md.ReferenceRenderer.sharedMaterials;
                    var validSlot = Math.Min(md.TriangleIndex.Length, materials.Length);

                    for (var s = 0; validSlot > s; s += 1)
                    {
                        var mat = materials[s];
                        if (mat == null) { continue; }
                        if (mat.HasProperty(TargetPropertyName) is false) { continue; }

                        var decalTargetTexture = mat.GetTexture(TargetPropertyName) as KeyTexture;
                        if (decalTargetTexture == null) { continue; }
                        if (fromTrianglesStorageBuffers[r][s] is null || toTrianglesStorageBuffers[r][s] is null) { continue; }
                        if (DrawMaskMaterials is not null && DrawMaskMaterials.Contains(mat) is false) { continue; }

                        if (renderTextureHolders.ContainsKey(decalTargetTexture) is false)
                        {
                            var newTempRt = renderTextureHolders[decalTargetTexture] = TTRenderTexWithDistance.Create(_ttce4u, (decalTargetTexture.width, decalTargetTexture.height), DecalPadding);
                            newTempRt.Texture.Name = $"{decalTargetTexture.name}-CreateWriteDecalTexture-{newTempRt.Texture.Width}x{newTempRt.Texture.Hight}";
                            newTempRt.DistanceMap.Name = $"{decalTargetTexture.name}-CreateDistanceDecalTexture-{newTempRt.Texture.Width}x{newTempRt.Texture.Hight}";
                        }

                        var writeTarget = renderTextureHolders[decalTargetTexture];
                        using var transMappingHolder = TTTransMappingHolder.Create(_ttce4u, writeTarget.Texture.Size(), sourceTexture.Size(), DecalPadding);

                        var tTri = MemoryMarshal.Cast<Vector2, System.Numerics.Vector2>(toTrianglesStorageBuffers[r][s]!.Value.AsSpan());
                        var fTri = MemoryMarshal.Cast<Vector4, TTVector4>(fromTrianglesStorageBuffers[r][s]!.Value.AsSpan());

                        if (HighQualityPadding is false) _ttce4u.WriteMapping(transMappingHolder, tTri, fTri);
                        else _ttce4u.WriteMappingHighQuality(transMappingHolder, tTri, fTri);

                        if (depthBuffer is not null) { _ttce4u.DepthCulling(transMappingHolder, depthBuffer, UseDepthOrInvert!.Value); }

                        if (IsTextureStretch) _ttce4u.TransWarpModifierWithStretch(transMappingHolder);
                        else _ttce4u.TransWarpModifierWithNone(transMappingHolder);

                        _ttce4u.TransWrite(transMappingHolder, writeTarget, sourceTexture, _ttce4u.StandardComputeKey.DefaultSampler);
                    }
                }
                Profiler.EndSample();
            }
            finally
            {
                foreach (var ff in fromTrianglesStorageBuffers)
                    foreach (var f in ff)
                        if (f is not null)
                            f.Value.Dispose();
                foreach (var tt in toTrianglesStorageBuffers)
                    foreach (var t in tt)
                        if (t is not null)
                            t.Value.Dispose();
            }
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
