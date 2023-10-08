#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using net.rs64.TexTransTool;
using TexLU = net.rs64.TexTransCore.BlendTexture.TextureBlendUtils;


namespace net.rs64.TexTransTool.ShaderSupport
{
    public class DefaultShaderSupport : IShaderSupport
    {
        public string ShaderName => "DefaultShader";

        public PropertyNameAndDisplayName[] GetPropertyNames => new PropertyNameAndDisplayName[] { new PropertyNameAndDisplayName("_MainTex", "MainTexture") };
    }
}
#endif
