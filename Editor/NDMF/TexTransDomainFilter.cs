using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
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

        public ReactiveValue<ImmutableList<RenderGroup>> TargetGroups { get; private set; }

        public TexTransDomainFilter(IEnumerable<TexTransPhase> previewTargetPhase)
        {
            PreviewTargetPhase = previewTargetPhase;
            var queryName = string.Join("-", PreviewTargetPhase.Select(i => i.ToString())) + "-TargetRenderers";
            TargetGroups = ReactiveValue<ImmutableList<RenderGroup>>.Create(queryName, QueryPreviewTarget);
        }


        private async Task<ImmutableList<RenderGroup>> QueryPreviewTarget(ComputeContext ctx)
        {
            var ttBehaviors = await ctx.Observe(CommonQueries.GetComponentsByType<TexTransBehavior>());

            var avatarGrouping = GroupingByAvatar(ttBehaviors);
            var allGroups = new List<RenderGroup>();
            foreach (var ag in avatarGrouping)
            {
                var domainRoot = ag.Key;
                var TexTransBehaviors = ag.Value;

                var domainRenderers = ctx.GetComponentsInChildren<Renderer>(domainRoot, true);
                var phaseDict = AvatarBuildUtils.FindAtPhase(TexTransBehaviors);

                var (previewTargetBehavior, behaviorIndex) = GetFlattenBehaviorAndIndex(phaseDict);

                var targetRendererGroup = GetTargetGrouping(ctx, domainRenderers, previewTargetBehavior);
                var renderersGroup2behavior = GetRendererGrouping(behaviorIndex, targetRendererGroup);

                allGroups.AddRange(renderersGroup2behavior.Select(i => RenderGroup.For(i.Key).WithData(i.Value)));
            }

            return allGroups.ToImmutableList();
        }

        private (List<TexTransBehavior> previewTargetBehavior, Dictionary<TexTransBehavior, int> behaviorIndex) GetFlattenBehaviorAndIndex(Dictionary<TexTransPhase, List<TexTransBehavior>> phaseDict)
        {
            var behaviorIndex = new Dictionary<TexTransBehavior, int>();
            var previewTargetBehavior = new List<TexTransBehavior>();
            var index = 0;
            foreach (var phase in PreviewTargetPhase)
            {
                var flattenPhase = AvatarBuildUtils.PhaseFlatten(phaseDict[phase]);
                foreach (var behavior in flattenPhase) { behaviorIndex[behavior] = index; index -= 1; }
                previewTargetBehavior.AddRange(flattenPhase);
            }

            return (previewTargetBehavior, behaviorIndex);
        }

        private static Dictionary<IEnumerable<Renderer>, SortedList<int, TexTransBehavior>> GetRendererGrouping(Dictionary<TexTransBehavior, int> behaviorIndex, Dictionary<TexTransBehavior, HashSet<Renderer>> targetRendererGroup)
        {
            var renderer2Behavior = new Dictionary<Renderer, SortedList<int, TexTransBehavior>>();

            foreach (var trg in targetRendererGroup)
            {
                var thisGroup = new SortedList<int, TexTransBehavior>() { { behaviorIndex[trg.Key], trg.Key } };
                var thisGroupTarget = new HashSet<Renderer>();
                foreach (var target in trg.Value)
                {
                    if (renderer2Behavior.ContainsKey(target))
                    {
                        var group = renderer2Behavior[target];

                        thisGroupTarget.UnionWith(renderer2Behavior.Where(i => i.Value == group).Select(i => i.Key));
                        foreach (var kv in group) { thisGroup[kv.Key] = kv.Value; }
                    }
                    else { thisGroupTarget.Add(target); }
                }

                foreach (var t in thisGroupTarget) { renderer2Behavior[t] = thisGroup; }
            }

            var grouping = new Dictionary<IEnumerable<Renderer>, SortedList<int, TexTransBehavior>>();

            foreach (var group in renderer2Behavior.Values.Distinct().ToArray())
            {
                grouping.Add(renderer2Behavior.Where(i => i.Value == group).Select(i => i.Key), group);
            }

            return grouping;
        }

        private static Dictionary<TexTransBehavior, HashSet<Renderer>> GetTargetGrouping(ComputeContext ctx, Renderer[] domainRenderers, List<TexTransBehavior> previewTargetBehavior)
        {
            var targetRendererGroup = new Dictionary<TexTransBehavior, HashSet<Renderer>>();
            foreach (var ttb in previewTargetBehavior)
            {
                if (!ctx.ActiveInHierarchy(ttb.gameObject)) { continue; }
                var modificationTargets = ttb.ModificationTargetRenderers(domainRenderers, (l, r) => l == r);
                targetRendererGroup.Add(ttb, modificationTargets.ToHashSet());
            }

            return targetRendererGroup;
        }


        private static Dictionary<GameObject, List<TexTransBehavior>> GroupingByAvatar(ImmutableList<TexTransBehavior> ttBehaviors)
        {
            var avatarGrouping = new Dictionary<GameObject, List<TexTransBehavior>>();
            foreach (var ttb in ttBehaviors)
            {
                if (ttb == null) { continue; }
                var root = RuntimeUtil.FindAvatarInParents(ttb.transform);
                if (root == null) { continue; }

                var avatarRootGameObject = root.gameObject;

                if (!avatarGrouping.ContainsKey(avatarRootGameObject)) { avatarGrouping[avatarRootGameObject] = new(); }
                avatarGrouping[avatarRootGameObject].Add(ttb);
            }

            return avatarGrouping;
        }

        public Task<IRenderFilterNode> Instantiate(RenderGroup group, IEnumerable<(Renderer, Renderer)> proxyPairs, ComputeContext context)
        {
            var sortedBehaviors = group.GetData<SortedList<int, TexTransBehavior>>();

            var node = new TexTransPhaseNode();
            node.NodeExecuteAndInit(sortedBehaviors.Select(i => i.Value), proxyPairs, context);

            return Task.FromResult(node as IRenderFilterNode);
        }
    }

    internal class TexTransPhaseNode : IRenderFilterNode
    {
        public RenderAspects Reads => RenderAspects.Everything;

        public RenderAspects WhatChanged => RenderAspects.Material | RenderAspects.Mesh | RenderAspects.Texture;

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
            if (!_proxy2OriginRendererDict.ContainsKey(proxyRenderer)) { Debug.Log($" {proxyRenderer.name} はプロキシーリストにないよ...?"); return; }

            if (_rendererApplyRecaller.ContainsKey(_proxy2OriginRendererDict[proxyRenderer])) { _rendererApplyRecaller[_proxy2OriginRendererDict[proxyRenderer]] += recall; }
            else { _rendererApplyRecaller[_proxy2OriginRendererDict[proxyRenderer]] = recall; }
        }

        public void ReplaceMaterials(Dictionary<Material, Material> mapping)
        {
            foreach (var dr in _proxyDomainRenderers)
            {
                RegisterRecall(dr, i => RendererUtility.SwapMaterials(i, mapping));
                RendererUtility.SwapMaterials(dr, mapping);
            }
            foreach (var matKV in mapping) { RegisterReplace(matKV.Key, matKV.Value); }
            this.transferAssets(mapping.Values);
        }

        public void SetMesh(Renderer renderer, Mesh mesh)
        {
            RegisterRecall(renderer, i => i.SetMesh(mesh));
            renderer.SetMesh(mesh);
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
