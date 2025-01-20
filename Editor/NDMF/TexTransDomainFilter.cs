#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
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
            Profiler.BeginSample("TexTransDomainFilter.QueryPreviewTarget-" + PreviewTargetPhase.ToString());
            Profiler.BeginSample("GetAvatarRoots");
            var avatarRoots = ctx.GetAvatarRoots();
            Profiler.EndSample();
            var allGroups = new List<RenderGroup>();
            var waker = new NDMFGameObjectObservedWaker(ctx);

            foreach (var root in avatarRoots)
            {
                //ルートから無効化されている場合そもそもプレビューする意味がないので完全スキップ
                if (ctx.ActiveInHierarchy(root) is false) { continue; }

                Profiler.BeginSample(root.name, root);
                Profiler.BeginSample("FindAtPhase");

                var behaviors = AvatarBuildUtils.FindAtPhase(root, waker)[PreviewTargetPhase];

                Profiler.EndSample();
                Profiler.BeginSample("domain2PhaseList");

                behaviors.RemoveAll(b => AvatarBuildUtils.CheckIsActiveBehavior(b, waker, root) is false);//ここで消すと同時に監視。

                Profiler.EndSample();
                Profiler.BeginSample("Grouping");
                var domainRenderers = ctx.GetComponentsInChildren<Renderer>(root, true).Where(r => r is SkinnedMeshRenderer or MeshRenderer).ToArray();
                var behaviorIndex = GetFlattenBehaviorAndIndex(behaviors);

                var targetRendererGroup = GetTargetGrouping(behaviorIndex, domainRenderers);
                var renderersGroup2behavior = GetRendererGrouping(targetRendererGroup);

                allGroups.AddRange(renderersGroup2behavior.Select(i => RenderGroup.For(i.Key).WithData(new PassingData(i.Value, domainRenderers, behaviorIndex))));
                Profiler.EndSample();

                Profiler.EndSample();
            }

            Profiler.EndSample();
            return allGroups.ToImmutableList();
        }
        class PassingData
        {
            public HashSet<TexTransBehavior> Behaviors;
            public Renderer[] DomainRenderers;
            public Dictionary<TexTransBehavior, int> BehaviorIndex;

            public PassingData(HashSet<TexTransBehavior> value, Renderer[] domainRenderers, Dictionary<TexTransBehavior, int> behaviorIndex)
            {
                Behaviors = value;
                DomainRenderers = domainRenderers;
                BehaviorIndex = behaviorIndex;
            }
        }
        private Dictionary<TexTransBehavior, int> GetFlattenBehaviorAndIndex(List<TexTransBehavior> behaviors)
        {
            var behaviorIndex = new Dictionary<TexTransBehavior, int>();

            var index = 0;
            foreach (var b in behaviors)
            {
                behaviorIndex[b] = index;
                index += 1;
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

        private static Dictionary<TexTransBehavior, HashSet<Renderer>> GetTargetGrouping(Dictionary<TexTransBehavior, int> behaviorIndex, Renderer[] ofRenderers)
        {
            var targetRendererGroup = new Dictionary<TexTransBehavior, HashSet<Renderer>>();
            foreach (var ttbKV in behaviorIndex)
            {
                Profiler.BeginSample("Mod target", ttbKV.Key);
                var modificationTargets = ttbKV.Key.ModificationTargetRenderers(ofRenderers, (l, r) => l == r);
                Profiler.EndSample();
                targetRendererGroup.Add(ttbKV.Key, modificationTargets.ToHashSet());
            }
            return targetRendererGroup;
        }

        public async Task<IRenderFilterNode> Instantiate(RenderGroup group, IEnumerable<(Renderer, Renderer)> proxyPairs, ComputeContext context)
        {
            var data = group.GetData<PassingData>();

            await Task.Delay(0);

            var o2pDict = proxyPairs.ToDictionary(i => i.Item1, i => i.Item2);
            var node = new TexTransPhaseNode();
            node.TargetPhase = PreviewTargetPhase;
            node.o2pDict = o2pDict;
            node.domainRenderers = o2pDict.Values.ToArray();
            node.behaviorIndex = data.BehaviorIndex;
#if TTT_DISPLAY_RUNTIME_LOG
            var timer = System.Diagnostics.Stopwatch.StartNew();
#endif

            Profiler.BeginSample("node.NodeExecuteAndInit");
            node.NodeExecuteAndInit(data.Behaviors, context);
            Profiler.EndSample();

#if TTT_DISPLAY_RUNTIME_LOG
            timer.Stop();
            Debug.Log($" time:{timer.ElapsedMilliseconds}ms - Instantiate: {string.Join("-", PreviewTargetPhase.ToString())}  \n  {string.Join("-", group.Renderers.Select(r => r.gameObject.name))} ");
#endif
            return node;
        }
        public IEnumerable<TogglablePreviewNode> GetPreviewControlNodes()
        {
            yield return NDMFPlugin.s_togglablePreviewPhases[PreviewTargetPhase];
        }


        internal struct NDMFGameObjectObservedWaker : AvatarBuildUtils.IGameObjectWakingTool, AvatarBuildUtils.IGameObjectActivenessWakingTool
        {
            ComputeContext _context;
            public NDMFGameObjectObservedWaker(ComputeContext context)
            {
                _context = context;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool ActiveSelf(GameObject gameObject)
            {
                return _context.Observe(gameObject, g => g.gameObject.activeSelf);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public GameObject? GetChilde(GameObject gameObject, int index)
            {
                return _context.Observe(gameObject, (g) => g.transform.GetChild(index)?.gameObject);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int GetChilesCount(GameObject gameObject)
            {
                return _context.Observe(gameObject, (g) => g.transform.childCount);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public C GetComponent<C>(GameObject gameObject) where C : Component
            {
                return _context.GetComponent<C>(gameObject);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public C[] GetComponentsInChildren<C>(GameObject gameObject, bool includeInactive) where C : Component
            {
                return _context.GetComponentsInChildren<C>(gameObject, includeInactive);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public GameObject? GetParent(GameObject gameObject)
            {
                return _context.Observe(gameObject, g => g.transform.parent?.gameObject);
            }
        }
    }
}
