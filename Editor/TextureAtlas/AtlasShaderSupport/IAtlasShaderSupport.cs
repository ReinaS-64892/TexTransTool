#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using net.rs64.TexTransTool.ShaderSupport;

namespace net.rs64.TexTransTool.TextureAtlas
{
    internal interface IAtlasShaderSupport
    {
        bool IsThisShader(Material material);
        void AddRecord(Material material);
        void ClearRecord();

        List<PropAndTexture> GetPropertyAndTextures(ITextureManager textureManager,Material material, PropertyBakeSetting bakeSetting);
        void MaterialCustomSetting(Material material);
    }
}
#endif
