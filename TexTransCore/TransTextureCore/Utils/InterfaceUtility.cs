using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace net.rs64.TexTransCore.TransTextureCore.Utils
{

    internal static class InterfaceUtility
    {

        public static IEnumerable<T> GetInterfaceInstance<T>(Type[] IgnoreType = null)
        {
            if (IgnoreType == null) { IgnoreType = new[] { typeof(object) }; }
            var shaderSupport = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(I => I.GetTypes())
                //.Where(I => I != typeof(IShaderSupport) && I != typeof(object)  && I.IsAssignableFrom(typeof(IShaderSupport))) // なぜか...この方法だとうまくいかなかった...
                .Where(I => I.GetInterfaces().Any(I2 => I2 == typeof(T)))
                .Where(I => !I.IsAbstract && IgnoreType.Any(I2 => I2 != I))
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
                });
            return shaderSupport;
        }

    }
}