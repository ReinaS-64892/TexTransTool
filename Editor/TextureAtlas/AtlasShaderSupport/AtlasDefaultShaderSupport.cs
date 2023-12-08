#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using net.rs64.TexTransTool;
using TexLU = net.rs64.TexTransCore.BlendTexture;

namespace net.rs64.TexTransTool.TextureAtlas
{
    internal class AtlasDefaultShaderSupport : IAtlasShaderSupport
    {
        public bool GetAllTexture = false;
        public void AddRecord(Material material) { }
        public void ClearRecord() { }
        public List<PropAndTexture> GetPropertyAndTextures(ITextureManager textureManager, Material material, PropertyBakeSetting bakeSetting)
        {
            if (GetAllTexture)
            {
                var textures = new List<PropAndTexture>();
                var shader = material.shader;
                var propCount = shader.GetPropertyCount();
                for (int i = 0; propCount > i; i += 1)
                {
                    if (shader.GetPropertyType(i) != UnityEngine.Rendering.ShaderPropertyType.Texture) { continue; }
                    var propName = shader.GetPropertyName(i);
                    if (!(material.GetTexture(propName) is Texture2D texture2D)) { continue; }
                    textures.Add(new PropAndTexture(propName, texture2D));
                }
                return textures;
            }

            if (material.HasProperty("_MainTex"))
            {
                return new List<PropAndTexture>() { new PropAndTexture("_MainTex", material.GetTexture("_MainTex") as Texture2D) };
            }
            else { return new List<PropAndTexture>(); }
        }
        public bool IsThisShader(Material material) { return false; }
        public void MaterialCustomSetting(Material material) { }
    }
}
#endif
