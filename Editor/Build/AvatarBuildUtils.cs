#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using net.rs64.TexTransTool.ReferenceResolver;
using net.rs64.TexTransTool.Utils;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace net.rs64.TexTransTool.Build
{
    internal static class AvatarBuildUtils
    {

        public static bool ProcessAvatar(GameObject avatarGameObject, UnityEngine.Object OverrideAssetContainer = null, bool UseTemp = false, bool DisplayProgressBar = false)
        {
            try
            {
                if (OverrideAssetContainer == null && UseTemp) { AssetSaveHelper.IsTemporary = true; }
                var timer = Stopwatch.StartNew();

                var resolverContext = new ResolverContext(avatarGameObject);
                resolverContext.ResolvingFor(avatarGameObject.GetComponentsInChildren<AbstractResolver>());

                var session = new TexTransBuildSession(new AvatarDomain(avatarGameObject, false, new AssetSaver(OverrideAssetContainer), DisplayProgressBar));

                session.FindAtPhaseTTT();

                session.ApplyFor(TexTransPhase.BeforeUVModification);

                session.MidwayMergeStack();

                session.ApplyFor(TexTransPhase.UVModification);
                session.ApplyFor(TexTransPhase.AfterUVModification);
                session.ApplyFor(TexTransPhase.UnDefined);

                session.TTTSessionEnd();
                timer.Stop(); Debug.Log($"ProcessAvatarTime : {timer.ElapsedMilliseconds}ms");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return false;
            }


        }
        public static void ResolvingFor(this ResolverContext resolverContext, IEnumerable<AbstractResolver> abstractResolvers)
        {
            foreach (var resolver in abstractResolvers)
            {
                resolver.Resolving(resolverContext);
            }
        }
        public class TexTransBuildSession
        {
            AvatarDomain _avatarDomain;
            Dictionary<TexTransPhase, List<TexTransBehavior>> _phaseAtList;
            public AvatarDomain AvatarDomain => _avatarDomain;
            public Dictionary<TexTransPhase, List<TexTransBehavior>> PhaseAtList => _phaseAtList;

            public TexTransBuildSession(AvatarDomain avatarDomain, Dictionary<TexTransPhase, List<TexTransBehavior>> phaseAtList)
            {
                _avatarDomain = avatarDomain;
                _phaseAtList = phaseAtList;
            }
            public TexTransBuildSession(AvatarDomain avatarDomain)
            {
                _avatarDomain = avatarDomain;
            }
            public void FindAtPhaseTTT()
            {
                _phaseAtList = FindAtPhase(_avatarDomain.AvatarRoot);
            }

            public void ApplyFor(TexTransPhase texTransPhase)
            {
                _avatarDomain.ProgressStateEnter(texTransPhase.ToString());
                var count = 0;
                var timer = new System.Diagnostics.Stopwatch();
                foreach (var tf in _phaseAtList[texTransPhase])
                {
                    timer.Restart();
                    TTTLog.ReportingObject(tf, () => { tf.Apply(_avatarDomain); });
                    timer.Stop();
                    count += 1;
                    Debug.Log($"{texTransPhase} : {tf.GetType().Name}:{tf.name} for Apply : {timer.ElapsedMilliseconds}ms");
                    _avatarDomain.ProgressUpdate($"{tf.name} - Apply", (float)count / _phaseAtList[texTransPhase].Count);
                }
                _avatarDomain.ProgressStateExit();
            }

            public void MidwayMergeStack()
            {
                _avatarDomain.ProgressStateEnter("MidwayMergeStack");
                _avatarDomain.MergeStack();
                _avatarDomain.ProgressStateExit();
            }

            public void TTTSessionEnd()
            {
                _avatarDomain.EditFinish();
                DestroyITexTransToolTags(_avatarDomain.AvatarRoot);
            }
        }

        public static Dictionary<TexTransPhase, List<TexTransBehavior>> FindAtPhase(GameObject avatarGameObject)
        {
            var phaseDict = new Dictionary<TexTransPhase, List<TexTransBehavior>>(){
                    {TexTransPhase.UnDefined,new List<TexTransBehavior>()},
                    {TexTransPhase.BeforeUVModification,new List<TexTransBehavior>()},
                    {TexTransPhase.UVModification,new List<TexTransBehavior>()},
                    {TexTransPhase.AfterUVModification,new List<TexTransBehavior>()}
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
                if (!definedChildren.Contains(pd))
                {
                    phaseDict[pd.TexTransPhase].Add(pd);
                    ContainsBy.Add(pd);
                }
            }

            void PhaseRegister(TexTransGroup absTTG)
            {
                if (ContainsBy.Contains(absTTG)) { return; }
                ContainsBy.Add(absTTG);
                foreach (var tf in TexTransGroup.TextureTransformerFilter(absTTG.Targets))
                {

                    if (tf is TexTransGroup abstractTexTransGroup) { PhaseRegister(abstractTexTransGroup); }
                    else
                    {
                        if (ContainsBy.Contains(tf)) { continue; }
                        phaseDict[tf.PhaseDefine].Add(tf);
                        ContainsBy.Add(tf);
                    }
                }
            }
            foreach (var absTTG in avatarGameObject.GetComponentsInChildren<TexTransGroup>().Where(I => !ContainsBy.Contains(I)))
            {
                PhaseRegister(absTTG);
            }


            foreach (var tf in TexTransGroup.TextureTransformerFilter(avatarGameObject.GetComponentsInChildren<TexTransBehavior>()))
            {
                if (!ContainsBy.Contains(tf))
                {
                    phaseDict[tf.PhaseDefine].Add(tf);
                }
            }


            return phaseDict;
        }

        private static HashSet<TexTransBehavior> FindChildren(TexTransGroup[] abstractTexTransGroups)
        {
            var children = new HashSet<TexTransBehavior>();
            foreach (var abstractTTG in abstractTexTransGroups)
            {
                children.UnionWith(FindChildren(abstractTTG));
            }
            return children;
        }
        private static HashSet<TexTransBehavior> FindChildren(TexTransGroup abstractTexTransGroup)
        {
            var children = new HashSet<TexTransBehavior>();
            children.UnionWith(abstractTexTransGroup.Targets);
            foreach (var tf in abstractTexTransGroup.Targets)
            {
                if (tf is TexTransGroup abstractTexTransGroupC) { children.UnionWith(FindChildren(abstractTexTransGroupC)); }
            }
            return children;
        }


        private static void DestroyITexTransToolTags(GameObject avatarGameObject)
        {
            foreach (var tf in avatarGameObject.GetComponentsInChildren<ITexTransToolTag>(true))
            {
                if (!(tf != null && tf is MonoBehaviour mb && mb != null && mb.gameObject != null)) { continue; }
                if (mb.gameObject.GetComponents<Component>().Where(I => !(I is ITexTransToolTag) && !(I is Transform)).Count() == 0)
                {
                    MonoBehaviour.DestroyImmediate(mb.gameObject);
                }
                else
                {
                    MonoBehaviour.DestroyImmediate(mb);
                }
            }
        }
    }

}
#endif
