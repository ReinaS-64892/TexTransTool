#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using net.rs64.TexTransTool.ShaderSupport;

namespace net.rs64.TexTransTool.TexturAtlas
{
    public interface IAtlasShaderSupport
    {
        bool IsThisShader(Material material);
        void AddRecord(Material material);
        void ClearRecord();

        List<PropAndTexture> GetPropertyAndTextures(Material material, PropatyBakeSetting bakeSetting);
        void MaterialCustomSetting(Material material);
    }
}
#endif