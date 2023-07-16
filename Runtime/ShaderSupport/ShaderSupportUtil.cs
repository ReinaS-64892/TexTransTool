#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Rs64.TexTransTool.ShaderSupport
{
    public class ShaderSupportUtil
    {
        public static List<IShaderSupport> GetSupprotInstans()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(I => I.GetTypes())
                //.Where(I => I != typeof(IShaderSupport) && I != typeof(object)  && I.IsAssignableFrom(typeof(IShaderSupport))) // なぜか...この方法だとうまくいかなかった...
                .Where(I => I.GetInterfaces().Any(I2 => I2 == typeof(IShaderSupport)))
                .Select(I =>
                {
                    try
                    {
                        //Debug.Log(I.ToString());
                        return (IShaderSupport)Activator.CreateInstance(I);
                    }
                    catch (Exception e)
                    {
                        Debug.Log(I.ToString());
                        throw e;
                    }
                })
                .ToList();
        }
    }
}
#endif