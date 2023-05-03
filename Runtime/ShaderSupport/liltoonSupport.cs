#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace Rs64.TexTransTool.ShaderSupport
{
    public class liltoonSupport : IShaderSupport
    {
        public string SupprotShaderName => "lilToon";

        public void GenereatMaterialCustomSetting(Material material)
        {
            var MainTex = material.GetTexture("_MainTex") as Texture2D;
            material.SetTexture("_BaseMap", MainTex);
            material.SetTexture("_BaseColorMap", MainTex);
        }

        public List<PropAndTexture> GetPropertyAndTextures(Material material)
        {
            var PropertyAndTextures = new List<PropAndTexture>();

            PropertyAndTextures.Add(new PropAndTexture("_MainTex", material.GetTexture("_MainTex") as Texture2D));
            PropertyAndTextures.Add(new PropAndTexture("_Main2ndTex", material.GetTexture("_Main2ndTex") as Texture2D));
            PropertyAndTextures.Add(new PropAndTexture("_Main2ndBlendMask", material.GetTexture("_Main2ndBlendMask") as Texture2D));
            PropertyAndTextures.Add(new PropAndTexture("_Main2ndDissolveMask", material.GetTexture("_Main2ndDissolveMask") as Texture2D));
            PropertyAndTextures.Add(new PropAndTexture("_Main2ndDissolveNoiseMask", material.GetTexture("_Main2ndDissolveNoiseMask") as Texture2D));
            PropertyAndTextures.Add(new PropAndTexture("_Main3rdTex", material.GetTexture("_Main3rdTex") as Texture2D));
            PropertyAndTextures.Add(new PropAndTexture("_Main3rdBlendMask", material.GetTexture("_Main3rdBlendMask") as Texture2D));
            PropertyAndTextures.Add(new PropAndTexture("_Main3rdDissolveMask", material.GetTexture("_Main3rdDissolveMask") as Texture2D));
            PropertyAndTextures.Add(new PropAndTexture("_Main3rdDissolveNoiseMask", material.GetTexture("_Main3rdDissolveNoiseMask") as Texture2D));
            PropertyAndTextures.Add(new PropAndTexture("_BumpMap", material.GetTexture("_BumpMap") as Texture2D));
            PropertyAndTextures.Add(new PropAndTexture("_Bump2ndMap", material.GetTexture("_Bump2ndMap") as Texture2D));
            PropertyAndTextures.Add(new PropAndTexture("_Bump2ndScaleMask", material.GetTexture("_Bump2ndScaleMask") as Texture2D));
            PropertyAndTextures.Add(new PropAndTexture("_AnisotropyTangentMap", material.GetTexture("_AnisotropyTangentMap") as Texture2D));
            PropertyAndTextures.Add(new PropAndTexture("_AnisotropyScaleMask", material.GetTexture("_AnisotropyScaleMask") as Texture2D));
            PropertyAndTextures.Add(new PropAndTexture("_BacklightColorTex", material.GetTexture("_BacklightColorTex") as Texture2D));
            PropertyAndTextures.Add(new PropAndTexture("_ShadowStrengthMask", material.GetTexture("_ShadowStrengthMask") as Texture2D));
            PropertyAndTextures.Add(new PropAndTexture("_ShadowBorderMask", material.GetTexture("_ShadowBorderMask") as Texture2D));
            PropertyAndTextures.Add(new PropAndTexture("_ShadowBlurMask", material.GetTexture("_ShadowBlurMask") as Texture2D));
            PropertyAndTextures.Add(new PropAndTexture("_ShadowColorTex", material.GetTexture("_ShadowColorTex") as Texture2D));
            PropertyAndTextures.Add(new PropAndTexture("_Shadow2ndColorTex", material.GetTexture("_Shadow2ndColorTex") as Texture2D));
            PropertyAndTextures.Add(new PropAndTexture("_Shadow3rdColorTex", material.GetTexture("_Shadow3rdColorTex") as Texture2D));
            PropertyAndTextures.Add(new PropAndTexture("_SmoothnessTex", material.GetTexture("_SmoothnessTex") as Texture2D));
            PropertyAndTextures.Add(new PropAndTexture("_MetallicGlossMap", material.GetTexture("_MetallicGlossMap") as Texture2D));
            PropertyAndTextures.Add(new PropAndTexture("_ReflectionColorTex", material.GetTexture("_ReflectionColorTex") as Texture2D));
            PropertyAndTextures.Add(new PropAndTexture("_MatCapBlendMask", material.GetTexture("_MatCapBlendMask") as Texture2D));
            PropertyAndTextures.Add(new PropAndTexture("_MatCap2ndBlendMask", material.GetTexture("_MatCap2ndBlendMask") as Texture2D));
            PropertyAndTextures.Add(new PropAndTexture("_RimColorTex", material.GetTexture("_RimColorTex") as Texture2D));
            PropertyAndTextures.Add(new PropAndTexture("_GlitterColorTex", material.GetTexture("_GlitterColorTex") as Texture2D));
            PropertyAndTextures.Add(new PropAndTexture("_GlitterShapeTex", material.GetTexture("_GlitterShapeTex") as Texture2D));
            PropertyAndTextures.Add(new PropAndTexture("_EmissionMap", material.GetTexture("_EmissionMap") as Texture2D));
            PropertyAndTextures.Add(new PropAndTexture("_EmissionBlendMask", material.GetTexture("_EmissionBlendMask") as Texture2D));
            PropertyAndTextures.Add(new PropAndTexture("_EmissionGradTex", material.GetTexture("_EmissionGradTex") as Texture2D));
            PropertyAndTextures.Add(new PropAndTexture("_Emission2ndMap", material.GetTexture("_Emission2ndMap") as Texture2D));
            PropertyAndTextures.Add(new PropAndTexture("_Emission2ndBlendMask", material.GetTexture("_Emission2ndBlendMask") as Texture2D));
            PropertyAndTextures.Add(new PropAndTexture("_Emission2ndGradTex", material.GetTexture("_Emission2ndGradTex") as Texture2D));
            PropertyAndTextures.Add(new PropAndTexture("_ParallaxMap", material.GetTexture("_ParallaxMap") as Texture2D));
            PropertyAndTextures.Add(new PropAndTexture("_AudioLinkMask", material.GetTexture("_AudioLinkMask") as Texture2D));
            PropertyAndTextures.Add(new PropAndTexture("_DissolveMask", material.GetTexture("_DissolveMask") as Texture2D));
            PropertyAndTextures.Add(new PropAndTexture("_DissolveNoiseMask", material.GetTexture("_DissolveNoiseMask") as Texture2D));
            PropertyAndTextures.Add(new PropAndTexture("_OutlineTex", material.GetTexture("_OutlineTex") as Texture2D));
            PropertyAndTextures.Add(new PropAndTexture("_OutlineWidthMask", material.GetTexture("_OutlineWidthMask") as Texture2D));
            PropertyAndTextures.Add(new PropAndTexture("_OutlineVectorTex", material.GetTexture("_OutlineVectorTex") as Texture2D));

            return PropertyAndTextures;
        }
    }
}
#endif