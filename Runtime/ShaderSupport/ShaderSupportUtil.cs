#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Rs.TexturAtlasCompiler.ShaderSupport
{
    public class ShaderSupportUtil
    {
        public static List<IShaderSupport> GetSupprotInstans()
        {
            var SappotedLists = Assembly.GetExecutingAssembly().GetTypes().Where(C => C.GetInterfaces().Any(I => I == typeof(IShaderSupport))).ToList();
            List<IShaderSupport> SappotedListInstans = SappotedLists.ConvertAll<IShaderSupport>(I => Activator.CreateInstance(I) as IShaderSupport);
            return SappotedListInstans;
        }
    }
}
#endif