using nadena.dev.ndmf;
using nadena.dev.ndmf.preview;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;


namespace net.rs64.TexTransTool.NDMF
{
    internal class EverythingUnlitTexture : IRenderFilter
    {
        public ImmutableList<RenderGroup> GetTargetGroups(ComputeContext context)
        {
            return context.GetComponentsByType<Renderer>()
                .Where(r => r is SkinnedMeshRenderer or MeshRenderer)
                .Where(r => context.ActiveInHierarchy(r.gameObject))
                .Select(r => RenderGroup.For(r))
                .ToImmutableList();
        }
        public bool IsEnabled(ComputeContext context)
        {
            var pubVal = s_EverythingUnlitTexture.IsEnabled;
            context.Observe(pubVal);
            return pubVal.Value;
        }
        public IEnumerable<TogglablePreviewNode> GetPreviewControlNodes()
        {
            yield return s_EverythingUnlitTexture;
        }
        internal static TogglablePreviewNode s_EverythingUnlitTexture = TogglablePreviewNode.Create(() => "Everything Unlit/Texture", "EverythingUnlitTexture", initialState: false);
        const string UNLIT_TEXTURE = "Unlit/Texture";

        static Shader _unlitTextureShader;
        static Shader GetUnlitTextureShader => _unlitTextureShader ??= Shader.Find(UNLIT_TEXTURE);
        public Task<IRenderFilterNode> Instantiate(RenderGroup group, IEnumerable<(Renderer, Renderer)> proxyPairs, ComputeContext context)
        {
            var matDict = new Dictionary<Material, Material>();
            foreach (var proxy in proxyPairs.Select(i => i.Item2))
            {
                var mats = proxy.sharedMaterials;
                for (var i = 0; mats.Length > i; i += 1)
                {
                    if (matDict.ContainsKey(mats[i])) { continue; }
                    var preMat = mats[i];
                    var previewMat = mats[i] = NDMFPreviewMaterialPool.GetFromShaderWithUninitialized(GetUnlitTextureShader);
                    previewMat.mainTexture = preMat.mainTexture;
                    matDict[preMat] = previewMat;
                }

            }
            return Task.FromResult<IRenderFilterNode>(new FilterUnlitTexture(matDict));
        }
        class FilterUnlitTexture : IRenderFilterNode
        {
            private Dictionary<Material, Material> previewMaterials;

            public FilterUnlitTexture(Dictionary<Material, Material> matDict)
            {
                this.previewMaterials = matDict;
            }

            public RenderAspects WhatChanged => RenderAspects.Material;

            public void OnFrame(Renderer original, Renderer proxy)
            {
                var mats = proxy.sharedMaterials;
                for (var i = 0; mats.Length > i; i += 1)
                { previewMaterials.TryGetValue(mats[i], out mats[i]); }
                proxy.sharedMaterials = mats;
            }

            void IDisposable.Dispose()
            {
                foreach (var previewMat in previewMaterials.Values)
                    NDMFPreviewMaterialPool.Ret(previewMat);
                previewMaterials.Clear();
            }
        }
    }
}
