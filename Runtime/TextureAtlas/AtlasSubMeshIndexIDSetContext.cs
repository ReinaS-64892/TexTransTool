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

                var mats = targeting.GetMaterials(renderer);
                var mesh = targeting.GetMesh(renderer)!;
                var meshID = atlasMeshSourceContext.Normalized2MeshID[atlasMeshSourceContext.Origin2NormalizedMesh[mesh!]];

                var atlasSubSet = new AtlasSubMeshIndexID?[mats.Length];
                for (var subMeshIndex = 0; mats.Length > subMeshIndex; subMeshIndex += 1)
                {
                    var mat = mats[subMeshIndex];

                    if (mat == null) { continue; }
                    if (targetMaterials.Contains(mat) is false) { continue; }
                    if (mesh.GetSubMesh(Math.Clamp(subMeshIndex, 0, mesh.subMeshCount - 1)).indexCount is 0) { continue; }

                    var matID = materialGroupingContext.GetMaterialGroupID(mat);
                    Debug.Assert(matID is not -1);

                    var atSubData = new AtlasSubMeshIndexID(meshID, subMeshIndex, matID);

                    atlasSubSet[subMeshIndex] = atSubData;
                    AtlasSubMeshIndexIDHash.Add(atSubData);
                }
                AtlasSubSets.Add(atlasSubSet);
            }

            IdenticalSubSetRemove(AtlasSubSets);
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
