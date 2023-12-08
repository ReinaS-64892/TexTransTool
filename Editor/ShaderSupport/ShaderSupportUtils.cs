#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using net.rs64.TexTransCore.TransTextureCore.Utils;
using UnityEngine;

namespace net.rs64.TexTransTool.ShaderSupport
{
    internal class ShaderSupportUtils
    {
        DefaultShaderSupport _defaultShaderSupport;
        List<IShaderSupport> _shaderSupports;
        public ShaderSupportUtils()
        {
            _defaultShaderSupport = new DefaultShaderSupport();
            _shaderSupports = InterfaceUtility.GetInterfaceInstance<IShaderSupport>(new Type[] { typeof(DefaultShaderSupport) });
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
#endif
