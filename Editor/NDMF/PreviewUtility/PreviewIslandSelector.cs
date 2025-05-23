using nadena.dev.ndmf;
using nadena.dev.ndmf.preview;
using net.rs64.TexTransCore;
using net.rs64.TexTransCore.TransTexture;
using net.rs64.TexTransTool.Decal;
using net.rs64.TexTransTool.IslandSelector;
using net.rs64.TexTransTool.Utils;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;


namespace net.rs64.TexTransTool.NDMF
{
    internal class PreviewIslandSelector : IRenderFilter
    {
        internal static PublishedValue<AbstractIslandSelector> PreviewTarget = new(null);
        internal static TogglablePreviewNode PreviewToggle = TogglablePreviewNode.Create(() => "Preview IslandSelector", "PreviewIslandSelector");
        public ImmutableList<RenderGroup> GetTargetGroups(ComputeContext ctx)
        {
            var islandSelector = ctx.Observe(PreviewTarget);
            if (islandSelector == null) { return ImmutableList.Create<RenderGroup>(); }

            var root = ctx.GetAvatarRoot(islandSelector.gameObject);
            if (root == null) { return ImmutableList.Create<RenderGroup>(); }

            var domainRenderers = ctx.GetComponentsInChildren<Renderer>(root, false).Where(r => r is SkinnedMeshRenderer or MeshRenderer);
            return ImmutableList.Create(RenderGroup.For(domainRenderers).WithData(islandSelector));
        }
        public Task<IRenderFilterNode> Instantiate(RenderGroup group, IEnumerable<(Renderer, Renderer)> proxyPairs, ComputeContext context)
        {
            var islandSelector = group.GetData<AbstractIslandSelector>();

            // proxy と origin どちらかだけ null だった場合の扱いをどうすればいいかわからんからどっちかなかったら全部弾くことにする。
            var o2pDict = proxyPairs.Where(i => i.Item1.GetMesh() != null && i.Item2.GetMesh() != null).ToDictionary(i => i.Item1, i => i.Item2);
            var proxyMeshData = o2pDict.Values.Select(i => i.GetToMemorizedMeshData()).ToArray();
            var nodeDomain = new NodeExecuteDomain(o2pDict, context, ObjectRegistry.ActiveRegistry);
            var engine = nodeDomain.GetTexTransCoreEngineForUnity();
            var blKey = engine.QueryBlendKey(ITexTransToolForUnity.BL_KEY_DEFAULT);

            islandSelector.LookAtCalling(nodeDomain);
            var islandsArray = IslandSelectToPPFilter.IslandsArrayFromMeshData(proxyMeshData);
            var bitArray = islandSelector.IslandSelect(new(islandsArray.flattenIslands, islandsArray.flattenIslandDescription, nodeDomain));
            using var islandSelectedTrianglesHolder = IslandSelectToPPFilter.IslandSelectToTriangleIndex(islandsArray.allMeshIslands, islandsArray.islandToIndex, bitArray);
            var islandSelectedTriangles = islandSelectedTrianglesHolder.Ref();

            for (var ri = 0; proxyMeshData.Length > ri; ri += 1)
            {
                var md = proxyMeshData[ri];
                var maskArray = new ITTRenderTexture[md.SubMeshCount];
                var materials = md.ReferenceRenderer.sharedMaterials;

                ITTStorageBuffer uvVertexBuffer = null;
                try
                {
                    for (var si = 0; md.SubMeshCount > si; si += 1)
                    {
                        var triangles = islandSelectedTriangles[ri][si];
                        if (triangles.Length == 0) { continue; }
                        if (materials.Length < si) { continue; }

                        var referenceTexture = materials[si].mainTexture;
                        if (referenceTexture == null) { continue; }

                        using var rt = maskArray[si] = engine.CreateRenderTexture(referenceTexture?.width ?? 512, referenceTexture?.height ?? 512);
                        using var writable = TTRenderTexWithPaddingDistance.CreateFrom(engine, rt, 5f);

                        uvVertexBuffer ??= engine.UploadStorageBuffer<Vector2>(md.VertexUV);
                        using var trianglesBuf = engine.UploadStorageBuffer<int>(MemoryMarshal.Cast<TriangleIndex, int>(triangles));

                        PolygonMask.Write(engine, writable, uvVertexBuffer, trianglesBuf, triangles.Length);
                        nodeDomain.AddTextureStack(referenceTexture, rt, blKey);
                    }
                }
                finally { uvVertexBuffer?.Dispose(); }
            }

            nodeDomain.DomainFinish();
            return Task.FromResult<IRenderFilterNode>(new PreviewIslandSelectorNode(nodeDomain));

        }
        public bool IsEnabled(ComputeContext context)
        {
            var pubVal = PreviewToggle.IsEnabled;
            context.Observe(pubVal);
            return pubVal.Value;
        }
        public IEnumerable<TogglablePreviewNode> GetPreviewControlNodes()
        {
            yield return PreviewToggle;
        }

        class PreviewIslandSelectorNode : IRenderFilterNode
        {
            NodeExecuteDomain _nodeDomain;

            public PreviewIslandSelectorNode(NodeExecuteDomain nodeDomain)
            {
                _nodeDomain = nodeDomain;
            }

            public RenderAspects Reads => _nodeDomain.UsedLookAt ? RenderAspects.Everything : 0;
            public RenderAspects WhatChanged
            {
                get
                {
                    RenderAspects flag = 0;

                    if (_nodeDomain.UsedMaterialReplace) flag |= RenderAspects.Material;
                    if (_nodeDomain.UsedSetMesh) flag |= RenderAspects.Mesh | RenderAspects.Texture;
                    if (_nodeDomain.UsedTextureStack) flag |= RenderAspects.Material | RenderAspects.Texture;

                    return flag;
                }
            }

            public void OnFrame(Renderer original, Renderer proxy)
            {
                _nodeDomain.DomainRecaller(original, proxy);
            }

            void IDisposable.Dispose()
            {
                _nodeDomain.Dispose();
                _nodeDomain = null;
            }

        }



    }
}
