#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransTool.Utils;
using UnityEngine;

namespace net.rs64.TexTransTool.Build
{
    public static class AvatarBuildUtils
    {

        public static bool ProcessAvatar(GameObject avatarGameObject, UnityEngine.Object OverrideAssetContainer = null, bool UseTemp = false, bool DisplayProgressBar = false)
        {
            try
            {
                if (OverrideAssetContainer == null && UseTemp) { AssetSaveHelper.IsTemporary = true; }
                var session = new TexTransBuildSession(new AvatarDomain(avatarGameObject, previewing: false, saver: new AssetSaver(OverrideAssetContainer), DisplayProgressBar ? new ProgressHandler() : null), FindAtPhase(avatarGameObject));

                session.ApplyFor(TexTransPhase.BeforeUVModification);
                session.ApplyFor(TexTransPhase.UVModification);
                session.ApplyFor(TexTransPhase.AfterUVModification);
                session.ApplyFor(TexTransPhase.UnDefined);

                session.TTTSessionEnd();
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return false;
            }


        }

        public class TexTransBuildSession
        {
            AvatarDomain _avatarDomain;
            Dictionary<TexTransPhase, List<TextureTransformer>> _phaseAtList;
            public AvatarDomain AvatarDomain => _avatarDomain;
            public Dictionary<TexTransPhase, List<TextureTransformer>> PhaseAtList => _phaseAtList;

            public TexTransBuildSession(AvatarDomain avatarDomain, Dictionary<TexTransPhase, List<TextureTransformer>> phaseAtList)
            {
                _avatarDomain = avatarDomain;
                _phaseAtList = phaseAtList;
            }
            public TexTransBuildSession(AvatarDomain avatarDomain)
            {
                _avatarDomain = avatarDomain;
                _phaseAtList = FindAtPhase(_avatarDomain.AvatarRoot);
            }
            public void ApplyFor(TexTransPhase texTransPhase)
            {
                _avatarDomain.ProgressStateEnter(texTransPhase.ToString());
                var count = 0;
                foreach (var tf in _phaseAtList[texTransPhase])
                {
                    Debug.Log($"{texTransPhase} : {tf.GetType().Name}:{tf.name} for Apply");
                    tf.Apply(_avatarDomain);
                    count += 1;
                    _avatarDomain.ProgressUpdate($"{tf.name} - Apply", (float)count / _phaseAtList[texTransPhase].Count);
                }
                _avatarDomain.ProgressStateExit();
            }

            public void TTTSessionEnd()
            {
                _avatarDomain.EditFinish();
                DestroyITexTransToolTags(_avatarDomain.AvatarRoot);
            }
        }

        public static Dictionary<TexTransPhase, List<TextureTransformer>> FindAtPhase(GameObject avatarGameObject)
        {
            var phaseDict = new Dictionary<TexTransPhase, List<TextureTransformer>>(){
                    {TexTransPhase.UnDefined,new List<TextureTransformer>()},
                    {TexTransPhase.BeforeUVModification,new List<TextureTransformer>()},
                    {TexTransPhase.UVModification,new List<TextureTransformer>()},
                    {TexTransPhase.AfterUVModification,new List<TextureTransformer>()}
                };

            var phaseDefinitions = avatarGameObject.GetComponentsInChildren<PhaseDefinition>();
            var definedChildren = FindChildren(phaseDefinitions);
            var ContainsBy = new HashSet<TextureTransformer>(definedChildren);

            foreach (var pd in phaseDefinitions)
            {
                if (!definedChildren.Contains(pd))
                {
                    phaseDict[pd.TexTransPhase].Add(pd);
                    ContainsBy.Add(pd);
                }
            }

            void PhaseRegister(AbstractTexTransGroup absTTG)
            {
                if (ContainsBy.Contains(absTTG)) { return; }
                ContainsBy.Add(absTTG);
                foreach (var tf in AbstractTexTransGroup.TextureTransformerFilter(absTTG.Targets))
                {

                    if (tf is AbstractTexTransGroup abstractTexTransGroup) { PhaseRegister(abstractTexTransGroup); }
                    else
                    {
                        if (ContainsBy.Contains(tf)) { continue; }
                        phaseDict[tf.PhaseDefine].Add(tf);
                        ContainsBy.Add(tf);
                    }
                }
            }
            foreach (var absTTG in avatarGameObject.GetComponentsInChildren<AbstractTexTransGroup>().Where(I => !ContainsBy.Contains(I)))
            {
                PhaseRegister(absTTG);
            }


            foreach (var tf in AbstractTexTransGroup.TextureTransformerFilter(avatarGameObject.GetComponentsInChildren<TextureTransformer>()))
            {
                if (!ContainsBy.Contains(tf))
                {
                    phaseDict[tf.PhaseDefine].Add(tf);
                }
            }

            return phaseDict;
        }

        private static HashSet<TextureTransformer> FindChildren(AbstractTexTransGroup[] abstractTexTransGroups)
        {
            var children = new HashSet<TextureTransformer>();
            foreach (var abstractTTG in abstractTexTransGroups)
            {
                children.UnionWith(FindChildren(abstractTTG));
            }
            return children;
        }
        private static HashSet<TextureTransformer> FindChildren(AbstractTexTransGroup abstractTexTransGroup)
        {
            var children = new HashSet<TextureTransformer>();
            children.UnionWith(abstractTexTransGroup.Targets);
            foreach (var tf in abstractTexTransGroup.Targets)
            {
                if (tf is AbstractTexTransGroup abstractTexTransGroupC) { children.UnionWith(FindChildren(abstractTexTransGroupC)); }
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
