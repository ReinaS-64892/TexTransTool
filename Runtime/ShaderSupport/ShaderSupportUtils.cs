using System;
using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransCoreForUnity.Utils;
namespace net.rs64.TexTransTool.ShaderSupport
{
    internal class ShaderSupportUtils
    {
        DefaultShaderSupport _defaultShaderSupport;
        List<IShaderSupport> _shaderSupports;
        public ShaderSupportUtils()
        {
            _defaultShaderSupport = new DefaultShaderSupport();
            _shaderSupports = InterfaceUtility.GetInterfaceInstance<IShaderSupport>(new Type[] { typeof(DefaultShaderSupport) }).ToList();
        }

        public List<(string ShaderName, (string PropertyName, string DisplayName)[])> GetPropertyNames()
        {
            var propertyNames = new List<(string ShaderName, (string PropertyName, string DisplayName)[])>(_shaderSupports.Count + 1) { (_defaultShaderSupport.ShaderName, _defaultShaderSupport.GetPropertyNames) };
            foreach (var i in _shaderSupports)
            {
                propertyNames.Add((i.ShaderName, i.GetPropertyNames));
            }
            return propertyNames;
        }


    }
}
