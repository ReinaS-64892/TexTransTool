#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Rs64.TexTransTool;
using TexLU = Rs64.TexTransTool.TextureLayerUtil;


namespace Rs64.TexTransTool.ShaderSupport
{
    public class liltoonSupport : IShaderSupport
    {
        public string ShaderName => "lilToon";

        public PropertyNameAndDisplayName[] GetPropatyNames
        {
            get
            {
                return new PropertyNameAndDisplayName[]{
                new PropertyNameAndDisplayName("_MainTex", "MainTexture"),
                new PropertyNameAndDisplayName("_EmissionMap", "EmissionMap"),

                new PropertyNameAndDisplayName("_Main2ndTex", "2ndMainTexture"),
                new PropertyNameAndDisplayName("_Main3rdTex", "3rdMainTexture"),

                new PropertyNameAndDisplayName("_Emission2ndMap", "Emission2ndMap"),


                new PropertyNameAndDisplayName("_MainColorAdjustMask", "MainColorAdjustMask"),
                new PropertyNameAndDisplayName("_Main2ndBlendMask", "2ndMainBlendMask"),
                new PropertyNameAndDisplayName("_Main3rdBlendMask", "3rdMainBlendMask"),
                new PropertyNameAndDisplayName("_AlphaMask", "AlphaMask"),
                new PropertyNameAndDisplayName("_BumpMap", "NormalMap"),
                new PropertyNameAndDisplayName("_Bump2ndMap", "2ndNormalMap"),
                new PropertyNameAndDisplayName("_Bump2ndScaleMask", "2ndNormalScaleMask"),
                new PropertyNameAndDisplayName("_AnisotropyTangentMap", "AnisotropyTangentMap"),
                new PropertyNameAndDisplayName("_AnisotropyScaleMask", "AnisotropyScaleMask"),
                new PropertyNameAndDisplayName("_BacklightColorTex", "BacklightColorTex"),
                new PropertyNameAndDisplayName("_ShadowStrengthMask", "ShadowStrengthMask"),
                new PropertyNameAndDisplayName("_ShadowBorderMask", "ShadowBorderMask"),
                new PropertyNameAndDisplayName("_ShadowBlurMask", "ShadowBlurMask"),
                new PropertyNameAndDisplayName("_ShadowColorTex", "ShadowColorTex"),
                new PropertyNameAndDisplayName("_Shadow2ndColorTex", "Shadow2ndColorTex"),
                new PropertyNameAndDisplayName("_Shadow3rdColorTex", "Shadow3rdColorTex"),
                new PropertyNameAndDisplayName("_SmoothnessTex", "SmoothnessTex"),
                new PropertyNameAndDisplayName("_MetallicGlossMap", "MetallicGlossMap"),
                new PropertyNameAndDisplayName("_ReflectionColorTex", "ReflectionColorTex"),
                new PropertyNameAndDisplayName("_RimColorTex", "RimColorTex"),
                new PropertyNameAndDisplayName("_GlitterColorTex", "GlitterColorTex"),
                new PropertyNameAndDisplayName("_EmissionBlendMask", "EmissionBlendMask"),
                new PropertyNameAndDisplayName("_Emission2ndBlendMask", "Emission2ndBlendMask"),
                new PropertyNameAndDisplayName("_AudioLinkMask", "AudioLinkMask"),
                new PropertyNameAndDisplayName("_OutlineTex", "OutlineTex"),
                new PropertyNameAndDisplayName("_OutlineWidthMask", "OutlineWidthMask"),
                new PropertyNameAndDisplayName("_OutlineVectorTex", "OutlineVectorTex"),
                };
            }
        }
    }
}
#endif