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
using net.rs64.TexTransTool.Utils;
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

                var behaviors = Memoize.Memo(root, AvatarBuildUtils.FindAtPhase)[PreviewTargetPhase];

                Profiler.EndSample();
                Profiler.BeginSample("Observing");

                // 順序の変更を見るために Path を監視
                foreach (var b in behaviors) ctx.ObservePath(b.transform);

                // 増減をみるために 無意味な GetComponentsInChildren を呼ぶ
                ctx.GetComponentsInChildren<TexTransMonoBaseGameObjectOwned>(root, true);

                Profiler.EndSample();
                Profiler.BeginSample("domain2PhaseList");

                behaviors.RemoveAll(b => AvatarBuildUtils.CheckIsActiveBehavior(b, waker, root) is false);//ここで消すと同時に監視。

                Profiler.EndSample();
                Profiler.BeginSample("Grouping");
                Profiler.BeginSample("get domain renderer and flatten");
                var domainRenderers = ctx.GetComponentsInChildren<Renderer>(root, true).Where(r => r is SkinnedMeshRenderer or MeshRenderer).ToArray();
                var behaviorIndex = GetFlattenBehaviorAndIndex(behaviors);
                Profiler.EndSample();

                Dictionary<TexTransBehavior, HashSet<Renderer>> targetGrouping;

                if (behaviors.Any(b => b is IRendererTargetingAffecter))
                {
                    Profiler.BeginSample("NDMFAffectingRendererTargeting ctr");
                    using var affectingRendererTargeting = new NDMFAffectingRendererTargeting(ctx, domainRenderers);
                    Profiler.EndSample();

                    Profiler.BeginSample("GetTargetGroupingWithAffecting");
                    targetGrouping = GetTargetGroupingWithAffecting(affectingRendererTargeting, behaviorIndex, domainRenderers);
                    Profiler.EndSample();
                }
                else
                {
                    Profiler.BeginSample("NDMFRendererTargeting ctr");
                    var rendererTargeting = new NDMFRendererTargeting(ctx, domainRenderers);
                    Profiler.EndSample();

                    Profiler.BeginSample("GetTargetGrouping");
                    targetGrouping = GetTargetGrouping(rendererTargeting, behaviorIndex, domainRenderers);
                    Profiler.EndSample();
                }
                Profiler.BeginSample("eval grouping");
                var renderersGroup2behavior = GetRendererGrouping(targetGrouping);
                Profiler.EndSample();

                Profiler.BeginSample("add to all groups");
                allGroups.AddRange(renderersGroup2behavior.Select(i => RenderGroup.For(i.Key).WithData(new PassingData(i.Value, domainRenderers, behaviorIndex))));
                Profiler.EndSample();
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

        private static Dictionary<TexTransBehavior, HashSet<Renderer>> GetTargetGrouping(IRendererTargeting rendererTargeting, Dictionary<TexTransBehavior, int> behaviorIndex, Renderer[] ofRenderers)
        {
            var behaviors = behaviorIndex.OrderBy(i => i.Value).Select(i => i.Key).ToArray();
            var targetRendererGroup = new Dictionary<TexTransBehavior, HashSet<Renderer>>();
            foreach (var ttbKV in behaviors)
            {
                Profiler.BeginSample("get ModificationTargetRenderers", ttbKV);
                var modificationTargets = ttbKV.ModificationTargetRenderers(rendererTargeting).ToHashSet();
                Profiler.EndSample();
                Profiler.BeginSample("register target group", ttbKV);
                targetRendererGroup.Add(ttbKV, modificationTargets);
                Profiler.EndSample();
            }
            return targetRendererGroup;
        }

        private static Dictionary<TexTransBehavior, HashSet<Renderer>> GetTargetGroupingWithAffecting(NDMFAffectingRendererTargeting affectingRendererTargeting, Dictionary<TexTransBehavior, int> behaviorIndex, Renderer[] ofRenderers)
        {
            var behaviors = behaviorIndex.OrderBy(i => i.Value).Select(i => i.Key).ToArray();
            var targetRendererGroup = new Dictionary<TexTransBehavior, HashSet<Renderer>>();
            foreach (var ttbKV in behaviors)
            {
                Profiler.BeginSample("get ModificationTargetRenderers", ttbKV);
                var modificationTargets = ttbKV.ModificationTargetRenderers(affectingRendererTargeting).ToHashSet();
                Profiler.EndSample();
                Profiler.BeginSample("Do AffectingRendererTargeting", ttbKV);
                if (ttbKV is IRendererTargetingAffecter affecter) affecter.AffectingRendererTargeting(affectingRendererTargeting);
                Profiler.EndSample();
                Profiler.BeginSample("register target group", ttbKV);
                targetRendererGroup.Add(ttbKV, modificationTargets);
                Profiler.EndSample();
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

        internal class NDMFAffectingRendererTargeting : IAffectingRendererTargeting, IDisposable
        {
            ComputeContext _ctx;
            Renderer[] _domainRenderers;
            Material[] _allMaterials;
            HashSet<Material> _allMaterialsHash;
            Dictionary<Renderer, Material?[]> _mutableMaterials;
            Dictionary<UnityEngine.Object, UnityEngine.Object> _replacing;
            public NDMFAffectingRendererTargeting(ComputeContext ctx, Renderer[] renderers)
            {
                _ctx = ctx;
                _domainRenderers = renderers;
                // _mutableMaterials = _domainRenderers.ToDictionary(i => i, i => i.sharedMaterials);
                _mutableMaterials = _domainRenderers.ToDictionary(i => i, i => _ctx.Observe(i, r => r.sharedMaterials));// すべてに対して Observe するの ... どうなんだろうね～?
                _allMaterials = _mutableMaterials.Values.SelectMany(i => i).Distinct().Where(i => i != null).Cast<Material>().ToArray();
                _allMaterialsHash = new(_allMaterials);

                var origin2Mutable = new Dictionary<Material, Material>();
                Profiler.BeginSample("get mutable from pool : count " + _allMaterials.Length);
                for (var i = 0; _allMaterials.Length > i; i += 1)
                {
                    var origin = _allMaterials[i];
                    _ctx.Observe(origin); // これは本当に必要だろうか ... ?
                    // LookAt(origin);
                    origin2Mutable[origin] = _allMaterials[i] = NDMFPreviewMaterialPool.Get(origin);
                }
                Profiler.EndSample();
                foreach (var rkv in _mutableMaterials)
                {
                    var matArray = rkv.Value;
                    for (var i = 0; rkv.Value.Length > i; i += 1)
                        if (matArray[i] != null)
                            matArray[i] = origin2Mutable[matArray[i]!];
                }
                _replacing = new();
                foreach (var o2m in origin2Mutable)
                    _replacing[o2m.Value] = o2m.Key;
            }
            public IEnumerable<Renderer> EnumerateRenderer() => _domainRenderers;

            public Material[] GetMutableMaterials(Renderer renderer) => _allMaterials;
            public Material[] GetMaterials(Renderer renderer) => _allMaterials;

            public bool OriginEqual(UnityEngine.Object l, UnityEngine.Object r)
            {
                if (l is Material mat && _replacing.ContainsKey(l)) l = _replacing[mat];
                if (r is Material mat2 && _replacing.ContainsKey(r)) r = _replacing[mat2];
                return l == r;
            }

            public void RegisterReplace(UnityEngine.Object oldObject, UnityEngine.Object nowObject)
            {
                _replacing[nowObject] = oldObject;
            }

            public void Dispose()
            {
                for (var i = 0; _allMaterials.Length > i; i += 1)
                {
                    NDMFPreviewMaterialPool.Ret(_allMaterials[i]);
                }
                _domainRenderers = null!;
                _allMaterials = null!;
                _mutableMaterials = null!;
                _replacing = null!;
            }
            public TOut LookAtGet<TObj, TOut>(TObj obj, Func<TObj, TOut> getAction, Func<TOut, TOut, bool>? comp = null)
            where TObj : UnityEngine.Object
            {
                if (obj is Material mat && _allMaterialsHash.Contains(mat)) { return getAction(obj); }//プールされているマテリアルを observe したらどうなるかわからないのでしない。
                return _ctx.Observe(obj, getAction, comp);
                // return getAction(obj);
            }
            public void LookAt(UnityEngine.Object obj)
            {
                if (obj is Material mat && _allMaterialsHash.Contains(mat)) { return; }
                _ctx.Observe(obj);
            }
            public void LookAtChildeComponents<LookTargetComponent>(GameObject gameObject) where LookTargetComponent : Component { _ctx.GetComponentsInChildren<LookTargetComponent>(gameObject, true); }
        }
        internal class NDMFRendererTargeting : IRendererTargeting
        {
            ComputeContext _ctx;
            Renderer[] _domainRenderers;
            private HashSet<Material>? _matHash;
            private HashSet<Texture>? _texHash;
            private Dictionary<Renderer, Material?[]> _sharedMaterialCache;

            public NDMFRendererTargeting(ComputeContext ctx, Renderer[] renderers)
            {
                _ctx = ctx;
                _domainRenderers = renderers;
                _sharedMaterialCache = new();
            }
            public IEnumerable<Renderer> EnumerateRenderer() { return _domainRenderers; }
            public bool OriginEqual(UnityEngine.Object l, UnityEngine.Object r) { return l == r; }

            public Material?[] GetMaterials(Renderer renderer)
            {
                if (_sharedMaterialCache.TryGetValue(renderer, out var mats)) { return mats; }
                mats = _sharedMaterialCache[renderer] = LookAtGet(renderer, GetShardMaterial);
                return mats;
                Material?[] GetShardMaterial(Renderer r) { return renderer.sharedMaterials; }
            }
            public Decal.MeshData GetMeshData(Renderer renderer) { return Decal.DecalContextUtility.GetToMemorizedMeshData(renderer); }
            public HashSet<Material> GetAllMaterials()
            {
                if (_matHash is null)
                {
                    _matHash = new HashSet<Material>();
                    foreach (var r in EnumerateRenderer()) { _matHash.UnionWith(GetMaterials(r).Where(m => m != null).Cast<Material>()); }
                }
                return _matHash;
            }
            public HashSet<Texture> GetAllTextures()
            {
                if (_texHash is null)
                {
                    _texHash = new HashSet<Texture>();
                    var mats = GetAllMaterials();
                    foreach (var m in mats) { _texHash.UnionWith(m.GetAllTexture(GetShader, GetTex)); }
                }
                return _texHash;
                Shader GetShader(Material mat) { return LookAtGet(mat, i => i.shader); }
                Texture GetTex(Material mat, int nameID) { return LookAtGet(mat, i => i.GetTexture(nameID)); }
            }
            public TOut LookAtGet<TObj, TOut>(TObj obj, Func<TObj, TOut> getAction, Func<TOut, TOut, bool>? comp = null)
            where TObj : UnityEngine.Object
            {
                return _ctx.Observe(obj, getAction, comp);
                // return getAction(obj);
            }
            public void LookAt(UnityEngine.Object obj) { _ctx.Observe(obj); }
            public void LookAtChildeComponents<LookTargetComponent>(GameObject gameObject) where LookTargetComponent : Component { _ctx.GetComponentsInChildren<LookTargetComponent>(gameObject, true); }
        }


    }
}
