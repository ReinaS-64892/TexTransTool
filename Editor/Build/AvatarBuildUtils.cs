#nullable enable
using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;
using UnityEngine.Profiling;
using System.Runtime.CompilerServices;
using System.Reflection;
using net.rs64.TexTransTool.Utils;

namespace net.rs64.TexTransTool.Build
{
    internal class TexTransBuildSession
    {
        protected GameObject? _domainRoot;
        protected RenderersDomain _domain;
        protected Dictionary<TexTransPhase, List<TexTransBehavior>> _phase2Behaviors;

        public bool DisplayEditorProgressBar { get; set; } = false;

        public RenderersDomain Domain => _domain;
        public IReadOnlyDictionary<TexTransPhase, List<TexTransBehavior>> PhaseAtList => _phase2Behaviors;


        public TexTransBuildSession(GameObject? domainRoot, RenderersDomain renderersDomain, Dictionary<TexTransPhase, List<TexTransBehavior>> phase2Behaviors)
        {
            _domainRoot = domainRoot;
            _domain = renderersDomain;
            _phase2Behaviors = phase2Behaviors;
        }


        public void ApplyFor(TexTransPhase texTransPhase)
        {
            if (DisplayEditorProgressBar) EditorUtility.DisplayProgressBar(texTransPhase.ToString(), "", 0f);
            using (var pf = new PFScope("ApplyFor-" + texTransPhase))
            {
                var count = 0;
                var timer = new System.Diagnostics.Stopwatch();
                var behaviors = _phase2Behaviors[texTransPhase];
                var behaviorsCount = behaviors.Count;
                foreach (var tf in behaviors)
                {
                    if (AvatarBuildUtils.CheckIsActiveBehavior(tf, _domainRoot) is false) { continue; }
                    if (DisplayEditorProgressBar) EditorUtility.DisplayProgressBar(texTransPhase.ToString(), $"{tf.name} - Apply", (float)count / behaviorsCount);
                    using var apf = new PFScope("Apply-" + tf.gameObject.name, tf);

                    timer.Restart();


                    ApplyImpl(tf, _domain);


                    timer.Stop();
                    count += 1;
                    Debug.Log($"{texTransPhase} : {tf.GetType().Name}:{tf.name} for Apply : {timer.ElapsedMilliseconds}ms");
                }

                if (DisplayEditorProgressBar) EditorUtility.DisplayProgressBar("MidwayMergeStack", "", 0.0f);


                pf.Split("MidwayMergeStack");

                _domain.MergeStack();
                _domain.ReadBackToTexture2D();
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
            _domain.Dispose();
#if TTT_TTCE_TRACING
            if (_domain.GetTexTransCoreEngineForUnity() is TTCEWithTTT4UInterfaceDebug debug)
            { debug.Dispose(); }
#endif
            if (DisplayEditorProgressBar) EditorUtility.ClearProgressBar();
        }
    }
    internal static class AvatarBuildUtils
    {

