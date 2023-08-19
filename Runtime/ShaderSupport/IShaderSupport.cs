#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Rs64.TexTransTool.ShaderSupport
{
    public interface IShaderSupport
    {
        string SupprotShaderName { get; }

        void PropatyDataStack(Material material);
        void StackClear();

        List<PropAndTexture> GetPropertyAndTextures(Material material, bool IsGNTFMP = false);

        void MaterialCustomSetting(Material material);


    }
}
#endif