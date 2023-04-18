#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Rs.TexturAtlasCompiler.ShaderSupport
{
    public interface IShaderSupport
    {
        string SupprotShaderName { get; }
        List<PropAndTexture> GetPropertyAndTextures(Material material);


    }
}
#endif