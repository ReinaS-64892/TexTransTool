#if CONTAINS_AAO
using System;
using System.Collections.Generic;
using System.Linq;
using Anatawa12.AvatarOptimizer.API;
using nadena.dev.ndmf;
using net.rs64.TexTransCore.Island;
using net.rs64.TexTransCore.Utils;
using net.rs64.TexTransTool.TextureAtlas.AAOCode;
using net.rs64.TexTransTool.Utils;
using Unity.Collections;
using UnityEngine;

namespace net.rs64.TexTransTool.NDMF.AAO
{
    internal class ProvideMeshRemovalToIsland : TTTPass<ProvideMeshRemovalToIsland>
    {
        protected override void Execute(BuildContext context)
        {
            if (TTTContext(context).PhaseAtList[TexTransPhase.Optimizing].Any() is false) { return; }

            var renderers = context.AvatarRootObject.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            var meshRemovals = renderers.ToDictionary(r => r, r => MeshRemovalProvider.GetForRenderer(r));


            foreach (var mKr in meshRemovals)
            {
                var renderer = mKr.Key;
                var removalProvider = mKr.Value;

                if (removalProvider is null) { continue; }

                ProvideToIsland(renderer, removalProvider);
            }
        }

        private static void ProvideToIsland(SkinnedMeshRenderer renderer, MeshRemovalProvider removalProvider)
        {
            var mesh = renderer.sharedMesh;

            //今は 三角形以外を処理でき無いから、三角形以外があった時は何もしないという安全側にする
            if (Enumerable.Range(0, mesh.subMeshCount).Any(i => mesh.GetTopology(i) is not MeshTopology.Triangles)) { return; }

            var vertex = MeshInfoUtility.ReadVertex(mesh, out var meshDesc);
            var removalsDict = new Dictionary<int, (List<Vertex> Triangles, List<Vertex> VanishTriangles)>();


            using (removalProvider)
            using (var uv = new NativeArray<Vector2>(mesh.vertexCount, Allocator.Temp))
            {
                using (var meshDataArray = Mesh.AcquireReadOnlyMeshData(mesh))
                {
                    var mainData = meshDataArray[0];
                    mainData.GetUVs(0, uv);
                }

                for (var subMeshIndex = 0; mesh.subMeshCount > subMeshIndex; subMeshIndex += 1)
                {
                    var islands = IslandUtility.UVtoIsland(mesh.GetSubTriangleIndex(subMeshIndex), uv.AsList());

                    var triangles = new List<Vertex>();
                    var vanishTriangles = new List<Vertex>();

                    foreach (var island in islands)
                    {
                        foreach (var tri in island.triangles)
                        {
                            Span<int> triangleBuffer = stackalloc int[3];
                            triangleBuffer[0] = tri[0];
                            triangleBuffer[1] = tri[1];
                            triangleBuffer[2] = tri[2];

                            var triVrtx = tri.Select(i => vertex[i]);
                            if (removalProvider.WillRemovePrimitive(MeshTopology.Triangles, subMeshIndex, triangleBuffer))
                            { vanishTriangles.AddRange(triVrtx); }
                            else { triangles.AddRange(triVrtx); }
                        }

                        if (removalsDict.ContainsKey(subMeshIndex) is false) { removalsDict.Add(subMeshIndex, new()); }
                        removalsDict[subMeshIndex] = (triangles, vanishTriangles);
                    }
                }
            }

            if (removalsDict.Any(i => i.Value.VanishTriangles.Any()) is false) { return; }

            var vanishVertex = new List<Vertex>();
            foreach (var kv in removalsDict)
            {
                var reTri = kv.Value.VanishTriangles;
                var replaceDict = reTri.Distinct().ToDictionary(t => t, t => t.Clone());
                vanishVertex.AddRange(replaceDict.Values);
                var replaced = reTri.Select(t => replaceDict[t]).ToArray();
                reTri.Clear();
                reTri.AddRange(replaced);
            }

            foreach (var vert in vanishVertex) { vert.TexCoord0 = new Vector2(-1, -1); }

            var usedHash = new HashSet<Vertex>(removalsDict.SelectMany(i => i.Value.Triangles));
            vertex.RemoveAll(i => usedHash.Contains(i) is false);
            var finalVertexes = vertex.Concat(vanishVertex).ToList();

            var editableMesh = UnityEngine.Object.Instantiate(mesh);
            MeshInfoUtility.WriteVertex(editableMesh, meshDesc, finalVertexes);

            for (var i = 0; editableMesh.subMeshCount > i; i += 1)
            {
                (var tri, var reTri) = removalsDict[i];
                var triangles = tri.Concat(reTri).Select(vi => finalVertexes.IndexOf(vi)).ToArray();
                editableMesh.SetTriangles(triangles, i);
            }

            renderer.sharedMesh = editableMesh;
            ObjectRegistry.RegisterReplacedObject(mesh, editableMesh);
        }
    }
}
#endif
