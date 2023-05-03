#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Rs64.TexTransTool.ShaderSupport
{
    public interface IShaderSupport
    {
        string SupprotShaderName { get; }
        List<PropAndTexture> GetPropertyAndTextures(Material material);

        void GenereatMaterialCustomSetting(Material material);


    }
}
#endif