#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using net.rs64.TexTransCore.TransTextureCore.Utils;
using UnityEngine;

namespace net.rs64.TexTransTool.ShaderSupport
{
    public class ShaderSupportUtils
    {
        DefaultShaderSupport _defaultShaderSupport;
        List<IShaderSupport> _shaderSupports;
        public ShaderSupportUtils()
        {
            _defaultShaderSupport = new DefaultShaderSupport();
            _shaderSupports = InterfaceUtility.GetInterfaceInstance<IShaderSupport>(new Type[] { typeof(DefaultShaderSupport) });
        }

        public Dictionary<string, PropertyNameAndDisplayName[]> GetPropertyNames()
        {
            var propertyNames = new Dictionary<string, PropertyNameAndDisplayName[]> { { _defaultShaderSupport.ShaderName, _defaultShaderSupport.GetPropertyNames } };
            foreach (var i in _shaderSupports)
            {
                propertyNames.Add(i.ShaderName, i.GetPropertyNames);
            }
            return propertyNames;
        }


    }
}
#endif
