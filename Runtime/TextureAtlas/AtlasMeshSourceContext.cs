#nullable enable
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Collections;
using net.rs64.TexTransTool.TextureAtlas.AAOCode;
using net.rs64.TexTransTool.Decal;
using System;
using net.rs64.TexTransCore;
using net.rs64.TexTransTool.Utils;

namespace net.rs64.TexTransTool.TextureAtlas
{
    internal class AtlasMeshSourceContext : IDisposable
    {
        public Dictionary<Mesh, Mesh> Origin2NormalizedMesh;
        public Dictionary<Mesh, Mesh> Normalized2OriginMesh;
        public Dictionary<Mesh, int> Normalized2MeshID;
        public Dictionary<int, Mesh> MeshID2Normalized;
        public Dictionary<Mesh, MeshData> Normalized2MeshData;

        public AtlasMeshSourceContext(IDomainReferenceViewer targeting, Renderer[] targetRenderers, UVChannel atlasTargetUVChannel)
        {
            using var pf = new PFScope("ctr");
            var meshGroupedRenderers = targetRenderers.GroupBy(r => targeting.GetMesh(r)).Cast<IGrouping<Mesh, Renderer>>().ToArray();
            var meshArray = meshGroupedRenderers.Select(k => k.Key).ToArray();
            Origin2NormalizedMesh = CrossSubMeshNormalizingAndExpandMaterialSlot(targeting, meshGroupedRenderers);
            Normalized2OriginMesh = Origin2NormalizedMesh.ToDictionary(kv => kv.Value, kv => kv.Key);
            Normalized2MeshID = Origin2NormalizedMesh.ToDictionary(kv => kv.Value, kv => Array.IndexOf(meshArray, kv.Key));
            MeshID2Normalized = Normalized2MeshID.ToDictionary(kv => kv.Value, kv => kv.Key);

            pf.Split("generate mesh data");
            Normalized2MeshData = new Dictionary<Mesh, MeshData>();
            foreach (var mkr in meshGroupedRenderers)
            {
                var normalizedMesh = Origin2NormalizedMesh[mkr.Key];
                TTLog.Assert(normalizedMesh.HasUV((int)atlasTargetUVChannel));

                var validRenderers = mkr.Where(r => normalizedMesh.subMeshCount >= targeting.GetMaterials(r).Length).ToArray();
                if (validRenderers.Any() is false) { throw new InvalidProgramException($"{normalizedMesh.name} が 何らかの問題により、メッシュのノーマライズに失敗しているか、不正な状態に突入している可能性があります！"); }
                var maxSlot = validRenderers.Max(r => targeting.GetMaterials(r).Length);//ノーマライズされた場合 subMeshCount が一番 slot の多いやつになるので、 slot が多いやつを持ってくる。
                var renderer = validRenderers.First(r => targeting.GetMaterials(r).Length == maxSlot);

                var bakedMesh = normalizedMesh;
                var needDestroy = false;

                if (renderer is SkinnedMeshRenderer skinnedMeshRenderer)
                {
                    bakedMesh = new Mesh(); bakedMesh.name = normalizedMesh.name + "baked";
                    needDestroy = true;

                    var tempRenderer = UnityEngine.Object.Instantiate(skinnedMeshRenderer);
                    // ここで Parent を移さないと ワールド原点に一時レンダラーが生成されて IslandSelector が壊れる。
                    tempRenderer.transform.SetParent(renderer.transform.parent, false);

                    tempRenderer.sharedMesh = normalizedMesh;
                    tempRenderer.BakeMesh(bakedMesh);
                    UnityEngine.Object.DestroyImmediate(tempRenderer.gameObject);
                }

                Normalized2MeshData[normalizedMesh] = new MeshData(renderer, (bakedMesh, needDestroy), MeshData.GetMatrix(renderer), atlasTargetUVChannel);
            }
        }
        public MeshData GetMeshDataFromMeshID(int meshID)
        {
            return Normalized2MeshData[MeshID2Normalized[meshID]];
        }
        private Dictionary<Mesh, Mesh> CrossSubMeshNormalizingAndExpandMaterialSlot(IDomainReferenceViewer targeting, IGrouping<Mesh, Renderer>[] targetRenderers)
        {
            using var pf = new PFScope("init");
            var normalizedMesh = new Dictionary<Mesh, Mesh>();
            foreach (var MkR in targetRenderers)
            {
                pf.Split(MkR.Key.name);
                var mesh = MkR.Key;
                var materialSlotCount = mesh.subMeshCount;
                foreach (var r in MkR) { materialSlotCount = Mathf.Max(targeting.GetMaterials(r).Length, materialSlotCount); }

                var isOverSubMesh = materialSlotCount > mesh.subMeshCount;
                var isCrossSubMesh = IsCrossSubMesh(mesh);

                if (isOverSubMesh is false && isCrossSubMesh is false) { normalizedMesh[mesh] = UnityEngine.Object.Instantiate(mesh); continue; }

                // Profiler.BeginSample(MkR.Key.name + "-Normalize");
                using (new PFScope("Normalizing"))
                    normalizedMesh[mesh] = Normalizing(mesh, materialSlotCount);
                // Profiler.EndSample();
            }
            return normalizedMesh;

        }

