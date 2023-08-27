#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransTool.Decal;
using net.rs64.TexTransTool.TexturAtlas;

namespace net.rs64.TexTransTool
{
    public static class TTGValidationUtili
    {
        public static void ValidatTTG(AbstractTexTransGroup texTransGroup)
        {
            var tTFs = texTransGroup.Targets.ToList();
            var rendarasPeaTTFsDict = new Dictionary<Renderer, List<TextureTransformer>>();
            CollectTexTransForms(tTFs, rendarasPeaTTFsDict);

            var warnTarget = new List<TextureTransformer>();

            foreach (var rptf in rendarasPeaTTFsDict)
            {
                var allowSepaleatFlag = true;
                foreach (var tf in rptf.Value)
                {
                    switch (tf)
                    {
                        case AbstractDecal abstractDecal:
                            {
                                if (!abstractDecal.IsSeparateMatAndTexture)
                                {
                                    allowSepaleatFlag = false;
                                }
                                else
                                {
                                    if (!allowSepaleatFlag) { warnTarget.Add(abstractDecal); }
                                }
                                break;
                            }
                        case AtlasTexture atlasTexture:
                            {
                                if (!allowSepaleatFlag) { warnTarget.Add(atlasTexture); }

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

        private static void CollectTexTransForms(IEnumerable<TextureTransformer> tTFs, Dictionary<Renderer, List<TextureTransformer>> rendarasPeaTTFsDict)
        {
            foreach (var ttf in tTFs)
            {
                switch (ttf)
                {
                    case AbstractDecal abstractDecal:
                        {
                            foreach (var trendaras in abstractDecal.TargetRenderers)
                            {
                                if (rendarasPeaTTFsDict.ContainsKey(trendaras))
                                {
                                    rendarasPeaTTFsDict[trendaras].Add(abstractDecal);
                                }
                                else
                                {
                                    rendarasPeaTTFsDict.Add(trendaras, new List<TextureTransformer>() { abstractDecal });
                                }
                            }
                            break;
                        }
                    case AtlasTexture atlasTexture:
                        {
                            foreach (var trendaras in atlasTexture.Renderers)
                            {
                                if (rendarasPeaTTFsDict.ContainsKey(trendaras))
                                {
                                    rendarasPeaTTFsDict[trendaras].Add(atlasTexture);
                                }
                                else
                                {
                                    rendarasPeaTTFsDict.Add(trendaras, new List<TextureTransformer>() { atlasTexture });
                                }
                            }
                            break;
                        }
                    case AbstractTexTransGroup abstractTexTransGroup:
                        {
                            CollectTexTransForms(abstractTexTransGroup.Targets, rendarasPeaTTFsDict);
                            break;
                        }

                    default: { break; }
                }
            }
        }
    }
}

#endif