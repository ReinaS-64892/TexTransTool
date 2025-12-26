#nullable enable
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace net.rs64.TexTransTool.TextureAtlas
{
    internal class AtlasSubMeshIndexIDSetContext
    {
        public HashSet<AtlasSubMeshIndexID> AtlasSubMeshIndexIDHash;
        public List<AtlasSubMeshIndexID?[]> AtlasSubSets;
        public AtlasSubMeshIndexIDSetContext(
            IDomainReferenceViewer targeting
            , Renderer[] targetRenderers
            , HashSet<Material> targetMaterials
            , AtlasMeshSourceContext atlasMeshSourceContext
            , MaterialGroupingContext materialGroupingContext
            )
        {
            AtlasSubMeshIndexIDHash = new();
            AtlasSubSets = new();// レンダラー - マテリアルスロット(SubMesh) - nullable はターゲットではないマテリアルの分
            for (var rendererIndex = 0; targetRenderers.Length > rendererIndex; rendererIndex += 1)
            {
                var renderer = targetRenderers[rendererIndex];
                var atlasSubSet = CreateSubSet(targeting, atlasMeshSourceContext, materialGroupingContext, renderer);
                AtlasSubMeshIndexIDHash.UnionWith(atlasSubSet.Where(v => v.HasValue).Select(v => v!.Value));
                AtlasSubSets.Add(atlasSubSet);
            }

            IdenticalSubSetRemove(AtlasSubSets);
        }

        private static AtlasSubMeshIndexID?[] CreateSubSet(IDomainReferenceViewer targeting, AtlasMeshSourceContext atlasMeshSourceContext, MaterialGroupingContext materialGroupingContext, Renderer renderer)
        {
            var mats = targeting.GetMaterials(renderer);
            var mesh = targeting.GetMesh(renderer)!;
            var meshID = atlasMeshSourceContext.Normalized2MeshID[atlasMeshSourceContext.Origin2NormalizedMesh[mesh!]];

            var atlasSubSet = new AtlasSubMeshIndexID?[mats.Length];
            for (var subMeshIndex = 0; mats.Length > subMeshIndex; subMeshIndex += 1)
            {
                var mat = mats[subMeshIndex];

                if (mat == null) { continue; }
                var matID = materialGroupingContext.GetMaterialGroupID(mat);
                if (matID is -1) { continue; }
                if (mesh.GetSubMesh(Math.Clamp(subMeshIndex, 0, mesh.subMeshCount - 1)).indexCount is 0) { continue; }

                atlasSubSet[subMeshIndex] = new AtlasSubMeshIndexID(meshID, subMeshIndex, matID);
            }

            return atlasSubSet;
        }
        internal static int GetIdenticalSubSetID(IDomainReferenceViewer domain, AtlasContext atlasContext, Renderer renderer)
        {
            var mesh = domain.GetMesh(renderer);
            if (mesh == null) { return -1; }
            if (atlasContext.NormalizedMeshCtx.Origin2NormalizedMesh.ContainsKey(mesh) is false) { return -1; }

            var subSetID = CreateSubSet(domain, atlasContext.NormalizedMeshCtx, atlasContext.MaterialGroupingCtx, renderer);
            int GetIdenticalSubSet(List<AtlasSubMeshIndexID?[]> atlasSubSetAll, AtlasSubMeshIndexID?[] findSource)
            {
                return atlasSubSetAll.FindIndex(subSet =>
                {
                    if (subSet.Length == findSource.Length && subSet.SequenceEqual(findSource)) { return true; }
                    if (SubPartEqual(subSet, findSource))
                    {
                        // マテリアルスロット数が少ないレンダラーに subMesh が長いメッシュを割り当てる分には問題がない。
                        // だが逆は壊れる。
                        var subSetIsMinimum = subSet.Length < findSource.Length;
                        return subSetIsMinimum is false;
                    }
                    return false;
                });
            }
            return GetIdenticalSubSet(atlasContext.AtlasSubMeshIndexSetCtx.AtlasSubSets, subSetID);
        }




        /*
        AtlasSubSetは、AtlasSubDataの一つの塊みたいな扱いで、 出力されるメッシュに相当する。

        SubMeshが多い分には問題がないから、
        同一メッシュを参照し、マテリアルがそれぞれ

        ABC
        ABCDE

        のようなものの場合同一のものにすべきなため、同一サブセット扱いを行う。

        */
        static void IdenticalSubSetRemove(List<AtlasSubMeshIndexID?[]> atlasSubSets)
        {
            while (true)
            {
                var identicalSubset = FindIdentical(atlasSubSets);
                if (identicalSubset is null) { return; }
                atlasSubSets.RemoveAt(identicalSubset.Value);
            }
        }
        static int? FindIdentical(List<AtlasSubMeshIndexID?[]> atlasSubSets)
        {
            var assCount = atlasSubSets.Count;
            for (var i = 0; assCount > i; i += 1)
            {
                for (var i2 = i + 1; assCount > i2; i2 += 1)
                {
                    var subSet = atlasSubSets[i];
                    var subSet2 = atlasSubSets[i2];

                    if (subSet.Length == subSet2.Length && subSet.SequenceEqual(subSet2)) { return i; }
                    if (SubPartEqual(subSet, subSet2))
                    {
                        if (subSet.Length > subSet2.Length) { return i2; }
                        else { return i; }
                    }
                }
            }
            return null;
        }
        internal static bool SubPartEqual(AtlasSubMeshIndexID?[] fSubSet, AtlasSubMeshIndexID?[] tSubSet)
        {
            var minCount = Math.Min(fSubSet.Length, tSubSet.Length);
            for (var i = 0; minCount > i; i += 1)
            {
                if (fSubSet[i].HasValue != fSubSet[i].HasValue) { return false; }
                if (fSubSet[i] != tSubSet[i]) { return false; }
            }
            return true;
        }

    }

}
