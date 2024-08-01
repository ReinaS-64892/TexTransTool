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
        public IEnumerable<TexTransPhase> PreviewTargetPhase;

        public TexTransDomainFilter(IEnumerable<TexTransPhase> previewTargetPhase)
        {
            PreviewTargetPhase = previewTargetPhase;
        }
        public ImmutableList<RenderGroup> GetTargetGroups(ComputeContext context)
        {
            return QueryPreviewTarget(context);
        }

        public bool IsEnabled(ComputeContext context)
        {
            var pubVal = NDMFPlugin.s_togglablePreviewPhases[PreviewTargetPhase.First()].IsActive;
            context.Observe(pubVal);
            return pubVal.Value;
        }
        private ImmutableList<RenderGroup> QueryPreviewTarget(ComputeContext ctx)
        {
            var ttBehaviors = ctx.GetComponentsByType<TexTransBehavior>();
            foreach (var ttb in ttBehaviors) { ctx.Observe(ttb); }

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
                foreach (var behavior in flattenPhase) { behaviorIndex[behavior] = index; index += 1; }
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

        public async Task<IRenderFilterNode> Instantiate(RenderGroup group, IEnumerable<(Renderer, Renderer)> proxyPairs, ComputeContext context)
        {
            var sortedBehaviors = group.GetData<SortedList<int, TexTransBehavior>>().Select(i => i.Value).ToArray();

            await Task.Delay(0);

            var node = new TexTransPhaseNode();
            node.TargetPhase = PreviewTargetPhase;
            var timer = System.Diagnostics.Stopwatch.StartNew();

            Profiler.BeginSample("node.NodeExecuteAndInit");
            node.NodeExecuteAndInit(sortedBehaviors, proxyPairs, context);
            Profiler.EndSample();

            timer.Stop();
#if TTT_DISPLAY_RUNTIME_LOG
            Debug.Log($" time:{timer.ElapsedMilliseconds}ms - Instantiate: {string.Join("-", PreviewTargetPhase.Select(i => i.ToString()))}  \n  {string.Join("-", group.Renderers.Select(r => r.gameObject.name))} ");
#endif
            return node;
        }
    }
}
