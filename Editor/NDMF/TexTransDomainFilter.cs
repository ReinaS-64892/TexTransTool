using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using nadena.dev.ndmf;
using nadena.dev.ndmf.preview;
using nadena.dev.ndmf.rq;
using nadena.dev.ndmf.rq.unity.editor;
using nadena.dev.ndmf.runtime;
using net.rs64.TexTransCore.BlendTexture;
using net.rs64.TexTransCore.Utils;
using net.rs64.TexTransTool.Build;
using net.rs64.TexTransTool.TextureStack;
using UnityEngine;

namespace net.rs64.TexTransTool.NDMF
{
    internal class TexTransDomainFilter : IRenderFilter
    {
        public IEnumerable<TexTransPhase> PreviewTargetPhase;

        public ReactiveValue<IImmutableList<IImmutableList<Renderer>>> TargetGroups { get; private set; }

        public TexTransDomainFilter(IEnumerable<TexTransPhase> previewTargetPhase)
        {
            PreviewTargetPhase = previewTargetPhase;
            var queryName = string.Join("-", PreviewTargetPhase.Select(i => i.ToString())) + "-TargetRenderers";
            TargetGroups = ReactiveValue<IImmutableList<IImmutableList<Renderer>>>.Create(queryName, QueryPreviewTarget);
        }


        private async Task<IImmutableList<IImmutableList<Renderer>>> QueryPreviewTarget(ComputeContext ctx)
        {
            var ttBehaviors = await ctx.Observe(CommonQueries.GetComponentsByType<TexTransBehavior>());

            var avatarGrouping = GroupingByAvatar(ttBehaviors);
            var targetGroupRenderer = new List<List<Renderer>>();
            foreach (var ag in avatarGrouping)
            {
                var phaseDict = AvatarBuildUtils.FindAtPhase(ag.Value);
                var gr = GetPreviewTargetsOnDomain(ctx, ag.Key, phaseDict, PreviewTargetPhase);
                foreach (var gr2 in gr.PreviewGroup) { targetGroupRenderer.Add(gr2); }
            }

            return targetGroupRenderer.Select(i => (IImmutableList<Renderer>)ImmutableList.CreateRange(i)).ToImmutableList();
        }

        private static (List<List<Renderer>> PreviewGroup, Dictionary<Renderer, HashSet<TexTransBehavior>> ModTargetMap) GetPreviewTargetsOnDomain(ComputeContext ctx, GameObject domainRoot, Dictionary<TexTransPhase, List<TexTransBehavior>> phaseDict, IEnumerable<TexTransPhase> targetPhaseList)
        {
            var domainRenderers = ctx.GetComponentsInChildren<Renderer>(domainRoot, true);

            var targetGroup = new List<HashSet<Renderer>>();
            var modTargetMap = new Dictionary<Renderer, HashSet<TexTransBehavior>>();

            foreach (var targetPhase in targetPhaseList)
            {
                var ttbList = AvatarBuildUtils.PhaseFlatten(phaseDict[targetPhase]);

                foreach (var ttb in ttbList)
                {
                    if (!ctx.ActiveInHierarchy(ttb.gameObject)) { continue; }
                    var target = ttb.ModificationTargetRenderers(domainRenderers, (l, r) => l == r);

                    var index = targetGroup.FindIndex(g => g.Intersect(target).Any());

                    if (index == -1) { targetGroup.Add(target.ToHashSet()); }
                    else { targetGroup[index].UnionWith(target); }

                    foreach (var r in target)
                    {
                        if (!modTargetMap.ContainsKey(r)) { modTargetMap[r] = new(); }
                        modTargetMap[r].Add(ttb);
                    }
                }
            }

            ForMarge();
            targetGroup.RemoveAll(g => !g.Any());

            void ForMarge()
            {
                var indexMax = targetGroup.Count - 1;
                for (var l = 0; indexMax > l; l += 1)
                    for (var r = l + 1; indexMax >= r; r += 1)
                    {
                        var intersect = targetGroup[l].Intersect(targetGroup[r]).Any();
                        if (intersect)
                        {
                            var margG = targetGroup[r];
                            var margS = targetGroup[l];
                            margS.UnionWith(margG);

                            targetGroup.RemoveAt(r);
                            ForMarge();
                            return;
                        }
                    }
            }

            return (targetGroup.Select(g => g.ToList()).ToList(), modTargetMap);
        }

