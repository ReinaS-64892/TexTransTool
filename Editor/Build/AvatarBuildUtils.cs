using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using net.rs64.TexTransTool.ReferenceResolver;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace net.rs64.TexTransTool.Build
{
    internal class TexTransBuildSession
    {
        protected RenderersDomain _domain;
        protected Dictionary<TexTransPhase, List<TexTransBehavior>> _phaseAtList;


        public bool DisplayEditorProgressBar { get; set; } = false;

        public RenderersDomain Domain => _domain;
        public IReadOnlyDictionary<TexTransPhase, List<TexTransBehavior>> PhaseAtList => _phaseAtList;


        public TexTransBuildSession(RenderersDomain renderersDomain, Dictionary<TexTransPhase, List<TexTransBehavior>> phaseAtList)
        {
            _domain = renderersDomain;
            _phaseAtList = phaseAtList;
        }


        public void ApplyFor(TexTransPhase texTransPhase)
        {
            if (DisplayEditorProgressBar) EditorUtility.DisplayProgressBar(texTransPhase.ToString(), "", 0f);
            var count = 0;
            var timer = new System.Diagnostics.Stopwatch();
            foreach (var tf in _phaseAtList[texTransPhase])
            {
                if (DisplayEditorProgressBar) EditorUtility.DisplayProgressBar(texTransPhase.ToString(), $"{tf.name} - Apply", (float)count / _phaseAtList[texTransPhase].Count);

                timer.Restart();
                ApplyImpl(tf);
                timer.Stop();
                count += 1;
                Debug.Log($"{texTransPhase} : {tf.GetType().Name}:{tf.name} for Apply : {timer.ElapsedMilliseconds}ms");
            }
            if (DisplayEditorProgressBar) EditorUtility.ClearProgressBar();
        }

        protected virtual void ApplyImpl(TexTransBehavior tf)
        {
            TTTLog.ReportingObject(tf, () => { tf.Apply(_domain); });
        }

        public void MidwayMergeStack()
        {
            if (DisplayEditorProgressBar) EditorUtility.DisplayProgressBar("MidwayMergeStack", "", 0.0f);
            _domain.MergeStack();
            if (DisplayEditorProgressBar) EditorUtility.ClearProgressBar();
        }

        public void TTTSessionEnd()
        {
            if (DisplayEditorProgressBar) EditorUtility.DisplayProgressBar("TTTSessionEnd", "EditFinisher", 0.0f);
            _domain.EditFinish();
            if (DisplayEditorProgressBar) EditorUtility.ClearProgressBar();
        }
    }
    internal static class AvatarBuildUtils
    {

        public static bool ProcessAvatar(GameObject avatarGameObject, UnityEngine.Object OverrideAssetContainer = null, bool DisplayProgressBar = false)
        {
            try
            {
                var timer = Stopwatch.StartNew();

                var resolverContext = new ResolverContext(avatarGameObject);
                resolverContext.ResolvingFor(avatarGameObject.GetComponentsInChildren<AbstractResolver>());

                var domain = new AvatarDomain(avatarGameObject, false, new AssetSaver(OverrideAssetContainer));
                var session = new TexTransBuildSession(domain, FindAtPhase(avatarGameObject));
                session.DisplayEditorProgressBar = DisplayProgressBar;

                ExecuteAllPhase(session);
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
        public static void ExecuteAllPhase(TexTransBuildSession session)
        {
            session.ApplyFor(TexTransPhase.BeforeUVModification);

            session.MidwayMergeStack();

            session.ApplyFor(TexTransPhase.UVModification);
            session.ApplyFor(TexTransPhase.AfterUVModification);
            session.ApplyFor(TexTransPhase.UnDefined);

            session.MidwayMergeStack();

            session.ApplyFor(TexTransPhase.Optimizing);

            session.TTTSessionEnd();
        }


        public static void ResolvingFor(this ResolverContext resolverContext, IEnumerable<AbstractResolver> abstractResolvers)
        {
            foreach (var resolver in abstractResolvers)
            {
                resolver.Resolving(resolverContext);
            }
        }

        public static Dictionary<TexTransPhase, List<TexTransBehavior>> FindAtPhase(GameObject avatarGameObject)
        {
            var phaseDict = new Dictionary<TexTransPhase, List<TexTransBehavior>>(){
                    {TexTransPhase.BeforeUVModification,new List<TexTransBehavior>()},
                    {TexTransPhase.UVModification,new List<TexTransBehavior>()},
                    {TexTransPhase.AfterUVModification,new List<TexTransBehavior>()},
                    {TexTransPhase.UnDefined,new List<TexTransBehavior>()},
                    {TexTransPhase.Optimizing,new List<TexTransBehavior>()},
                };

            var phaseDefinitions = avatarGameObject.GetComponentsInChildren<PhaseDefinition>();
            var definedChildren = FindChildren(phaseDefinitions);
            var ContainsBy = new HashSet<TexTransBehavior>(definedChildren);

            var chileExcluders = avatarGameObject.GetComponentsInChildren<ITTTChildExclusion>();
            foreach (var ce in chileExcluders)
            {
                var cec = ce as Component;
                foreach (var tf in cec.GetComponentsInChildren<TexTransBehavior>(true))
                {
                    if (cec == tf) { continue; }
                    ContainsBy.Add(tf);
                }
            }

            foreach (var pd in phaseDefinitions)
            {
                if (!definedChildren.Contains(pd)) { phaseDict[pd.TexTransPhase].Add(pd); ContainsBy.Add(pd); }
            }

            foreach (var absTTG in avatarGameObject.GetComponentsInChildren<TexTransGroup>().Where(I => !ContainsBy.Contains(I)))
            { PhaseRegister(absTTG, phaseDict, ContainsBy); }

            foreach (var tf in TexTransGroup.TextureTransformerFilter(avatarGameObject.GetComponentsInChildren<TexTransBehavior>()))
            { if (!ContainsBy.Contains(tf)) { phaseDict[tf.PhaseDefine].Add(tf); } }

            return phaseDict;
        }
        static void PhaseRegister(TexTransGroup absTTG, Dictionary<TexTransPhase, List<TexTransBehavior>> phaseDict, HashSet<TexTransBehavior> containsBy)
        {
            if (containsBy.Contains(absTTG)) { return; }
            containsBy.Add(absTTG);
            foreach (var tf in TexTransGroup.TextureTransformerFilter(absTTG.Targets))
            {

                if (tf is TexTransGroup abstractTexTransGroup) { PhaseRegister(abstractTexTransGroup, phaseDict, containsBy); }
                else
                {
                    if (containsBy.Contains(tf)) { continue; }
                    phaseDict[tf.PhaseDefine].Add(tf);
                    containsBy.Add(tf);
                }
            }
        }
        public static void WhiteList(Dictionary<TexTransPhase, List<TexTransBehavior>> phase, HashSet<TexTransBehavior> whitelist)
        {
            foreach (var kv in phase) { kv.Value.RemoveAll(i => !whitelist.Contains(i)); }
        }
        public static IEnumerable<TexTransBehavior> PhaseDictFlatten(Dictionary<TexTransPhase, List<TexTransBehavior>> phaseDict)
        {
            foreach (var ttb in phaseDict[TexTransPhase.BeforeUVModification]) { if (ttb is PhaseDefinition pd) { foreach (var c in FindChildren(pd)) { yield return c; } } else { yield return ttb; } }
            foreach (var ttb in phaseDict[TexTransPhase.UVModification]) { if (ttb is PhaseDefinition pd) { foreach (var c in FindChildren(pd)) { yield return c; } } else { yield return ttb; } }
            foreach (var ttb in phaseDict[TexTransPhase.AfterUVModification]) { if (ttb is PhaseDefinition pd) { foreach (var c in FindChildren(pd)) { yield return c; } } else { yield return ttb; } }
            foreach (var ttb in phaseDict[TexTransPhase.UnDefined]) { if (ttb is PhaseDefinition pd) { foreach (var c in FindChildren(pd)) { yield return c; } } else { yield return ttb; } }
            foreach (var ttb in phaseDict[TexTransPhase.Optimizing]) { if (ttb is PhaseDefinition pd) { foreach (var c in FindChildren(pd)) { yield return c; } } else { yield return ttb; } }
        }

        public static HashSet<TexTransBehavior> FindChildren(TexTransGroup[] abstractTexTransGroups)
        {
            var children = new HashSet<TexTransBehavior>();
            foreach (var abstractTTG in abstractTexTransGroups)
            {
                children.UnionWith(FindChildren(abstractTTG));
            }
            return children;
        }
        public static HashSet<TexTransBehavior> FindChildren(TexTransGroup abstractTexTransGroup)
        {
            var children = new HashSet<TexTransBehavior>();
            children.UnionWith(abstractTexTransGroup.Targets);
            foreach (var tf in abstractTexTransGroup.Targets)
            {
                if (tf is TexTransGroup abstractTexTransGroupC) { children.UnionWith(FindChildren(abstractTexTransGroupC)); }
            }
            return children;
        }


        public static void DestroyITexTransToolTags(GameObject avatarGameObject)
        {
            foreach (var itttTag in avatarGameObject.GetComponentsInChildren<ITexTransToolTag>(true))
            {
                if (itttTag is not MonoBehaviour mb) { continue; }
                MonoBehaviour.DestroyImmediate(mb);
            }
        }
    }

}
