#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransCore;
using Unity.Collections;
using UnityEngine;

namespace net.rs64.TexTransTool
{
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public sealed class TileAtlasBreaker : TexTransBehavior, IDomainReferenceModifier
    {
        internal const string FoldoutName = "Other";
        internal const string ComponentName = "TTT " + nameof(TileAtlasBreaker);
        internal const string MenuPath = FoldoutName + "/" + ComponentName;
        internal override TexTransPhase PhaseDefine => TexTransPhase.UVDisassembly;

        public Material? TargetMaterial;
        public Material[] OriginalMaterials = new Material[4];// 2*2 じゃないタイルのことは一旦考えずに行きます。

        internal override IEnumerable<Renderer> TargetRenderers(IDomainReferenceViewer rendererTargeting)
        {
            return rendererTargeting.RendererFilterForMaterial(rendererTargeting.ObserveToGet(this, i => i.TargetMaterial));
        }
        internal override void Apply(IDomain domain)
        {
            var targetMaterial = domain.ObserveToGet(this, i => i.TargetMaterial);
            if (targetMaterial == null) { TTLog.Info("TileAtlasBreaker:info:TargetNotSet"); return; }

            var originalMaterials = domain.ObserveToGet(this, i => i.OriginalMaterials.ToArray(), (l, r) => l.SequenceEqual(r));
            if (originalMaterials.Length is not 4) { TTLog.Error("TileAtlasBreaker:error:InvalidSetting"); return; }

            var targetTextures = domain.RendererFilterForMaterial(targetMaterial).ToArray();
            if (targetTextures.Any() is false) { TTLog.Info("TileAtlasBreaker:info:TargetNotFound"); return; }

            foreach (var renderer in targetTextures)
            {
                ApplyImpl(domain, renderer, OriginalMaterials);
            }
        }
        void ApplyImpl(IDomain domain, Renderer renderer, Material[] originalMaterial)
        {
            var mesh = domain.GetMesh(renderer);
            if (mesh == null) { return; }
            if (mesh.HasVertexAttribute(UnityEngine.Rendering.VertexAttribute.TexCoord0) is false) { return; }
            var meshData = domain.GetMeshData(renderer);

            var triangles = meshData.TriangleVertexIndices[0];// 今はとりあえず SubMesh が存在しない前提で進みます。
            var uv = meshData.VertexUV;
            using var modifiedUVNativeArray = new NativeArray<Vector2>(uv, Allocator.TempJob);
            var modifiedUV = modifiedUVNativeArray.AsSpan();

            var verticalTileCount = 2;
            var horizontalTileCount = 2;
            var verticalTileSize = 1f / verticalTileCount;// 一応ここのあたりを可変にできるようにこういう構造にしておく、だが今は決め打ち
            var horizontalTileSize = 1f / horizontalTileCount;

            var spitedTriangles = new List<TriangleVertexIndices>[4] { new(), new(), new(), new() };

            foreach (var triangleIndex in triangles)
            {
                (int tileIndex, Vector2 originalUV) CucullateTiling(Vector2 uvVertex)// これ分けれないかな ... ? どういう分け方すればいいんやろね
                {
                    var tileScaleUVPositionX = uvVertex.x / horizontalTileSize;
                    var tileScaleUVPositionY = uvVertex.y / verticalTileSize;

                    var tileIndexX = (int)TTMath.Trunc(tileScaleUVPositionX);
                    var tileIndexY = (int)TTMath.Trunc(tileScaleUVPositionY);
                    var originalUV = new Vector2(TTMath.Frac(tileScaleUVPositionX), TTMath.Frac(tileScaleUVPositionY));

                    var tileIndex = tileIndexY * horizontalTileCount + tileIndexX;
                    return (tileIndex, originalUV);
                }

                //タイルにまたがるポリゴンはないものとします。
                var tileIndex = CucullateTiling(uv[triangleIndex[0]]).tileIndex;
                spitedTriangles[tileIndex].Add(triangleIndex);
                foreach (var vertexIndex in triangleIndex)
                {
                    modifiedUV[vertexIndex] = CucullateTiling(uv[vertexIndex]).originalUV;
                }
            }

            var mutableMesh = UnityEngine.Object.Instantiate(mesh);

            mutableMesh.subMeshCount = 0;
            mutableMesh.subMeshCount = 4;
            mutableMesh.SetUVs(0, modifiedUVNativeArray);
            for (var i = 0; spitedTriangles.Length > i; i += 1)
            {
                var modifiedTriangles = new int[spitedTriangles[i].Count * 3];
                var ti = 0;
                foreach (var triangleIndex in spitedTriangles[i])
                {
                    modifiedTriangles[ti] = triangleIndex[0];
                    modifiedTriangles[ti + 1] = triangleIndex[1];
                    modifiedTriangles[ti + 2] = triangleIndex[2];
                    ti += 3;
                }
                mutableMesh.SetTriangles(modifiedTriangles, i);
            }

            domain.SetMesh(renderer, mutableMesh);
            domain.SetMaterials(renderer, originalMaterial);
        }

        void IDomainReferenceModifier.RegisterDomainReference(IDomainReferenceViewer domainReferenceViewer, IDomainReferenceRegistry registry)
        {
            foreach (var renderer in TargetRenderers(domainReferenceViewer))
            {
                registry.RegisterAddMaterials(renderer, OriginalMaterials);
            }
        }
    }
}