        private static Dictionary<GameObject, List<TexTransBehavior>> GroupingByAvatar(ImmutableList<TexTransBehavior> ttBehaviors)
        {
            var avatarGrouping = new Dictionary<GameObject, List<TexTransBehavior>>();
            foreach (var ttb in ttBehaviors)
            {
                var root = RuntimeUtil.FindAvatarInParents(ttb.transform);
                if (root == null) { continue; }
                var avatarRootGameObject = root.gameObject;
                if (!avatarGrouping.ContainsKey(avatarRootGameObject)) { avatarGrouping[avatarRootGameObject] = new(); }
                avatarGrouping[avatarRootGameObject].Add(ttb);
            }

            return avatarGrouping;
        }

        public async Task<IRenderFilterNode> Instantiate(IEnumerable<(Renderer, Renderer)> proxyPairs, ComputeContext context)
        {
            var ttBehaviors = await context.Observe(CommonQueries.GetComponentsByType<TexTransBehavior>());

            var avatarGrouping = GroupingByAvatar(ttBehaviors);

            var previewTargetGroup = proxyPairs.Select(r => r.Item1).ToHashSet();
            foreach (var avatar2TTB in avatarGrouping)
            {
                var phaseDict = AvatarBuildUtils.FindAtPhase(avatar2TTB.Value);
                var (previewGroup, modTargetMap) = GetPreviewTargetsOnDomain(context, avatar2TTB.Key, phaseDict, PreviewTargetPhase);
                foreach (var group in previewGroup)
                {
                    if (new HashSet<Renderer>(group).SequenceEqual(previewTargetGroup))
                    {
                        var previewExecuteTTB = previewTargetGroup.SelectMany(i => modTargetMap[i]).ToHashSet();
                        var previewExecuteTTBAsOrdered = PreviewTargetPhase.SelectMany(i => AvatarBuildUtils.PhaseFlatten(phaseDict[i])).Where(i => previewExecuteTTB.Contains(i)).ToList();
                        var node = new TexTransPhaseNode();
                        node.NodeExecuteAndInit(previewExecuteTTBAsOrdered, proxyPairs, context);
                        return node;
                    }
                }
            }
            throw new InvalidOperationException("にゃああああああああああああああああああ！");
        }
    }

    internal class TexTransPhaseNode : IRenderFilterNode
    {
        public ulong Reads => IRenderFilterNode.Everything;

        public ulong WhatChanged => IRenderFilterNode.Material | IRenderFilterNode.Mesh | IRenderFilterNode.Texture;

        NodeExecuteDomain _nodeDomain;

