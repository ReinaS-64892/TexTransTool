using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using nadena.dev.ndmf.preview;
using nadena.dev.ndmf.runtime;
using net.rs64.TexTransTool.Build;
using UnityEngine;
using UnityEngine.Profiling;

namespace net.rs64.TexTransTool.NDMF
{
    internal class TexTransDomainFilter : IRenderFilter
    {
        public TexTransPhase PreviewTargetPhase;

        public TexTransDomainFilter(TexTransPhase previewTargetPhase)
        {
            PreviewTargetPhase = previewTargetPhase;
        }
        public ImmutableList<RenderGroup> GetTargetGroups(ComputeContext context)
        {
            return QueryPreviewTarget(context);
        }

        public bool IsEnabled(ComputeContext context)
        {
            var pubVal = NDMFPlugin.s_togglablePreviewPhases[PreviewTargetPhase].IsEnabled;
            context.Observe(pubVal);
            return pubVal.Value;
        }
        private ImmutableList<RenderGroup> QueryPreviewTarget(ComputeContext ctx)
        {
            var avatarRoots = ctx.GetAvatarRoots();
            var allGroups = new List<RenderGroup>();

            AvatarBuildUtils.GetComponent getComponent = (t, g) => ctx.GetComponent(g, t);
            AvatarBuildUtils.GetComponentsInChildren getComponentsInChildren = (t, g, i) => ctx.GetComponentsInChildren(g, t, i);

            foreach (var root in avatarRoots)
            {
                var domain2PhaseList = AvatarBuildUtils.FindAtPhase(root, getComponent, getComponentsInChildren);
                foreach (var d in domain2PhaseList)
                {
                    var behaviors = d.Behaviour[PreviewTargetPhase];
                    behaviors.RemoveAll(a => LookAtIsActive(a, ctx) is false);//ここで消すと同時に監視となる。
                    foreach (var b in behaviors) { ctx.Observe(b); }
                }
                var ofRenderers = domain2PhaseList.Select(i => i.Domain != null ? ctx.GetComponentsInChildren<Renderer>(i.Domain.gameObject, true).Where(r => r is SkinnedMeshRenderer or MeshRenderer).ToArray() : ctx.GetComponentsInChildren<Renderer>(root, true)).Where(r => r is SkinnedMeshRenderer or MeshRenderer).ToArray();
                var behaviorIndex = GetFlattenBehaviorAndIndex(domain2PhaseList);

                var targetRendererGroup = GetTargetGrouping(behaviorIndex, ofRenderers);
                var renderersGroup2behavior = GetRendererGrouping(targetRendererGroup);

                allGroups.AddRange(renderersGroup2behavior.Select(i => RenderGroup.For(i.Key).WithData(new PassingData(i.Value, ofRenderers, behaviorIndex))));
            }

            return allGroups.ToImmutableList();
        }
        class PassingData
        {
            public HashSet<TexTransBehavior> Behaviors;
            public Renderer[][] domainOfRenderers;
            public Dictionary<TexTransBehavior, (int index, int domainIndex)> behaviorIndex;

            public PassingData(HashSet<TexTransBehavior> value, Renderer[][] ofRenderers, Dictionary<TexTransBehavior, (int index, int domainIndex)> behaviorIndex)
            {
                this.Behaviors = value;
                this.domainOfRenderers = ofRenderers;
                this.behaviorIndex = behaviorIndex;
            }
        }
        private Dictionary<TexTransBehavior, (int index, int domainIndex)> GetFlattenBehaviorAndIndex(List<Domain2Behavior> domain2Behaviors)
        {
            var behaviorIndex = new Dictionary<TexTransBehavior, (int index, int domainIndex)>();
            var index = 0;
            var domainIndex = 0;
            foreach (var phase in domain2Behaviors)
            {
                var behaviors = phase.Behaviour[PreviewTargetPhase];
                foreach (var behavior in behaviors)
                {
                    behaviorIndex[behavior] = (index, domainIndex);
                    index += 1;
                }
                domainIndex += 1;
            }

            return behaviorIndex;
        }

