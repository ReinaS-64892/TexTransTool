#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;
using net.rs64.TexTransCore.MultiLayerImageCanvas;
using System.Runtime.CompilerServices;
using net.rs64.TexTransTool.IslandSelector;
using System.Linq;
using net.rs64.TexTransTool.Utils;
using net.rs64.TexTransTool.Decal;
using net.rs64.TexTransCore;
using System.Runtime.InteropServices;
using net.rs64.TexTransCore.TransTexture;

namespace net.rs64.TexTransTool.MultiLayerImage
{
    [Serializable]
    public sealed class IslandSelectAsLayerMask : ILayerMask
    {
        public AbstractIslandSelector? IslandSelector;
        public float MaskPadding = 5f;
        AlphaMask<ITexTransToolForUnity> ILayerMask.GetAlphaMaskObject(GenerateLayerObjectContext ctx, UnityEngine.Object thisObj, Func<UnityEngine.Object, ILayerMask?> getThisToLayerMask)
        {
            var domain = ctx.Domain;
            var engine = ctx.Engine;

            if (ctx.TargetContainedMaterials is null) { return new NoMask<ITexTransToolForUnity>(); }

            var thisLayerMask = domain.LookAtGet(thisObj, o => getThisToLayerMask(o) as IslandSelectAsLayerMask);
            if (thisLayerMask is null) { return new NoMask<ITexTransToolForUnity>(); }

            var islandSelector = domain.LookAtGet(thisObj, o => (getThisToLayerMask(o) as IslandSelectAsLayerMask)!.IslandSelector);
            if (islandSelector == null) { return new NoMask<ITexTransToolForUnity>(); }
            islandSelector.LookAtCalling(domain);

            var padding = domain.LookAtGet(thisObj, o => (getThisToLayerMask(o) as IslandSelectAsLayerMask)!.MaskPadding);

            var mask = engine.CreateRenderTexture(ctx.CanvasSize.x, ctx.CanvasSize.y);

            var decalRenderTarget = domain.RendererFilterForMaterialFromDomains(ctx.TargetContainedMaterials);
            domain.LookAt(decalRenderTarget);

            var meshData = decalRenderTarget
                .Where(r => r is SkinnedMeshRenderer or MeshRenderer)
                .Where(r => r.GetMesh() != null)
                .Select(r => ctx.Domain.GetMeshData(r))
                .ToArray();

            var islandsArray = IslandSelectToPPFilter.IslandsArrayFromMeshData(meshData);
            var bitArray = islandSelector.IslandSelect(new(islandsArray.flattenIslands, islandsArray.flattenIslandDescription, domain));
            using var islandSelectedTrianglesHolder = IslandSelectToPPFilter.IslandSelectToTriangleIndex(islandsArray.allMeshIslands, islandsArray.islandToIndex, bitArray);
            var islandSelectedTriangles = islandSelectedTrianglesHolder.Ref();

            using var writable = TTRenderTexWithPaddingDistance.CreateFrom(engine, mask, padding);
            for (var ri = 0; meshData.Length > ri; ri += 1)
            {
                var md = meshData[ri];
                ITTStorageBuffer? uvVertexBuffer = null;
                try
                {
                    for (var si = 0; md.SubMeshCount > si; si += 1)
                    {
                        var triangles = islandSelectedTriangles[ri][si];
                        if (triangles.Length == 0) { continue; }

                        uvVertexBuffer ??= engine.UploadStorageBuffer<Vector2>(md.VertexUV);
                        using var trianglesBuf = engine.UploadStorageBuffer<int>(MemoryMarshal.Cast<TriangleIndex, int>(triangles));

                        PolygonMask.Write(engine, writable, uvVertexBuffer, trianglesBuf, triangles.Length);
                    }
                }
                finally { uvVertexBuffer?.Dispose(); }
            }

            return new RenderTextureOnlyToMask<ITexTransToolForUnity>(mask);
        }
    }
}
