#if CONTAINS_AAO
using System;
using System.Collections.Generic;
using System.Linq;
using Anatawa12.AvatarOptimizer.API;
using nadena.dev.ndmf;
using net.rs64.TexTransTool.TextureAtlas;
using net.rs64.TexTransTool.TextureAtlas.AAOCode;
using net.rs64.TexTransTool.Utils;
using net.rs64.TexTransTool.UVIsland;
using Unity.Collections;
using UnityEngine;

namespace net.rs64.TexTransTool.NDMF.AAO
{
    internal class NegotiateAAOPass : TTTPass<NegotiateAAOPass>
    {
        protected override void Execute(BuildContext context)
        {
            var tttCtx = TTTContext(context);
            var tttComponents = tttCtx.PhaseAtList.SelectMany(i => i.Value);
            if (tttComponents.Any() is false) { return; }

            var config = context.AvatarRootObject.GetComponent<NegotiateAAOConfig>();
            var removalToIsland = config?.AAORemovalToIsland ?? true;
            var uvEvacuationAndRegisterToAAO = config?.UVEvacuationAndRegisterToAAO ?? true;
            var overrideEvacuationIndex = (config?.OverrideEvacuationUVChannel ?? false) ? config?.OverrideEvacuationUVChannelIndex : null;

            var uvEditTarget = tttComponents.Where(i => i is AtlasTexture)//後々 UV いじる系でありどの UV をいじるかを示す形が必要になる。今は AtlasTexture しかないから問題ないけど
                .SelectMany(i => i.ModificationTargetRenderers(tttCtx.Domain.EnumerateRenderer(), tttCtx.Domain.OriginEqual)).OfType<SkinnedMeshRenderer>().Distinct();

            List<Vector4> uvBuf = null;
            foreach (var smr in uvEditTarget)
            {
                if (uvEvacuationAndRegisterToAAO && UVUsageCompabilityAPI.IsTexCoordUsed(smr, 0))
                    UVEvacuation(uvBuf, smr, overrideEvacuationIndex);

                if (removalToIsland is false) { continue; }
                var removalProvider = MeshRemovalProvider.GetForRenderer(smr);
                if (removalProvider is not null)
                    using (removalProvider)
                        ProvideToIsland(smr, removalProvider);
            }
        }

        private static void UVEvacuation(List<Vector4> uvBuf, SkinnedMeshRenderer smr, int? overrideEvacuationIndex)
        {
            var mesh = smr.sharedMesh = UnityEngine.Object.Instantiate(smr.sharedMesh);

            var evacuationIndex = 7;
            if (overrideEvacuationIndex is null)
            {
                while (evacuationIndex >= 0 && mesh.HasUV(evacuationIndex)) { evacuationIndex -= 1; }

                if (evacuationIndex == -1)
                {
                    TTTLog.Warning("NegotiateAAO:warn:UVEvacuationFailed", smr);
                    return;
                }
            }
            else { evacuationIndex = overrideEvacuationIndex.Value; }

            uvBuf ??= new();
            mesh.GetUVs(0, uvBuf);
            mesh.SetUVs(evacuationIndex, uvBuf);
            UVUsageCompabilityAPI.RegisterTexCoordEvacuation(smr, 0, evacuationIndex);
        }

        private static void ProvideToIsland(SkinnedMeshRenderer renderer, MeshRemovalProvider removalProvider)
        {
            var mesh = renderer.sharedMesh;
            var editableMesh = UnityEngine.Object.Instantiate(mesh);
            Span<int> triangleBuffer = stackalloc int[3];

            //今は 三角形以外を処理でき無いから、三角形以外があった時は何もしないという安全側にする
            if (Enumerable.Range(0, mesh.subMeshCount).Any(i => mesh.GetTopology(i) is not MeshTopology.Triangles)) { return; }

            var vertex = MeshInfoUtility.ReadVertex(mesh, out var meshDesc);
            var removalsDict = new Dictionary<int, (List<Vertex> Triangles, List<Vertex> VanishTriangles)>();

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

            MeshInfoUtility.ClearTriangleToWriteVertex(editableMesh, meshDesc, finalVertexes);

            for (var i = 0; editableMesh.subMeshCount > i; i += 1)
            {
                if (removalsDict.TryGetValue(i, out var triTuple) is false) { continue; }
                (var tri, var reTri) = triTuple;
                var triangles = tri.Concat(reTri).Select(vi => finalVertexes.IndexOf(vi)).ToArray();
                editableMesh.SetTriangles(triangles, i);
            }

            renderer.sharedMesh = editableMesh;
            ObjectRegistry.RegisterReplacedObject(mesh, editableMesh);
        }
    }
}
#endif
