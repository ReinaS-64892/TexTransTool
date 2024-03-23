using System.Collections.Generic;
using UnityEngine;
using net.rs64.TexTransCore.Island;
using System;
using net.rs64.TexTransCore.Decal;
using System.Linq;
using net.rs64.TexTransCore.TransTextureCore.Utils;
using System.Collections;
using net.rs64.TexTransTool.TextureAtlas.AAOCode;
using net.rs64.TexTransTool.Utils;

namespace net.rs64.TexTransTool.TextureAtlas
{
    internal class AtlasContext : IDisposable
    {
        public Mesh[] Meshes;//ノーマライズされていない
        public Dictionary<Mesh, Mesh> NormalizeMeshes;//ノーマライズされていないメッシュとされているメッシュと対応する
        public Dictionary<Mesh, MeshData> MeshDataDict;//適当なレンダラーのノーマライズされたメッシュと対応する

        public HashSet<Material>[] Materials;
        public HashSet<AtlasSubData> AtlasSubAll;
        public List<AtlasSubData?[]> AtlasSubSets;

        public Dictionary<AtlasSubData, List<Island>> IslandDict;
        public Island[] Islands;
        public AtlasSubData[] IslandSubData;

        public struct AtlasSubData
        {
            public int MeshID;
            public int SubMeshIndex;
            public int MaterialID;
            public AtlasSubData(int meshID, int subMeshIndex, int materialID) : this()
            {
                MeshID = meshID;
                SubMeshIndex = subMeshIndex;
                MaterialID = materialID;
            }
            public override bool Equals(object obj) { return obj is AtlasSubData other && MeshID == other.MeshID && SubMeshIndex == other.SubMeshIndex && MaterialID == other.MaterialID; }
            public override int GetHashCode() { return HashCode.Combine(MeshID, SubMeshIndex, MaterialID); }
            public static bool operator ==(AtlasSubData l, AtlasSubData r)
            {
                if (l.MeshID != r.MeshID) { return false; }
                if (l.SubMeshIndex != r.SubMeshIndex) { return false; }
                if (l.MaterialID != r.MaterialID) { return false; }
                return true;
            }
            public static bool operator !=(AtlasSubData l, AtlasSubData r)
            {
                if (l.MeshID != r.MeshID) { return true; }
                if (l.SubMeshIndex != r.SubMeshIndex) { return true; }
                if (l.MaterialID != r.MaterialID) { return true; }
                return false;
            }

        }
        public AtlasContext(List<Material> TargetMaterials, List<Renderer> inputRenderers)
        {
            var materialHash = TargetMaterials.ToHashSet();
            Materials = materialHash.Select(m => new HashSet<Material>() { m }).ToArray();//今後マテリアルが違っても同一とみる機会が発生するから、マテリアルのグルーピングの仕組みを作る。

            var TargetRenderers = inputRenderers.Where(r => r.sharedMaterials.Any(m => materialHash.Contains(m))).ToArray();
            var normalizedMesh = SubVertNormalize(TargetRenderers);

            var m2md = new Dictionary<Mesh, MeshData>();
            foreach (var mkr in TargetRenderers.GroupBy(r => r.GetMesh()))
            {
                var nmMesh = normalizedMesh[mkr.Key];
                var renderer = mkr.First(r => r.sharedMaterials.Length == nmMesh.subMeshCount);

                m2md[nmMesh] = new MeshData(renderer, nmMesh, MeshData.GetMatrix(renderer));
            }

            NormalizeMeshes = normalizedMesh;
            MeshDataDict = m2md;
            Meshes = normalizedMesh.Select(i => i.Key).ToArray();

            AtlasSubAll = new();
            var atlasSubSets = new List<AtlasSubData?[]>();
            for (var ri = 0; TargetRenderers.Length > ri; ri += 1)
            {
                var renderer = TargetRenderers[ri];
                var mats = renderer.sharedMaterials;
                var mesh = renderer.GetMesh();
                var meshID = Array.IndexOf(Meshes, mesh);
                var atlasSubSet = new AtlasSubData?[mats.Length];
                for (var si = 0; mats.Length > si; si += 1)
                {
                    if (!materialHash.Contains(mats[si])) { continue; }
                    var matID = Array.FindIndex(Materials, i => i.Contains(mats[si]));
                    var atSubData = atlasSubSet[si] = new(meshID, si, matID);
                    AtlasSubAll.Add(atSubData.Value);
                }
                atlasSubSets.Add(atlasSubSet);
            }

            IdenticalSubSetRemove(atlasSubSets);
            AtlasSubSets = atlasSubSets;

            var islandDict = new Dictionary<AtlasSubData, List<Island>>();
            foreach (var atSub in AtlasSubAll)
            {
                var md = MeshDataDict[normalizedMesh[Meshes[atSub.MeshID]]];

                var triangle = normalizedMesh[Meshes[atSub.MeshID]].GetSubTriangleIndex(atSub.SubMeshIndex);
                // var triangle = md.TriangleIndex[atSub.SubMeshIndex].AsList(); //なぜかこっちだと一部のモデルで正しくUVtoIslandができない...なぜ？

                var island = IslandUtility.UVtoIsland(triangle, md.VertexUV.AsList());
                islandDict[atSub] = island;
            }


            //Cross SubMesh Island Marge
            foreach (var atSubGroup in AtlasSubAll.GroupBy(i => (i.MeshID, i.MaterialID)))
            {
                var atSubCrossTarget = atSubGroup.ToArray();
                Array.Sort(atSubCrossTarget, (l, r) => l.SubMeshIndex - r.SubMeshIndex);

                var meshData = MeshDataDict[normalizedMesh[Meshes[atSubGroup.Key.MeshID]]];
                var uv = meshData.VertexUV;
                var uvToIndex = new Dictionary<Vector2, int>();
                var uvIndex = 0;
                foreach (var uvVert in uv)
                {
                    if (!uvToIndex.ContainsKey(uvVert))
                    {
                        uvToIndex[uvVert] = uvIndex;
                        uvIndex += 1;
                    }
                }
                var uvIndexCount = uvIndex + 1;

                var usedUVVertIndexUsed = new BitArray[atSubCrossTarget.Length];
                for (var i = 0; atSubCrossTarget.Length > i; i += 1)
                {
                    var atSub = atSubCrossTarget[i];
                    var bitArray = new BitArray(uvIndexCount);
                    foreach (var tri in meshData.TriangleIndex[atSub.SubMeshIndex])
                    {
                        for (var ti = 0; 3 > ti; ti += 1)
                        {
                            bitArray[uvToIndex[uv[tri[ti]]]] = true;
                        }
                    }
                    usedUVVertIndexUsed[i] = bitArray;
                }

                var margeAt = new Dictionary<int, int>();

                for (var fi = 0; atSubCrossTarget.Length > fi; fi += 1)
                {
                    for (var ti = fi + 1; atSubCrossTarget.Length > ti; ti += 1)
                    {
                        for (var vi = 0; usedUVVertIndexUsed[fi].Length > vi; vi += 1)
                        {
                            if (usedUVVertIndexUsed[fi][vi] && usedUVVertIndexUsed[ti][vi]) { margeAt[fi] = ti; break; }
                        }
                    }

                }

                foreach (var mki in margeAt)
                {
                    var fromAtSub = atSubCrossTarget[mki.Key];
                    var toAtSub = atSubCrossTarget[mki.Value];

                    (Island, BitArray) islandToUseBitArray(Island i)
                    {
                        var islandBitArray = new BitArray(uvIndexCount);
                        foreach (var tri in i.triangles)
                        {
                            for (var ti = 0; 3 > ti; ti += 1)
                            {
                                islandBitArray[uvToIndex[uv[tri[ti]]]] = true;
                            }
                        }
                        return (i, islandBitArray);
                    }

                    var islandToUseIndexBits = islandDict[fromAtSub].Concat(islandDict[toAtSub]).Select(islandToUseBitArray).ToDictionary(i => i.Item1, i => i.Item2);


                    foreach (var fIsland in islandDict[fromAtSub])
                    {
                        var removeAt = new HashSet<Island>();
                        foreach (var tIsland in islandDict[toAtSub])
                        {
                            var fBit = islandToUseIndexBits[fIsland];
                            var tBit = islandToUseIndexBits[tIsland];

                            var needMerge = false;
                            for (var vi = 0; fBit.Length > vi; vi += 1)
                            {
                                if (fBit[vi] && tBit[vi]) { needMerge = true; break; }
                            }
                            if (!needMerge) { continue; }

                            var min = Vector2.Min(fIsland.Pivot, tIsland.Pivot);
                            var max = Vector2.Max(fIsland.GetMaxPos, tIsland.GetMaxPos);
                            fIsland.Pivot = min;
                            fIsland.Size = max - fIsland.Pivot;

                            fIsland.triangles.AddRange(tIsland.triangles);
                            removeAt.Add(tIsland);
                        }
                        islandDict[toAtSub].RemoveAll(removeAt.Contains);
                    }

                }
            }
            IslandDict = islandDict;

            var atSubLinkList = new LinkedList<AtlasSubData>();
            var IslandLinkList = new LinkedList<Island>();

            foreach (var atKv in IslandDict)
            {
                var atlasSubData = atKv.Key;
                var islands = atKv.Value;

                var count = islands.Count;
                for (var ii = 0; count > ii; ii += 1)
                {
                    atSubLinkList.AddLast(atlasSubData);
                    IslandLinkList.AddLast(islands[ii]);
                }
            }

            Islands = IslandLinkList.ToArray();
            IslandSubData = atSubLinkList.ToArray();

            static void IdenticalSubSetRemove(List<AtlasSubData?[]> atlasSubSets)
            {
                while (true)
                {
                    var margeAt = Find(atlasSubSets);
                    if (margeAt is null) { return; }
                    var val = margeAt.Value;
                    atlasSubSets.RemoveAt(val.Item2);
                }

                static (int, int)? Find(List<AtlasSubData?[]> atlasSubSets)
                {
                    var assCount = atlasSubSets.Count;
                    for (var fi = 0; assCount > fi; fi += 1)
                    {
                        for (var ti = fi + 1; assCount > ti; ti += 1)
                        {
                            var fSubSet = atlasSubSets[fi];
                            var tSubSet = atlasSubSets[ti];

                            var first = fSubSet[0];
                            var tFirst = tSubSet[0];

                            if (first is null || tFirst is null) { continue; }
                            if (first.Value.MeshID != tFirst.Value.MeshID) { continue; }

                            if (fSubSet.SequenceEqual(tSubSet)) { return (fi, ti); }

                            if (SubPartEqual(fSubSet, tSubSet))
                            {
                                if (fSubSet.Length > tSubSet.Length) { return (fi, ti); }
                                else { return (ti, fi); }
                            }

                        }
                    }

                    return null;

                    static bool SubPartEqual(AtlasSubData?[] fSubSet, AtlasSubData?[] tSubSet)
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
        }

        private Dictionary<Mesh, Mesh> SubVertNormalize(Renderer[] targetRenderers)
        {
            var normalizedMesh = new Dictionary<Mesh, Mesh>();
            foreach (var MkR in targetRenderers.GroupBy(i => i.GetMesh()))
            {
                var mesh = MkR.Key;
                var maxSlot = 0; foreach (var r in MkR) { maxSlot = Mathf.Max(r.sharedMaterials.Length, maxSlot); }

                var isOverSubMesh = maxSlot != mesh.subMeshCount;
                var isCrossSubMesh = IsCrossSubMesh(mesh);

                if (!isOverSubMesh && !isCrossSubMesh) { normalizedMesh[mesh] = UnityEngine.Object.Instantiate(mesh); continue; }

                normalizedMesh[mesh] = NormalizedMesh(mesh, maxSlot);
            }
            return normalizedMesh;

            bool IsCrossSubMesh(Mesh mesh)
            {
                var vertSubMeshBit = new BitArray[mesh.subMeshCount];
                var triList = new List<int>();
                for (var subMeshIndex = 0; mesh.subMeshCount > subMeshIndex; subMeshIndex += 1)
                {
                    triList.Clear();
                    mesh.GetTriangles(triList, subMeshIndex);
                    var vertBit = new BitArray(mesh.vertexCount);
                    foreach (var ti in triList) { vertBit[ti] = true; }
                    vertSubMeshBit[subMeshIndex] = vertBit;
                }

                for (var fi = 0; vertSubMeshBit.Length > fi; fi += 1)
                {
                    for (var ti = fi + 1; vertSubMeshBit.Length > ti; ti += 1)
                    {
                        for (var vi = 0; vertSubMeshBit[fi].Length > vi; vi += 1)
                        {
                            if (vertSubMeshBit[fi][vi] && vertSubMeshBit[ti][vi]) { return true; }
                        }
                    }
                }

                return false;
            }

            Mesh NormalizedMesh(Mesh mesh, int expandSlot)
            {
                var vertex = MeshInfoUtility.ReadVertex(mesh, out var meshDesc);
                var vertHash = new HashSet<Vertex>();


                var subMeshViArray = new int[expandSlot][];
                for (var si = 0; subMeshViArray.Length > si; si += 1) { subMeshViArray[si] = mesh.GetTriangles(si % mesh.subMeshCount); }

                var subMeshOfVertex = new Vertex[expandSlot][];
                for (var si = 0; subMeshViArray.Length > si; si += 1)
                {
                    var subMeshVi = subMeshViArray[si];
                    var subMeshVert = subMeshOfVertex[si] = new Vertex[subMeshVi.Length];
                    for (var vi = 0; subMeshVi.Length > vi; vi += 1)
                    {
                        subMeshVert[vi] = vertex[subMeshVi[vi]];
                    }

                }

                var useVert = new HashSet<Vertex>();
                for (var si = 0; subMeshOfVertex.Length > si; si += 1)
                {
                    var subMeshVert = subMeshOfVertex[si];

                    var subCrossUsed = new HashSet<Vertex>(subMeshVert);
                    subCrossUsed.IntersectWith(useVert);

                    useVert.UnionWith(subMeshVert);

                    if (!subCrossUsed.Any()) { continue; }

                    var replaceDict = subCrossUsed.ToDictionary(i => i, i => i.Clone());

                    for (var vi = 0; subMeshVert.Length > vi; vi += 1)
                    {
                        if (replaceDict.TryGetValue(subMeshVert[vi], out var rv)) { subMeshVert[vi] = rv; }
                    }

                    vertex.AddRange(replaceDict.Values);
                }

                var modifiedMesh = UnityEngine.Object.Instantiate(mesh);
                MeshInfoUtility.WriteVertex(modifiedMesh, meshDesc, vertex);

                modifiedMesh.subMeshCount = subMeshOfVertex.Length;
                for (var si = 0; subMeshOfVertex.Length > si; si += 1) { modifiedMesh.SetTriangles(subMeshOfVertex[si].Select(v => vertex.IndexOf(v)).ToArray(), si); }

                //念のために複製
                var moreCloned = UnityEngine.Object.Instantiate(modifiedMesh);
                UnityEngine.Object.DestroyImmediate(modifiedMesh);
                return moreCloned;
            }
        }


        public void Dispose()
        {
            foreach (var md in MeshDataDict) { md.Value.Dispose(); }
        }


    }
}
