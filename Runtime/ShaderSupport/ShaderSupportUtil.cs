#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace net.rs64.TexTransTool.ShaderSupport
{
    public class ShaderSupportUtili
    {
        DefaultShaderSupprot _defaultShaderSupprot;
        List<IShaderSupport> _shaderSupports;
        public ShaderSupportUtili()
        {
            _defaultShaderSupprot = new DefaultShaderSupprot();
            _shaderSupports = ShaderSupportUtili.GetInterfaseInstans<IShaderSupport>(new Type[] { typeof(DefaultShaderSupprot) });
        }

        public Dictionary<string, PropertyNameAndDisplayName[]> GetPropatyNames()
        {
            var PropatyNames = new Dictionary<string, PropertyNameAndDisplayName[]> { { _defaultShaderSupprot.ShaderName, _defaultShaderSupprot.GetPropatyNames } };
            foreach (var i in _shaderSupports)
            {
                PropatyNames.Add(i.ShaderName, i.GetPropatyNames);
            }
            return PropatyNames;
        }

        public static List<T> GetInterfaseInstans<T>(Type[] IgnorType = null)
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