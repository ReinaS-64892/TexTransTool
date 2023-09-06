
namespace net.rs64.TexTransTool.ShaderSupport
{
    public class DefaultShaderSupport : IShaderSupport
    {
        public string ShaderName => "DefaultShader";

        public PropertyNameAndDisplayName[] GetPropertyNames => new PropertyNameAndDisplayName[] { new PropertyNameAndDisplayName("_MainTex", "MainTexture") };
    }
}