        public static bool ProcessAvatar(GameObject avatarGameObject, bool DisplayProgressBar = false)
        {
            return ProcessAvatar(avatarGameObject, new AssetSaver(), DisplayProgressBar);
        }
        public static bool ProcessAvatar(GameObject avatarGameObject, IAssetSaver assetSaver, bool DisplayProgressBar = false)
        {
            try
            {
                var timer = Stopwatch.StartNew();

                AvatarDomain domain;
                switch (TTTProjectConfig.instance.TexTransCoreEngineBackend)
                {

#if CONTAINS_TTCE_WGPU
                    case TTTProjectConfig.TexTransCoreEngineBackendEnum.Wgpu:
                        {
                            var wgpuCtx = TTCEWgpuDeviceWithTTT4UnityHolder.Device().GetTTCEWgpuContext();
                            domain = new AvatarDomain(avatarGameObject, assetSaver, null, wgpuCtx);
                            break;
                        }
#endif

                    default:
                    case TTTProjectConfig.TexTransCoreEngineBackendEnum.Unity:
                        {
                            domain = new AvatarDomain(avatarGameObject, assetSaver);
                            break;
                        }
                }
                
                var domain2Phase = FindAtPhase(avatarGameObject);
                var session = new TexTransBuildSession(avatarGameObject, domain, domain2Phase);
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
            finally { EditorUtility.ClearProgressBar(); }
        }
        public static void ExecuteAllPhaseAndEnd(TexTransBuildSession session)
        {
            ExeCuteAllPhase(session);
            session.TTTSessionEnd();
        }

        public static void ExeCuteAllPhase(TexTransBuildSession session)
        {
            foreach (var phase in TexTransPhaseUtility.EnumerateAllPhase())
                session.ApplyFor(phase);
        }
        public interface IGameObjectWakingTool
        {
            C[] GetComponentsInChildren<C>(GameObject gameObject, bool includeInactive) where C : Component;
            C GetComponent<C>(GameObject gameObject) where C : Component;
            int GetChilesCount(GameObject gameObject);
            GameObject? GetChilde(GameObject gameObject, int index);
        }
        public interface IGameObjectActivenessWakingTool
        {
            C GetComponent<C>(GameObject gameObject) where C : Component;
            GameObject? GetParent(GameObject gameObject);
            bool ActiveSelf(GameObject gameObject);
        }
        public struct DefaultGameObjectWakingTool : IGameObjectWakingTool, IGameObjectActivenessWakingTool
        {

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public GameObject? GetChilde(GameObject gameObject, int index)
            {
                return gameObject.transform.GetChild(index)?.gameObject;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int GetChilesCount(GameObject gameObject)
            {
                return gameObject.transform.childCount;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public C GetComponent<C>(GameObject gameObject) where C : Component
            {
                return gameObject.GetComponent<C>();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public C[] GetComponentsInChildren<C>(GameObject gameObject, bool includeInactive) where C : Component
            {
                return gameObject.GetComponentsInChildren<C>(includeInactive);
            }

            public GameObject? GetParent(GameObject gameObject)
            {
                return gameObject.transform.parent?.gameObject;
            }
            public bool ActiveSelf(GameObject gameObject)
            {
                return gameObject.activeSelf;
            }
        }
        /*
        基本としてグループアノテーションは一番上にいる存在が一番強く、その配下のグループアノテーションは効果を発揮できない

        アバタールートにつけたら破綻することは当然なのでしないように。

        あと無効なTTBがこの時点では消えないから注意ね

        それと TexTransSequencing を継承する存在、その影響は配下すべて(再帰的)に影響する。
        */
        public static Dictionary<TexTransPhase, List<TexTransBehavior>> FindAtPhase(GameObject rootDomainObject)
        { return FindAtPhase(rootDomainObject, new DefaultGameObjectWakingTool()); }
        public static Dictionary<TexTransPhase, List<TexTransBehavior>> FindAtPhase<WakingTool>(GameObject rootDomainObject, WakingTool wakingTool)
        where WakingTool : IGameObjectWakingTool
        {
            var behavior = new CorrectingResult();
            Correct(behavior, rootDomainObject, wakingTool);

            var phasedBehaviour = TexTransPhaseUtility.GeneratePhaseDictionary<List<TexTransBehavior>>();

            foreach (var pd in behavior.PhaseDefinitions)
                GroupedComponentsCorrect(phasedBehaviour[pd.TexTransPhase], pd.gameObject, wakingTool);


            var ttbList = new List<TexTransBehavior>();
            foreach (var ttg in behavior.TexTransGroups)
            {
                ttbList.Clear();
                GroupedComponentsCorrect(ttbList, ttg.gameObject, wakingTool);

                foreach (var ttb in ttbList)
                    phasedBehaviour[ttb.PhaseDefine].Add(ttb);
            }


            foreach (var ttb in behavior.OtherBehaviors)
                phasedBehaviour[ttb.PhaseDefine].Add(ttb);

            return phasedBehaviour;
        }
        class CorrectingResult
        {
            public List<PhaseDefinition> PhaseDefinitions = new();
            public List<TexTransGroup> TexTransGroups = new();
            public List<TexTransBehavior> OtherBehaviors = new();
        }

        static void Correct<WakingTool>(CorrectingResult wakingResult, GameObject wakingPoint, WakingTool wakingTool)
        where WakingTool : IGameObjectWakingTool
        {
            var chilesCount = wakingTool.GetChilesCount(wakingPoint);
            for (var i = 0; chilesCount > i; i += 1)
            {
                var cObject = wakingTool.GetChilde(wakingPoint, i)!;
                var ownedComponent = wakingTool.GetComponent<TexTransMonoBaseGameObjectOwned>(cObject);

                if (ownedComponent != null)
                    switch (ownedComponent)
                    {
                        default: { break; }

                        case PhaseDefinition pd:
                            {
                                wakingResult.PhaseDefinitions.Add(pd);
                                break;
                            }
                        case TexTransGroup ttg:
                            {
                                wakingResult.TexTransGroups.Add(ttg);
                                break;
                            }
                        case TexTransBehavior ttb:
                            {
                                wakingResult.OtherBehaviors.Add(ttb);
                                break;
                            }
                    }
                else
                    Correct(wakingResult, cObject, wakingTool);
            }
        }

        internal static void GroupedComponentsCorrect<WakingTool>(List<TexTransBehavior> behaviors, GameObject wakingPoint, WakingTool wakingTool)
        where WakingTool : IGameObjectWakingTool
        {
            var chilesCount = wakingTool.GetChilesCount(wakingPoint);
            for (var i = 0; chilesCount > i; i += 1)
            {
                var cObject = wakingTool.GetChilde(wakingPoint, i)!;
                var component = wakingTool.GetComponent<TexTransBehavior>(cObject);

                if (component != null) behaviors.Add(component);
                else GroupedComponentsCorrect(behaviors, cObject, wakingTool);
            }
        }
        public static bool CheckIsActiveBehavior(TexTransBehavior behavior, GameObject? domainRoot = null)
        { return CheckIsActiveBehavior(behavior, new DefaultGameObjectWakingTool(), domainRoot); }
        public static bool CheckIsActiveBehavior<WakingTool>(TexTransBehavior behavior, WakingTool wakingTool, GameObject? domainRoot = null)
        where WakingTool : IGameObjectActivenessWakingTool
        {
            var wakingPoint = behavior.gameObject;
            while (wakingPoint != domainRoot)
            {
                if (wakingTool.ActiveSelf(wakingPoint) is false) { return false; }
                if (wakingTool.GetComponent<IsActiveInheritBreaker>(wakingPoint) != null) { return true; }

                wakingPoint = wakingTool.GetParent(wakingPoint);

                if (wakingPoint == null) { break; }// safety
            }
            return true;
        }
        public static void DestroyITexTransToolTags(GameObject avatarGameObject)
        {
            foreach (var itttTag in avatarGameObject.GetComponentsInChildren<ITexTransToolTag>(true))
            {
                if (itttTag is not MonoBehaviour mb) { continue; }
                if (mb == null) { continue; }
                RemoveDependent(mb.gameObject, mb);
                MonoBehaviour.DestroyImmediate(mb);
            }
        }

        static Dictionary<Type, HashSet<Type>> s_requireComponentCache = new();
        private static void RemoveDependent(GameObject gameObject, Component component)
        {
            var removeTargetType = component.GetType();
            foreach (var mayDependent in gameObject.GetComponents<Component>())
            {
                if (mayDependent == component) { continue; }
                if (GetRequireComponent(mayDependent.GetType()).Any(t => t.IsAssignableFrom(removeTargetType)) is false) { continue; }
                var dependent = mayDependent;

                if (dependent is not ITexTransToolTag dpTTTComponent) { continue; }
                if (dependent is not Component dpTTTComponent2) { continue; }
                if (dpTTTComponent2 == null) { continue; }
                RemoveDependent(gameObject, dpTTTComponent2);
                Component.DestroyImmediate(dpTTTComponent2);
            }

        }
        private static HashSet<Type> GetRequireComponent(Type type)
        {
            if (s_requireComponentCache.TryGetValue(type, out var rqTypes)) { return rqTypes; }
            s_requireComponentCache[type] = rqTypes = new();
            foreach (var rc in type.GetCustomAttributes<RequireComponent>(true))
            {
                if (rc.m_Type0 != null) rqTypes.Add(rc.m_Type0);
                if (rc.m_Type1 != null) rqTypes.Add(rc.m_Type1);
                if (rc.m_Type2 != null) rqTypes.Add(rc.m_Type2);
            }
            return rqTypes;
        }

        internal static IEnumerable<TexTransRuntimeBehavior> PhaseDictFlatten(Dictionary<TexTransPhase, List<TexTransBehavior>> behaviors)
        {
            foreach (var phase in TexTransPhaseUtility.EnumerateAllPhase())
                foreach (var behavior in behaviors[phase].OfType<TexTransRuntimeBehavior>()) { yield return behavior; }
        }
    }

}
