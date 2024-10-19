using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace net.rs64.TexTransTool.Build
{
    internal class TexTransBuildSession
    {
        protected RenderersDomain _domain;
        protected List<Domain2Behavior> _domain2Phase;
        protected Dictionary<DomainDefinition, List<Renderer>> _subDomainRenderers;

        public bool DisplayEditorProgressBar { get; set; } = false;

        public RenderersDomain Domain => _domain;
        public IReadOnlyList<Domain2Behavior> PhaseAtList => _domain2Phase;


        public TexTransBuildSession(RenderersDomain renderersDomain, List<Domain2Behavior> phaseAtList, Dictionary<DomainDefinition, List<Renderer>> subDomainRenderers = null)
        {
            _domain = renderersDomain;
            _domain2Phase = phaseAtList;
            _subDomainRenderers = subDomainRenderers ?? phaseAtList.Where(i => i.Domain != null).ToDictionary(i => i.Domain, i => i.Domain.GetComponentsInChildren<Renderer>(true).ToList());
        }


        public void ApplyFor(TexTransPhase texTransPhase)
        {
            if (DisplayEditorProgressBar) EditorUtility.DisplayProgressBar(texTransPhase.ToString(), "", 0f);
            var count = 0;
            var timer = new System.Diagnostics.Stopwatch();
            foreach (var domains in _domain2Phase)
            {
                IDomain domain = domains.Domain is null ? _domain : _domain.GetSubDomain(_subDomainRenderers[domains.Domain]);
                var behaviorCount = domains.Behaviour[texTransPhase].Count;
                foreach (var tf in domains.Behaviour[texTransPhase])
                {
                    if (tf.CheckIsActiveBehavior() is false) { continue; }
                    if (DisplayEditorProgressBar) EditorUtility.DisplayProgressBar(texTransPhase.ToString(), $"{tf.name} - Apply", (float)count / behaviorCount);

                    timer.Restart();
                    ApplyImpl(tf, domain);
                    timer.Stop();
                    count += 1;
                    Debug.Log($"{texTransPhase} : {tf.GetType().Name}:{tf.name} for Apply : {timer.ElapsedMilliseconds}ms");
                }

                if (DisplayEditorProgressBar) EditorUtility.DisplayProgressBar("MidwayMergeStack", "", 0.0f);
                if (domain is RenderersDomain rd) { rd.MergeStack(); }
                if (domain is RenderersDomain.RenderersSubDomain rsd) { rsd.MergeStack(); }
                if (DisplayEditorProgressBar) EditorUtility.ClearProgressBar();
            }
            if (DisplayEditorProgressBar) EditorUtility.ClearProgressBar();
        }

        protected virtual void ApplyImpl(TexTransBehavior tf, IDomain domain)
        {
            TTTLog.ReportingObject(tf, () => { tf.Apply(domain); });
        }

        public void TTTSessionEnd()
        {
            if (DisplayEditorProgressBar) EditorUtility.DisplayProgressBar("TTTSessionEnd", "EditFinisher", 0.0f);
            _domain.EditFinish();
            if (DisplayEditorProgressBar) EditorUtility.ClearProgressBar();
        }
    }
    internal class Domain2Behavior
    {
        public DomainDefinition Domain = null;//null だったら Root ってことで
        public Dictionary<TexTransPhase, List<TexTransBehavior>> Behaviour = new(){
                {TexTransPhase.BeforeUVModification,new List<TexTransBehavior>()},
                {TexTransPhase.UVModification,new List<TexTransBehavior>()},
                {TexTransPhase.AfterUVModification,new List<TexTransBehavior>()},
                {TexTransPhase.UnDefined,new List<TexTransBehavior>()},
                {TexTransPhase.Optimizing,new List<TexTransBehavior>()},
            };
    }
    internal static class AvatarBuildUtils
    {

        public static bool ProcessAvatar(GameObject avatarGameObject, UnityEngine.Object OverrideAssetContainer = null, bool DisplayProgressBar = false)
        {
            try
            {
                var timer = Stopwatch.StartNew();

                var domain = new AvatarDomain(avatarGameObject, false, new AssetSaver(OverrideAssetContainer));
                var domain2Phase = FindAtPhase(avatarGameObject);
                var session = new TexTransBuildSession(domain, domain2Phase);
                session.DisplayEditorProgressBar = DisplayProgressBar;

                ExecuteAllPhaseAndEnd(session);

                DestroyITexTransToolTags(avatarGameObject);
                timer.Stop(); Debug.Log($"ProcessAvatarTime : {timer.ElapsedMilliseconds}ms");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return false;
            }
        }
        public static void ExecuteAllPhaseAndEnd(TexTransBuildSession session)
        {
            ExeCuteAllPhase(session);
            session.TTTSessionEnd();
        }

        public static void ExeCuteAllPhase(TexTransBuildSession session)
        {
            session.ApplyFor(TexTransPhase.BeforeUVModification);
            session.ApplyFor(TexTransPhase.UVModification);
            session.ApplyFor(TexTransPhase.AfterUVModification);
            session.ApplyFor(TexTransPhase.UnDefined);
            session.ApplyFor(TexTransPhase.Optimizing);
        }

        /*
        基本としてグループアノテーションは一番上にいる存在が一番強く、その配下のグループアノテーションは効果を発揮できない

        アバタールートにつけたら破綻することは当然なのでしないように。

        あと無効なTTB がこの時点では消えないから注意ね
        */

        public delegate Component[] GetComponentsInChildren(Type type, GameObject gameObject, bool includeInactive);
        public delegate Component GetComponent(Type type, GameObject gameObject);
        public static List<Domain2Behavior> FindAtPhase(GameObject rootDomainObject, GetComponent getComponent = null, GetComponentsInChildren getComponentsInChildren = null)
        {
            getComponent ??= (t, g) => g.GetComponent(t);
            getComponentsInChildren ??= (t, g, i) => g.GetComponentsInChildren(t, i);
            var rootTree = new RootBehaviorTree();
            FindDomainsTexTransBehavior(rootTree.Behaviors, rootTree.ChildeTrees, rootDomainObject.transform);

            var domainTreeList = new List<DomainTree>();
            var rootDefine = getComponent(typeof(DomainDefinition), rootDomainObject);
            foreach (var sudDomain in getComponentsInChildren(typeof(DomainDefinition), rootDomainObject, true).OfType<DomainDefinition>().Where(d => d != rootDefine))
            {
                var point = sudDomain.transform.parent;
                while (getComponent(typeof(DomainDefinition), point.gameObject) == null && point.gameObject != rootDomainObject)
                {
                    point = point.parent;
                }

                var dt = new DomainTree();
                dt.ParentDomain = point.gameObject;
                dt.DomainPoint = sudDomain;
                domainTreeList.Add(dt);

                FindDomainsTexTransBehavior(dt.BehaviorTree.Behaviors, dt.BehaviorTree.ChildeTrees, dt.DomainPoint.transform, getComponent);
            }

            for (var i = 0; domainTreeList.Count > i; i += 1)
            {
                var sd = domainTreeList[i];

                var depth = 0;
                var wt = sd;
                while (wt.ParentDomain != rootDomainObject)
                {
                    wt = domainTreeList.Find(i => i.DomainPoint.gameObject == wt.ParentDomain.gameObject);
                    depth += 1;
                }

                sd.Depth = depth;
            }

            domainTreeList.Sort((l, r) => r.Depth - l.Depth);//深いほうが先に並ぶようにする

            var domainList = new List<Domain2Behavior>();

            foreach (var domainTree in domainTreeList)
            {
                var d2b = new Domain2Behavior();

                d2b.Domain = domainTree.DomainPoint;
                RegisterDomain2Behavior(d2b, domainTree.BehaviorTree.ChildeTrees, domainTree.BehaviorTree.Behaviors);

                domainList.Add(d2b);
            }

            var rootDomain2Behavior = new Domain2Behavior();
            RegisterDomain2Behavior(rootDomain2Behavior, rootTree.ChildeTrees, rootTree.Behaviors);
            domainList.Add(rootDomain2Behavior);

            return domainList;
        }

        internal static void RegisterDomain2Behavior(Domain2Behavior d2b, List<BehaviorTree> behaviorTrees, List<TexTransBehavior> behaviors)
        {
            foreach (var ct in behaviorTrees)
            {
                if (ct.TreePoint is PhaseDefinition pd)
                {
                    d2b.Behaviour[pd.TexTransPhase].AddRange(ct.Behaviors);
                }
            }

            foreach (var ct in behaviorTrees)
            {
                if (ct.TreePoint is not PhaseDefinition)
                {
                    foreach (var ttb in ct.Behaviors)
                    {
                        d2b.Behaviour[ttb.PhaseDefine].Add(ttb);
                    }
                }
            }
            foreach (var ttb in behaviors)
            {
                d2b.Behaviour[ttb.PhaseDefine].Add(ttb);
            }
        }

        public static bool CheckIsActiveBehavior(this TexTransBehavior behavior)
        {
            var activenessChanger = behavior.GetComponentInParent<IActivenessChanger>(true);
            if (activenessChanger == null) { return behavior.gameObject.activeInHierarchy; }

            var activenessChangerTransform = ((Component)activenessChanger).transform;
            var warkPoint = behavior.transform;
            var state = true;
            while (warkPoint != activenessChangerTransform)
            {
                state &= warkPoint.gameObject.activeSelf;
                warkPoint = warkPoint.parent;
            }
            state &= activenessChanger.IsActive;

            return state;
        }

        internal class DomainTree
        {
            public GameObject ParentDomain = null;
            public DomainDefinition DomainPoint = null;
            public RootBehaviorTree BehaviorTree = new();

            public int Depth = 0;
        }
        internal class RootBehaviorTree
        {
            public List<TexTransBehavior> Behaviors = new();
            public List<BehaviorTree> ChildeTrees = new();
        }
        internal class BehaviorTree
        {
            public TexTransGroup TreePoint = null;
            public List<TexTransBehavior> Behaviors = new();
        }
        internal static void FindDomainsTexTransBehavior(List<TexTransBehavior> behaviors, List<BehaviorTree> chilesTree, Transform entryPoint, GetComponent getComponent = null)
        {
            getComponent ??= (t, g) => g.GetComponent(t);
            var chilesCount = entryPoint.childCount;
            for (var i = 0; chilesCount > i; i += 1)
            {
                var c = entryPoint.GetChild(i);
                var ttc = c.GetComponent<TexTransMonoBase>();

                if (ttc is DomainDefinition) { continue; }
                if (ttc is TexTransGroup ttg)
                {
                    var nTree = new BehaviorTree();
                    nTree.TreePoint = ttg;
                    FindTreedBehavior(nTree.Behaviors, nTree.TreePoint.transform, getComponent);
                    chilesTree.Add(nTree);
                    continue;
                }
                if (ttc is TexTransBehavior ttb)
                {
                    behaviors.Add(ttb);
                    continue;
                }


                FindDomainsTexTransBehavior(behaviors, chilesTree, c);
            }
        }

        internal static void FindTreedBehavior(List<TexTransBehavior> behaviors, Transform entryPoint, GetComponent getComponent = null)
        {
            getComponent ??= (t, g) => g.GetComponent(t);
            var chilesCount = entryPoint.childCount;
            for (var i = 0; chilesCount > i; i += 1)
            {
                var c = entryPoint.GetChild(i);
                var ttc = getComponent(typeof(TexTransMonoBase), c.gameObject);
                if (ttc is DomainDefinition) { continue; }
                if (ttc is TexTransBehavior ttb) { behaviors.Add(ttb); }

                FindTreedBehavior(behaviors, c, getComponent);
            }
        }


        public static void DestroyITexTransToolTags(GameObject avatarGameObject)
        {
            foreach (var itttTag in avatarGameObject.GetComponentsInChildren<ITexTransToolTag>(true))
            {
                if (itttTag is not MonoBehaviour mb) { continue; }
                MonoBehaviour.DestroyImmediate(mb);
            }
        }

        internal static IEnumerable<TexTransRuntimeBehavior> PhaseDictFlatten(List<Domain2Behavior> domain2Behaviors)
        {
            foreach (var domain in domain2Behaviors)
                foreach (var behavior in domain.Behaviour[TexTransPhase.BeforeUVModification].OfType<TexTransRuntimeBehavior>()) { yield return behavior; }
            foreach (var domain in domain2Behaviors)
                foreach (var behavior in domain.Behaviour[TexTransPhase.UVModification].OfType<TexTransRuntimeBehavior>()) { yield return behavior; }
            foreach (var domain in domain2Behaviors)
                foreach (var behavior in domain.Behaviour[TexTransPhase.AfterUVModification].OfType<TexTransRuntimeBehavior>()) { yield return behavior; }
            foreach (var domain in domain2Behaviors)
                foreach (var behavior in domain.Behaviour[TexTransPhase.UnDefined].OfType<TexTransRuntimeBehavior>()) { yield return behavior; }
            foreach (var domain in domain2Behaviors)
                foreach (var behavior in domain.Behaviour[TexTransPhase.Optimizing].OfType<TexTransRuntimeBehavior>()) { yield return behavior; }

        }

        internal static void FindTreedBehavior(object groupBhaviors, Transform transform)
        {
            throw new NotImplementedException();
        }
    }

}
