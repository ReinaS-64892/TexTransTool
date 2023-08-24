#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Rs64.TexTransTool;
using TexLU = Rs64.TexTransTool.TextureLayerUtil;


namespace Rs64.TexTransTool.ShaderSupport
{
    public class DefaultShaderSupprot : IShaderSupport
    {
        public string ShaderName => "DefaultShader";

        public PropertyNameAndDisplayName[] GetPropatyNames => new PropertyNameAndDisplayName[] { new PropertyNameAndDisplayName("_MainTex", "MainTexture") };

        public void AddRecord(Material material)
        {
        }
        public void ClearRecord()
        {
        }

        public List<PropAndTexture> GetPropertyAndTextures(Material material, bool IsGNTFMP = false)
        {
            if (material.HasProperty("_MainTex")){ return new List<PropAndTexture>() { new PropAndTexture("_MainTex", material.GetTexture("_MainTex")) }; }
            else { return new List<PropAndTexture>(); }
        }

        public void MaterialCustomSetting(Material material)
        {
        }
    }
}
#endif