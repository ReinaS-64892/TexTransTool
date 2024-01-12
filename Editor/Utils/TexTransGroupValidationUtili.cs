#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransTool.Decal;
using net.rs64.TexTransTool.TextureAtlas;
using net.rs64.TexTransTool.MatAndTexUtils;

namespace net.rs64.TexTransTool
{
    internal static class TexTransGroupValidationUtils
    {
        public static void ValidateTexTransGroup(TexTransGroup texTransGroup)
        {
            var tTFs = TexTransGroup.TextureTransformerFilter(texTransGroup.Targets);
            var renderersPeaTTFsDict = new Dictionary<Renderer, List<TexTransBehavior>>();
            CollectTexTransForms(tTFs, renderersPeaTTFsDict);

            var warnTarget = new List<TexTransBehavior>();

            foreach (var rendererPadTTF in renderersPeaTTFsDict)
            {
                var allowSeparateFlag = true;
                foreach (var tf in rendererPadTTF.Value)
                {
                    switch (tf)
                    {
                        case MatAndTexAbsoluteSeparator matAndTexSeparator:
                            {
                                if (!allowSeparateFlag) { warnTarget.Add(matAndTexSeparator); }
                                break;
                            }
                        case MatAndTexRelativeSeparator matAndTexRelativeSeparator:
                            {
                                if (!allowSeparateFlag) { warnTarget.Add(matAndTexRelativeSeparator); }
                                break;
                            }
                        case AtlasTexture atlasTexture:
                            {
                                if (!allowSeparateFlag) { warnTarget.Add(atlasTexture); }
                                break;
                            }
                        default: { break; }

                    }
                }
            }

            foreach (var tf in warnTarget.Distinct())
            {
                switch (tf)
                {
                    case AbstractDecal abstractDecal:
                        {
                            Debug.LogWarning($"{abstractDecal.GetType().Name} : {abstractDecal.name} のIsSeparateMatAndTextureは、先に張り付けられたデカールを完全に消してしまう可能性があります。");
                            break;
                        }
                    case AtlasTexture atlasTexture:
                        {
                            Debug.LogWarning($"AtlasTexture : {atlasTexture.name} は、先に張り付けられたデカールを完全に消す可能性があります。");
                            break;
                        }
                    default: { break; }
                }

            }

        }

        private static void CollectTexTransForms(IEnumerable<TexTransBehavior> tTFs, Dictionary<Renderer, List<TexTransBehavior>> renderersPeaTTFsDict)
        {
            foreach (var ttf in tTFs)
            {
                switch (ttf)
                {
                    case AbstractDecal abstractDecal:
                        {
                            foreach (var tRenderer in abstractDecal.TargetRenderers)
                            {
                                if (renderersPeaTTFsDict.ContainsKey(tRenderer))
                                {
                                    renderersPeaTTFsDict[tRenderer].Add(abstractDecal);
                                }
                                else
                                {
                                    renderersPeaTTFsDict.Add(tRenderer, new List<TexTransBehavior>() { abstractDecal });
                                }
                            }
                            break;
                        }
                    case AtlasTexture atlasTexture:
                        {
                            foreach (var tRenderer in atlasTexture.Renderers)
                            {
                                if (renderersPeaTTFsDict.ContainsKey(tRenderer))
                                {
                                    renderersPeaTTFsDict[tRenderer].Add(atlasTexture);
                                }
                                else
                                {
                                    renderersPeaTTFsDict.Add(tRenderer, new List<TexTransBehavior>() { atlasTexture });
                                }
                            }
                            break;
                        }
                    case TexTransGroup abstractTexTransGroup:
                        {
                            CollectTexTransForms(TexTransGroup.TextureTransformerFilter(abstractTexTransGroup.Targets), renderersPeaTTFsDict);
                            break;
                        }

                    default: { break; }
                }
            }
        }
    }
}

#endif