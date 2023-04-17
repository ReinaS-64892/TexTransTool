#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace Rs.TexturAtlasCompiler.ShaderSupport
{
    [InitializeOnLoad]
    public class liltoonSupport : IShaderSupport
    {
        public string SupprotShaderName => "liltoon";

        public List<PropertyAndTextures> GetPropertyAndTextures(Material material)
        {
            var PropertyAndTextures = new List<PropertyAndTextures>();

            PropertyAndTextures.Add(new PropertyAndTextures("_MainTex", material.GetTexture("_MainTex") as Texture2D));
            PropertyAndTextures.Add(new PropertyAndTextures("_BumpMap", material.GetTexture("_BumpMap") as Texture2D));
            PropertyAndTextures.Add(new PropertyAndTextures("_Bump2ndMap", material.GetTexture("_Bump2ndMap") as Texture2D));
            PropertyAndTextures.Add(new PropertyAndTextures("_ShadowStrengthMask", material.GetTexture("_ShadowStrengthMask") as Texture2D));
            PropertyAndTextures.Add(new PropertyAndTextures("_ShadowColorTex", material.GetTexture("_ShadowColorTex") as Texture2D));
            PropertyAndTextures.Add(new PropertyAndTextures("_RimColorTex", material.GetTexture("_RimColorTex") as Texture2D));
            PropertyAndTextures.Add(new PropertyAndTextures("_EmissionMap", material.GetTexture("_EmissionMap") as Texture2D));
            PropertyAndTextures.Add(new PropertyAndTextures("_Emission2ndMap", material.GetTexture("_Emission2ndMap") as Texture2D));
            PropertyAndTextures.Add(new PropertyAndTextures("_OutlineTex", material.GetTexture("_OutlineTex") as Texture2D));
            PropertyAndTextures.Add(new PropertyAndTextures("_OutlineWidthMask", material.GetTexture("_OutlineWidthMask") as Texture2D));

            return PropertyAndTextures;
        }
    }
}
#endif