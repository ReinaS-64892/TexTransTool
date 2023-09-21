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

        public static bool ProcessAvatar(GameObject avatarGameObject, UnityEngine.Object OverrideAssetContainer = null, bool UseTemp = false)
        {
            try
            {
                if (OverrideAssetContainer == null && UseTemp) { AssetSaveHelper.IsTemporary = true; }

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

                foreach (var tf in avatarGameObject.GetComponentsInChildren<AbstractTexTransGroup>())
                {
                    if (!ContainsBy.Contains(tf))
                    {
                        phaseDict[tf.PhaseDefine].Add(tf);
                        ContainsBy.UnionWith(FindChildren(tf));
                    }
                }

                var singleTextureTransformer = new List<TextureTransformer>();

                foreach (var tf in avatarGameObject.GetComponentsInChildren<TextureTransformer>())
                {
                    if (!ContainsBy.Contains(tf))
                    {
                        phaseDict[tf.PhaseDefine].Add(tf);
                        singleTextureTransformer.Add(tf);
                    }
                }



                var domain = new AvatarDomain(avatarGameObject, previewing: false, saver: new AssetSaver(OverrideAssetContainer));

                foreach (var pd in phaseDict[TexTransPhase.BeforeUVModification])
                {
                    pd.Apply(domain);
                }

                foreach (var pd in phaseDict[TexTransPhase.UVModification])
                {
                    pd.Apply(domain);
                }

                foreach (var pd in phaseDict[TexTransPhase.AfterUVModification])
                {
                    pd.Apply(domain);
                }

                foreach (var pd in phaseDict[TexTransPhase.UnDefined])
                {
                    pd.Apply(domain);
                }

                domain.EditFinish();
                DestroySingleTextureTransformer(singleTextureTransformer);
                DestroyITexTransToolTagsForGameObject(avatarGameObject);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return false;
            }
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
                if (tf is AbstractTexTransGroup abstractTexTransGroupC)
                {
                    children.UnionWith(FindChildren(abstractTexTransGroupC));
                }
            }
            return children;
        }

        private static void DestroySingleTextureTransformer(List<TextureTransformer> singleTextureTransformer)
        {
            foreach (var tf in singleTextureTransformer)
            {
                if (tf != null)
                {
                    MonoBehaviour.DestroyImmediate(tf);
                }
            }
        }
        private static void DestroyITexTransToolTagsForGameObject(GameObject avatarGameObject)
        {
            foreach (var tf in avatarGameObject.GetComponentsInChildren<ITexTransToolTag>(true))
            {
                if (tf != null && tf is MonoBehaviour mb && mb != null && mb.gameObject != null)
                { MonoBehaviour.DestroyImmediate(mb.gameObject); }
            }
        }
    }
}
#endif
