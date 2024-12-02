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
    internal interface IConvertSpace : IDisposable
    {
        void Input(MeshData meshData);//この MeshData の解放責任は受け取らない
        NativeArray<Vector2> OutPutUV();
    }
    internal interface ITrianglesFilter<SpaceConverter> : IDisposable
    {
        void SetSpace(SpaceConverter space);
        NativeArray<TriangleIndex> GetFilteredSubTriangle(int subMeshIndex);
    }
    internal class DecalContext<ConvertSpace, TrianglesFilter>
    where ConvertSpace : IConvertSpace
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

        public bool NotContainsKeyAutoGenerate { get; set; } = true;

        public DecalContext(ITexTransToolForUnity ttce4u, ConvertSpace convertSpace, TrianglesFilter trianglesFilter)
        {
            _ttce4u = ttce4u;
            _convertSpace = convertSpace;
            _trianglesFilter = trianglesFilter;
        }

        internal void WriteDecalTexture(Dictionary<Material, TTRenderTexWithDistance> renderTextures, Renderer targetRenderer, ITTRenderTexture sourceTexture)
        {
            if (renderTextures == null) { throw new ArgumentNullException(nameof(renderTextures)); }
            if (targetRenderer is not SkinnedMeshRenderer && targetRenderer is not MeshRenderer) { return; }
            if (targetRenderer.GetMesh() == null) { return; }

            Profiler.BeginSample("GetMeshData");
            var meshData = targetRenderer.Memo(MeshData.GetMeshData, i => i.Dispose());
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
                    var targetTexture = targetMat.GetTexture(TargetPropertyName);
                    if (targetTexture == null) { continue; }

                    if (!NotContainsKeyAutoGenerate && !renderTextures.ContainsKey(targetMat)) { continue; }

                    Profiler.BeginSample("GetFilteredSubTriangle");
                    var filteredTriangle = _trianglesFilter.GetFilteredSubTriangle(i);
                    Profiler.EndSample();
                    if (filteredTriangle.Length == 0) { continue; }

                    if (!renderTextures.ContainsKey(targetMat))
                    {
                        var newTempRt = renderTextures[targetMat] = TTRenderTexWithDistance.Create(_ttce4u, (targetTexture.width, targetTexture.height), DecalPadding);
                        newTempRt.Texture.Name = $"{targetTexture.name}-CreateWriteDecalTexture-{newTempRt.Texture.Width}x{newTempRt.Texture.Hight}";
                        newTempRt.DistanceMap.Name = $"{targetTexture.name}-CreateDistanceDecalTexture-{newTempRt.Texture.Width}x{newTempRt.Texture.Hight}";
                    }

                    var sUV = _convertSpace.OutPutUV();

                    Profiler.BeginSample("TransTexture");
                    using var transMappingHolder = TTTransMappingHolder.Create(_ttce4u, renderTextures[targetMat].Texture.Size(), sourceTexture.Size(), DecalPadding);

                    Profiler.BeginSample("PackingTriangles");
                    Profiler.BeginSample("from");
                    using var packedFromTriangle = TransTexture.PackingTrianglesForFrom(filteredTriangle, sUV, Allocator.Temp);
                    Profiler.EndSample();
                    Profiler.BeginSample("to");
                    using var packedToTriangle = TransTexture.PackingTrianglesForTo(filteredTriangle, tUV, Allocator.Temp);
                    Profiler.EndSample();
                    Profiler.EndSample();

                    Profiler.BeginSample("TransTexture");
                    var fromTriSpan = MemoryMarshal.Cast<Vector4, System.Numerics.Vector4>(packedFromTriangle);
                    var toTriSpan = MemoryMarshal.Cast<Vector2, System.Numerics.Vector2>(packedToTriangle);
                    if (HighQualityPadding is false) _ttce4u.WriteMapping(transMappingHolder, toTriSpan, fromTriSpan);
                    else _ttce4u.WriteMappingHighQuality(transMappingHolder, toTriSpan, fromTriSpan);

                    if (IsTextureStretch) _ttce4u.TransWarpModifierWithStretch(transMappingHolder);
                    else _ttce4u.TransWarpModifierWithNone(transMappingHolder);

                    _ttce4u.TransWrite(transMappingHolder, renderTextures[targetMat], sourceTexture, _ttce4u.StandardComputeKey.DefaultSampler);
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

        internal void GenerateKey(Dictionary<Material, TTRenderTexWithDistance> writeable, IEnumerable<Material> targetMat)
        {
            foreach (var mat in targetMat)
            {
                if (mat == null) { continue; }
                if (writeable.ContainsKey(mat)) { continue; }
                if (!mat.HasProperty(TargetPropertyName)) { continue; }
                var targetTexture = mat.GetTexture(TargetPropertyName);
                if (targetTexture == null) { continue; }

                var rt = writeable[mat] = TTRenderTexWithDistance.Create(_ttce4u, (targetTexture.width, targetTexture.height), DecalPadding);
                rt.Texture.Name = $"{targetTexture.name}-CreateGenerateKey-TempRt-{rt.Texture.Width}x{rt.Texture.Hight}";
                rt.DistanceMap.Name = $"{targetTexture.name}-CreateDistanceDecalTexture-{rt.Texture.Width}x{rt.Texture.Hight}";
            }
        }
    }

    public enum PolygonCulling
    {
        Vertex,
        Edge,
        EdgeAndCenterRay,
    }
}
