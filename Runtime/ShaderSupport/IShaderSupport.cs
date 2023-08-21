#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Rs64.TexTransTool.ShaderSupport
{
    public interface IShaderSupport
    {
        string SupprotShaderName { get; }

        void AddRecord(Material material);
        void ClearRecord();

        List<PropAndTexture> GetPropertyAndTextures(Material material, bool IsGNTFMP = false);

        void MaterialCustomSetting(Material material);


    }
}
#endif