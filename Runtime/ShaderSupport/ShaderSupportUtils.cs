#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
            _shaderSupports = ShaderSupportUtils.GetInterfaceInstance<IShaderSupport>(new Type[] { typeof(DefaultShaderSupport) });
        }

        public Dictionary<string, PropertyNameAndDisplayName[]> GetPropertyNames()
        {
            var PropertyNames = new Dictionary<string, PropertyNameAndDisplayName[]> { { _defaultShaderSupport.ShaderName, _defaultShaderSupport.GetPropertyNames } };
            foreach (var i in _shaderSupports)
            {
                PropertyNames.Add(i.ShaderName, i.GetPropertyNames);
            }
            return PropertyNames;
        }

        public static List<T> GetInterfaceInstance<T>(Type[] IgnorType = null)
        {
            var shaderSupport = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(I => I.GetTypes())
                //.Where(I => I != typeof(IShaderSupport) && I != typeof(object)  && I.IsAssignableFrom(typeof(IShaderSupport))) // なぜか...この方法だとうまくいかなかった...
                .Where(I => I.GetInterfaces().Any(I2 => I2 == typeof(T)))
                .Where(I => !I.IsAbstract && IgnorType.Any(I2 => I2 != I))
                .Select(I =>
                {
                    try
                    {
                        //Debug.Log(I.ToString());
                        return (T)Activator.CreateInstance(I);
                    }
                    catch (Exception e)
                    {
                        Debug.Log(I.ToString());
                        throw e;
                    }
                }).ToList();
            return shaderSupport;
        }


    }
}
#endif
