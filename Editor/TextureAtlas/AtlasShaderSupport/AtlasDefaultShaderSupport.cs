using System.Collections.Generic;
using UnityEngine;

namespace net.rs64.TexTransTool.TextureAtlas
{
    public class AtlasDefaultShaderSupport : IAtlasShaderSupport
    {
        public void AddRecord(Material material) { }
        public void ClearRecord() { }
        public List<PropAndTexture> GetPropertyAndTextures(Material material, PropertyBakeSetting bakeSetting)
        {
            if (material.HasProperty("_MainTex")) { return new List<PropAndTexture>() { new PropAndTexture("_MainTex", material.GetTexture("_MainTex")) }; }
            else { return new List<PropAndTexture>(); }
        }
        public bool IsThisShader(Material material) { return false; }
        public void MaterialCustomSetting(Material material) { }
    }
}