        private static Dictionary<IEnumerable<Renderer>, HashSet<TexTransBehavior>> GetRendererGrouping(Dictionary<TexTransBehavior, HashSet<Renderer>> targetRendererGroup)
        {
            var renderer2Behavior = new Dictionary<Renderer, HashSet<TexTransBehavior>>();

            foreach (var targetKV in targetRendererGroup)
            {
                var thisTTGroup = new HashSet<TexTransBehavior>() { { targetKV.Key } };
                var thisGroupTarget = new HashSet<Renderer>();
                foreach (var target in targetKV.Value)
                {
                    if (renderer2Behavior.ContainsKey(target))
                    {
                        var ttbGroup = renderer2Behavior[target];
                        thisGroupTarget.UnionWith(renderer2Behavior.Where(i => i.Value == ttbGroup).Select(i => i.Key));//同じ TTB が紐づくレンダラーを集める
                        thisTTGroup.UnionWith(ttbGroup);
                    }
                    else { thisGroupTarget.Add(target); }
                }

                foreach (var t in thisGroupTarget) { renderer2Behavior[t] = thisTTGroup; }
            }

            var grouping = new Dictionary<IEnumerable<Renderer>, HashSet<TexTransBehavior>>();
            foreach (var group in renderer2Behavior.Values.Distinct()) { grouping.Add(renderer2Behavior.Where(i => i.Value == group).Select(i => i.Key), group); }
            return grouping;
        }

        private static Dictionary<TexTransBehavior, HashSet<Renderer>> GetTargetGrouping(Dictionary<TexTransBehavior, (int index, int domainIndex)> behaviorIndex, Renderer[][] ofRenderers)
        {
            var targetRendererGroup = new Dictionary<TexTransBehavior, HashSet<Renderer>>();
            foreach (var ttbKV in behaviorIndex)
            {
                var modificationTargets = ttbKV.Key.ModificationTargetRenderers(ofRenderers[ttbKV.Value.domainIndex], (l, r) => l == r);
                targetRendererGroup.Add(ttbKV.Key, modificationTargets.ToHashSet());
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

        public async Task<IRenderFilterNode> Instantiate(RenderGroup group, IEnumerable<(Renderer, Renderer)> proxyPairs, ComputeContext context)
        {
            var data = group.GetData<PassingData>();

            await Task.Delay(0);

            var o2pDict = proxyPairs.ToDictionary(i => i.Item1, i => i.Item2);
            var node = new TexTransPhaseNode();
            node.TargetPhase = PreviewTargetPhase;
            node.o2pDict = o2pDict;
            node.ofRenderers = data.domainOfRenderers.Select(i => i.Where(r => o2pDict.ContainsKey(r)).Select(r => o2pDict[r]).ToArray()).ToArray();
            node.behaviorIndex = data.behaviorIndex;
            var timer = System.Diagnostics.Stopwatch.StartNew();

            Profiler.BeginSample("node.NodeExecuteAndInit");
            node.NodeExecuteAndInit(data.Behaviors, context);
            Profiler.EndSample();

            timer.Stop();
#if TTT_DISPLAY_RUNTIME_LOG
            Debug.Log($" time:{timer.ElapsedMilliseconds}ms - Instantiate: {string.Join("-", PreviewTargetPhase.ToString())}  \n  {string.Join("-", group.Renderers.Select(r => r.gameObject.name))} ");
#endif
            return node;
        }
        public IEnumerable<TogglablePreviewNode> GetPreviewControlNodes()
        {
            yield return NDMFPlugin.s_togglablePreviewPhases[PreviewTargetPhase];
        }

        static bool LookAtIsActive(TexTransBehavior ttb, ComputeContext ctx)
        {
            var state = true;
            foreach (var tf in ctx.ObservePath(ttb.transform))
            {
                var activenessChanger = ctx.GetComponent<IActivenessChanger>(tf.gameObject);
                if (activenessChanger is not null) { return state && activenessChanger.IsActive; }
                state &= tf.gameObject.activeSelf;
            }
            return state;
        }
    }
}