        public void NodeExecuteAndInit(IEnumerable<TexTransBehavior> flattenTTB, IEnumerable<(Renderer origin, Renderer proxy)> proxyPairs, ComputeContext ctx)
        {
            _nodeDomain = new NodeExecuteDomain(proxyPairs, ctx);
            foreach (var ttb in flattenTTB) { ttb.Apply(_nodeDomain); }
            _nodeDomain.DomainFinish();
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

    internal class NodeExecuteDomain : IEditorCallDomain, IDisposable
    {
        HashSet<UnityEngine.Object> _transferredObject = new();
        protected readonly StackManager<ImmediateTextureStack> _textureStacks;
        protected readonly ITextureManager _textureManager;

        ComputeContext _ctx;

        List<Renderer> _proxyDomainRenderers;
        Dictionary<Renderer, Renderer> _proxy2OriginRendererDict;

        protected Dictionary<UnityEngine.Object, UnityEngine.Object> _replaceMap = new();//New Old

        Dictionary<Renderer, Action<Renderer>> _rendererApplyRecaller = new();//origin 2 apply call

        public NodeExecuteDomain(IEnumerable<(Renderer origin, Renderer proxy)> renderers, ComputeContext computeContext)
        {
            _proxyDomainRenderers = renderers.Select(i => i.proxy).ToList();
            _proxy2OriginRendererDict = renderers.ToDictionary(i => i.proxy, i => i.origin);
            _textureManager = new TextureManager(false);
            _textureStacks = new(_textureManager);
            _ctx = computeContext;
        }

        public void LookAt(UnityEngine.Object obj) { _ctx?.Observe(obj); }

        public void AddTextureStack<BlendTex>(Texture dist, BlendTex setTex) where BlendTex : TextureBlend.IBlendTexturePair
        { _textureStacks.AddTextureStack(dist as Texture2D, setTex); }
        public IEnumerable<Renderer> EnumerateRenderer() { return _proxyDomainRenderers; }

        public ITextureManager GetTextureManager() => _textureManager;

        public bool IsPreview() => false;

        private void RegisterRecall(Renderer proxyRenderer, Action<Renderer> recall)
        {
            if (_rendererApplyRecaller.ContainsKey(_proxy2OriginRendererDict[proxyRenderer])) { _rendererApplyRecaller[_proxy2OriginRendererDict[proxyRenderer]] += recall; }
            else { _rendererApplyRecaller[_proxy2OriginRendererDict[proxyRenderer]] = recall; }
        }

        public void ReplaceMaterials(Dictionary<Material, Material> mapping)
        {
            foreach (var dr in _proxyDomainRenderers)
            {
                RendererUtility.SwapMaterials(dr, mapping);
                RegisterRecall(dr, i => RendererUtility.SwapMaterials(i, mapping));
            }
            foreach (var matKV in mapping) { RegisterReplace(matKV.Key, matKV.Value); }
            this.transferAssets(mapping.Values);
        }

        public void SetMesh(Renderer renderer, Mesh mesh)
        {
            renderer.SetMesh(mesh);
            RegisterRecall(renderer, i => i.SetMesh(mesh));
        }

        public void RegisterReplace(UnityEngine.Object oldObject, UnityEngine.Object nowObject)
        {
            ObjectRegistry.RegisterReplacedObject(oldObject, nowObject);
        }
        public bool OriginEqual(UnityEngine.Object l, UnityEngine.Object r)
        {
            if (l == r) { return true; }
            if (l is Renderer lRen && r is Renderer rRen)
            {
                if (RenderersDomain.GetOrigin(_proxy2OriginRendererDict, lRen) == RenderersDomain.GetOrigin(_proxy2OriginRendererDict, rRen)) { return true; }
            }
            return ObjectRegistry.GetReference(l) == ObjectRegistry.GetReference(r);
        }

        public void SetSerializedProperty(UnityEditor.SerializedProperty property, UnityEngine.Object value)
        {
            property.objectReferenceValue = value;
            property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        public void TransferAsset(UnityEngine.Object asset) { _transferredObject.Add(asset); }


        public void DomainFinish()
        {
            var MergedStacks = _textureStacks.MergeStacks();

            foreach (var mergeResult in MergedStacks)
            {
                if (mergeResult.FirstTexture == null || mergeResult.MergeTexture == null) continue;
                SetTexture(mergeResult.FirstTexture, mergeResult.MergeTexture);
                TransferAsset(mergeResult.MergeTexture);
            }

            _textureManager.DestroyDeferred();
            _textureManager.CompressDeferred();


            void SetTexture(Texture2D firstTexture, Texture2D mergeTexture)
            {
                var mats = RendererUtility.GetFilteredMaterials(_proxyDomainRenderers);
                ReplaceMaterials(MaterialUtility.ReplaceTextureAll(mats, firstTexture, mergeTexture));
                RegisterReplace(firstTexture, mergeTexture);
            }
        }

        public void Dispose()
        {
            foreach (var obj in _transferredObject) { UnityEngine.Object.DestroyImmediate(obj); }
            _ctx = null;
        }

        internal void DomainRecaller(Renderer original, Renderer proxy)
        {
            _rendererApplyRecaller[original].Invoke(proxy);
        }
    }
}
