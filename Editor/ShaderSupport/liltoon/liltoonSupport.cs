#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using net.rs64.TexTransTool;
using TexLU = net.rs64.TexTransCore.BlendTexture;


namespace net.rs64.TexTransTool.ShaderSupport
{
    internal class liltoonSupport : IShaderSupport
    {
        public string ShaderName => "lilToon";

        public (string PropertyName, string DisplayName)[] GetPropertyNames => property;

        static (string PropertyName, string DisplayName)[] property = new []{
                ("_MainTex", "MainTexture"),
                ("_EmissionMap", "EmissionMap"),
                ("_Main2ndTex", "2ndMainTexture"),
                ("_Main3rdTex", "3rdMainTexture"),
                ("_Emission2ndMap", "Emission2ndMap"),
                ("_MainColorAdjustMask", "MainColorAdjustMask"),
                ("_Main2ndBlendMask", "2ndMainBlendMask"),
                ("_Main3rdBlendMask", "3rdMainBlendMask"),
                ("_AlphaMask", "AlphaMask"),
                ("_BumpMap", "NormalMap"),
                ("_Bump2ndMap", "2ndNormalMap"),
                ("_Bump2ndScaleMask", "2ndNormalScaleMask"),
                ("_AnisotropyTangentMap", "AnisotropyTangentMap"),
                ("_AnisotropyScaleMask", "AnisotropyScaleMask"),
                ("_BacklightColorTex", "BacklightColorTex"),
                ("_ShadowStrengthMask", "ShadowStrengthMask"),
                ("_ShadowBorderMask", "ShadowBorderMask"),
                ("_ShadowBlurMask", "ShadowBlurMask"),
                ("_ShadowColorTex", "ShadowColorTex"),
                ("_Shadow2ndColorTex", "Shadow2ndColorTex"),
                ("_Shadow3rdColorTex", "Shadow3rdColorTex"),
                ("_SmoothnessTex", "SmoothnessTex"),
                ("_MetallicGlossMap", "MetallicGlossMap"),
                ("_ReflectionColorTex", "ReflectionColorTex"),
                ("_RimColorTex", "RimColorTex"),
                ("_GlitterColorTex", "GlitterColorTex"),
                ("_EmissionBlendMask", "EmissionBlendMask"),
                ("_Emission2ndBlendMask", "Emission2ndBlendMask"),
                ("_AudioLinkMask", "AudioLinkMask"),
                ("_OutlineTex", "OutlineTex"),
                ("_OutlineWidthMask", "OutlineWidthMask"),
                ("_OutlineVectorTex", "OutlineVectorTex"),
                };

    }
}
#endif
