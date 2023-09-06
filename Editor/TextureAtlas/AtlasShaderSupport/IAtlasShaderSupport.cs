using System.Collections.Generic;
using UnityEngine;

namespace net.rs64.TexTransTool.TextureAtlas
{
    public interface IAtlasShaderSupport
    {
        bool IsThisShader(Material material);
        void AddRecord(Material material);
        void ClearRecord();

        List<PropAndTexture> GetPropertyAndTextures(Material material, PropertyBakeSetting bakeSetting);
        void MaterialCustomSetting(Material material);
    }
}