        static bool IsCrossSubMesh(Mesh mesh)
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

        static Mesh Normalizing(Mesh mesh, int expandSlot)
        {
            // Profiler.BeginSample("ReadVertex");
            var vertex = MeshInfoUtility.ReadVertex(mesh, out var meshDesc);
            // Profiler.EndSample();

            // Profiler.BeginSample("subMeshViArray");
            var subMeshVertexArray = new int[expandSlot][];
            var subMeshTopology = new MeshTopology[expandSlot];
            for (var si = 0; subMeshVertexArray.Length > si; si += 1)
            {
                var sourceSubMeshIndex = Math.Min(si, mesh.subMeshCount - 1);
                subMeshVertexArray[si] = mesh.GetIndices(sourceSubMeshIndex);
                subMeshTopology[si] = mesh.GetTopology(sourceSubMeshIndex);
            }
            // Profiler.EndSample();

            // Profiler.BeginSample("subMeshOfVertex");
            var subMeshOfVertex = new Vertex[expandSlot][];
            for (var si = 0; subMeshVertexArray.Length > si; si += 1)
            {
                var subMeshVi = subMeshVertexArray[si];
                var subMeshVert = subMeshOfVertex[si] = new Vertex[subMeshVi.Length];
                for (var vi = 0; subMeshVi.Length > vi; vi += 1)
                {
                    subMeshVert[vi] = vertex[subMeshVi[vi]];
                }
            }
            // Profiler.EndSample();

            // Profiler.BeginSample("useVert");
            {
                var useVert = new HashSet<Vertex>();
                var compareHashSet = new HashSet<Vertex>();
                var replaceDict = new Dictionary<Vertex, Vertex>();
                for (var si = 0; subMeshOfVertex.Length > si; si += 1)
                {
                    var subMeshVert = subMeshOfVertex[si];
                    var subCrossUsed = compareHashSet; subCrossUsed.Clear(); subCrossUsed.UnionWith(subMeshVert);

                    subCrossUsed.IntersectWith(useVert);
                    useVert.UnionWith(subMeshVert);

                    if (subCrossUsed.Any() is false) { continue; }

                    foreach (var v in subCrossUsed) replaceDict.Add(v, v.Clone());

                    for (var vi = 0; subMeshVert.Length > vi; vi += 1)
                    {
                        if (replaceDict.TryGetValue(subMeshVert[vi], out var rv)) { subMeshVert[vi] = rv; }
                    }

                    vertex.AddRange(replaceDict.Values);
                    replaceDict.Clear();
                }
            }
            // Profiler.EndSample();

            // Profiler.BeginSample("Instantiate");
            var modifiedMesh = UnityEngine.Object.Instantiate(mesh);
            modifiedMesh.name = mesh.name + "normalized";
            // Profiler.EndSample();

            MeshInfoUtility.ClearTriangleToWriteVertex(modifiedMesh, meshDesc, vertex);

            // Profiler.BeginSample("SetTriangles");
            modifiedMesh.subMeshCount = subMeshOfVertex.Length;

            var reverseVertexIndex = new Dictionary<Vertex, int>(vertex.Count);
            for (var vi = 0; vertex.Count > vi; vi += 1) { reverseVertexIndex[vertex[vi]] = vi; }

            for (var si = 0; subMeshOfVertex.Length > si; si += 1)
            {
                var newIndexes = subMeshOfVertex[si].Select(v => reverseVertexIndex[v]).ToArray();
                modifiedMesh.SetIndices(newIndexes, subMeshTopology[si], si);
            }
            // Profiler.EndSample();

            //念のために複製
            // Profiler.BeginSample("Instantiate.MoreCloned");
            var moreCloned = UnityEngine.Object.Instantiate(modifiedMesh);
            moreCloned.name = modifiedMesh.name;
            UnityEngine.Object.DestroyImmediate(modifiedMesh);
            // Profiler.EndSample();
            return moreCloned;
        }

        public void Dispose()
        {
            foreach (var mesh in Origin2NormalizedMesh.Values) { UnityEngine.Object.DestroyImmediate(mesh); }
            Origin2NormalizedMesh.Clear();

            foreach (var md in Normalized2MeshData.Values) { try { md.Dispose(); } catch (Exception e) { TexTransCore.TTLog.Exception(e); } }
            Normalized2MeshData.Clear();
        }
    }
}
