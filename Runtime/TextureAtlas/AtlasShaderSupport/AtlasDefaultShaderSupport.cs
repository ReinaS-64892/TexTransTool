using System.Collections.Generic;
using UnityEngine;

namespace net.rs64.TexTransTool.TextureAtlas
{
    internal class AtlasDefaultShaderSupport : IAtlasShaderSupport
    {
        public void AddRecord(Material material) { }
        public void ClearRecord() { }
        public List<PropAndTexture> GetPropertyAndTextures(IOriginTexture textureManager, Material material, PropertyBakeSetting bakeSetting)
        {
            if (material.HasProperty("_MainTex"))
            {
                return new () { new ("_MainTex", material.GetTexture("_MainTex") as Texture2D) };
            }
            else { return new (); }
        }
        public bool IsThisShader(Material material) { return false; }
        public void MaterialCustomSetting(Material material) { }
    }
}
