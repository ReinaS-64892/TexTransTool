#if UNITY_EDITOR
using System.Collections.Generic;

namespace net.rs64.TexTransTool.ShaderSupport
{
    public class DefaultShaderSupport : IShaderSupport
    {
        public string ShaderName => "DefaultShader";
        public (string PropertyName, string DisplayName)[] GetPropertyNames => Property;
        static (string PropertyName, string DisplayName)[] Property = new (string PropertyName, string DisplayName)[] { ("_MainTex", "MainTexture") };

    }
}
#endif
