#if UNITY_EDITOR
using System.Collections.Generic;

namespace net.rs64.TexTransTool.ShaderSupport
{
    internal class DefaultShaderSupport : IShaderSupport
    {
        public string ShaderName => "DefaultShader";
        public (string PropertyName, string DisplayName)[] GetPropertyNames => s_property;
        static (string PropertyName, string DisplayName)[] s_property = new[] { ("_MainTex", "MainTexture") };

    }
}
#endif
