
namespace net.rs64.TexTransTool.ShaderSupport
{
    public interface IShaderSupport
    {
        string ShaderName { get; }

        PropertyNameAndDisplayName[] GetPropertyNames { get; }
    }

    public struct PropertyNameAndDisplayName
    {
        public string PropertyName;
        public string DisplayName;
        public PropertyNameAndDisplayName(string PropertyName, string DisplayName)
        {
            this.PropertyName = PropertyName;
            this.DisplayName = DisplayName;
        }
    }


}
