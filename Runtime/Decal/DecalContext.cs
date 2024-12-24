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

namespace net.rs64.TexTransTool.Decal
{
    internal interface ISpaceConverter : IDisposable
    {
        void Input(MeshData meshData);//この MeshData の解放責任は受け取らない
        NativeArray<Vector2> UVOut();
        bool AllowDepth { get; }
        NativeArray<Vector3> UVOutWithDepth();
    }
    internal interface ITrianglesFilter<TSpace> : IDisposable
    {
        void SetSpace(TSpace space);
        NativeArray<TriangleIndex> GetFilteredSubTriangle(int subMeshIndex);
    }
    internal class DecalContext<ConvertSpace, TrianglesFilter>
    where ConvertSpace : ISpaceConverter
    where TrianglesFilter : ITrianglesFilter<ConvertSpace>
    {
        ITexTransToolForUnity _ttce4u;
        ConvertSpace _convertSpace;
        TrianglesFilter _trianglesFilter;


        public string TargetPropertyName { get; set; } = "_MainTex";
        public bool IsTextureStretch { get; set; } = false;
        public float DecalPadding { get; set; } = 5f;
        public bool HighQualityPadding { get; set; } = false;
        public bool? UseDepthOrInvert { get; set; } = null;

        public HashSet<Material> DrawMaskMaterials = null;

        public DecalContext(ITexTransToolForUnity ttce4u, ConvertSpace convertSpace, TrianglesFilter trianglesFilter)
        {
            _ttce4u = ttce4u;
            _convertSpace = convertSpace;
            _trianglesFilter = trianglesFilter;
        }

        internal void WriteDecalTexture<KeyTexture>(Dictionary<KeyTexture, TTRenderTexWithDistance> renderTextures, Renderer targetRenderer, ITTRenderTexture sourceTexture)
        where KeyTexture : Texture
        {
            if (renderTextures == null) { throw new ArgumentNullException(nameof(renderTextures)); }
            if (targetRenderer is not (SkinnedMeshRenderer or MeshRenderer)) { return; }
            if (targetRenderer.GetMesh() == null) { return; }

            Profiler.BeginSample("GetMeshData");
            var meshData = targetRenderer.GetToMemorizedMeshData();
            Profiler.EndSample();

            Profiler.BeginSample("GetUVs");
            var tUV = meshData.VertexUV;
            Profiler.EndSample();
            try
            {
                Profiler.BeginSample("convertSpace.Input");
                _convertSpace.Input(meshData);
                Profiler.EndSample();

                _trianglesFilter.SetSpace(_convertSpace);

                var materials = targetRenderer.sharedMaterials;
                var validSlotCount = Math.Min(materials.Length, meshData.Triangles.Length);
                for (int i = 0; i < validSlotCount; i++)
                {
                    var targetMat = materials[i];

                    if (targetMat == null) { continue; }
                    if (!targetMat.HasProperty(TargetPropertyName)) { continue; };
                    var targetTexture = targetMat.GetTexture(TargetPropertyName) as KeyTexture;
                    if (targetTexture == null) { continue; }

                    if (DrawMaskMaterials is not null && DrawMaskMaterials.Contains(targetMat) is false) { continue; }

                    Profiler.BeginSample("GetFilteredSubTriangle");
                    var filteredTriangle = _trianglesFilter.GetFilteredSubTriangle(i);
                    Profiler.EndSample();
                    if (filteredTriangle.Length == 0) { continue; }

                    if (!renderTextures.ContainsKey(targetTexture))
                    {
                        var newTempRt = renderTextures[targetTexture] = TTRenderTexWithDistance.Create(_ttce4u, (targetTexture.width, targetTexture.height), DecalPadding);
                        newTempRt.Texture.Name = $"{targetTexture.name}-CreateWriteDecalTexture-{newTempRt.Texture.Width}x{newTempRt.Texture.Hight}";
                        newTempRt.DistanceMap.Name = $"{targetTexture.name}-CreateDistanceDecalTexture-{newTempRt.Texture.Width}x{newTempRt.Texture.Hight}";
                    }
                    var target = renderTextures[targetTexture];

                    Profiler.BeginSample("TransTexture");

                    using var transMappingHolder = TTTransMappingHolder.Create(_ttce4u, target.Texture.Size(), sourceTexture.Size(), DecalPadding);

                    if (UseDepthOrInvert.HasValue && _convertSpace.AllowDepth)
                    {
                        var sUV = _convertSpace.UVOutWithDepth();

                        Profiler.BeginSample("PackingTriangles");
                        using var packedFromTriangle = TransTexture.PackingTrianglesForFrom(filteredTriangle, sUV, Allocator.Temp);
                        using var packedToTriangle = TransTexture.PackingTrianglesForTo(filteredTriangle, tUV, Allocator.Temp);
                        Profiler.EndSample();

                        Profiler.BeginSample("WriteMapping");
                        var fromTriSpan = MemoryMarshal.Cast<Vector4, TTVector4>(packedFromTriangle);
                        var toTriSpan = MemoryMarshal.Cast<Vector2, System.Numerics.Vector2>(packedToTriangle);
                        using var fromPolygonStorage = _ttce4u.UploadStorageBuffer(fromTriSpan);
                        if (HighQualityPadding is false) _ttce4u.WriteMapping(transMappingHolder, toTriSpan, fromPolygonStorage);
                        else _ttce4u.WriteMappingHighQuality(transMappingHolder, toTriSpan, fromTriSpan);
                        Profiler.EndSample();

                        Profiler.BeginSample("TransCulling");
                        if (IsTextureStretch) _ttce4u.TransWarpModifierWithStretch(transMappingHolder);
                        else _ttce4u.TransWarpModifierWithNone(transMappingHolder);
                        Profiler.EndSample();

                        Profiler.BeginSample("WriteDepth");
                        try
                        {
                            var polygonCount = (uint)(fromTriSpan.Length / 3);
                            _ttce4u.DepthCulling(transMappingHolder, fromPolygonStorage, polygonCount, UseDepthOrInvert.Value, 0.0001f);
                        }
                        finally { fromPolygonStorage.Dispose(); }
                        Profiler.EndSample();
                    }
                    else
                    {
                        var sUV = _convertSpace.UVOut();

                        Profiler.BeginSample("PackingTriangles");
                        using var packedFromTriangle = TransTexture.PackingTrianglesForFrom(filteredTriangle, sUV, Allocator.Temp);
                        using var packedToTriangle = TransTexture.PackingTrianglesForTo(filteredTriangle, tUV, Allocator.Temp);
                        Profiler.EndSample();

                        Profiler.BeginSample("WriteMapping");
                        var fromTriSpan = MemoryMarshal.Cast<Vector4, TTVector4>(packedFromTriangle);
                        var toTriSpan = MemoryMarshal.Cast<Vector2, System.Numerics.Vector2>(packedToTriangle);
                        if (HighQualityPadding is false) _ttce4u.WriteMapping(transMappingHolder, toTriSpan, fromTriSpan);
                        else _ttce4u.WriteMappingHighQuality(transMappingHolder, toTriSpan, fromTriSpan);
                        Profiler.EndSample();
                        Profiler.BeginSample("TransCulling");
                        if (IsTextureStretch) _ttce4u.TransWarpModifierWithStretch(transMappingHolder);
                        else _ttce4u.TransWarpModifierWithNone(transMappingHolder);
                        Profiler.EndSample();
                    }

                    Profiler.BeginSample("TransWrite");
                    _ttce4u.TransWrite(transMappingHolder, target, sourceTexture, _ttce4u.StandardComputeKey.DefaultSampler);
                    Profiler.EndSample();

                    Profiler.EndSample();
                }
            }
            finally
            {
                _trianglesFilter.Dispose();
                _convertSpace.Dispose();
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
