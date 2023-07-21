#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Rs64.TexTransTool.ShaderSupport
{
    public class ShaderSupportUtili
    {
        List<IShaderSupport> _shaderSupports;

        public ShaderSupportUtili()
        {
            _shaderSupports = ShaderSupportUtili.GetSupprotInstans();
        }

        public List<PropAndTexture> GetTextures(Material material)
        {
            var textures = new List<PropAndTexture>();
            var SupportShederI = _shaderSupports.Find(i => { return material.shader.name.Contains(i.SupprotShaderName); });

            if (SupportShederI != null)
            {
                var texs = SupportShederI.GetPropertyAndTextures(material);
                foreach (var tex in texs)
                {
                    if (tex.Texture2D != null)
                    {
                        textures.Add(tex);
                    }
                }
            }
            else
            {
                var PropertyName = "_MainTex";
                if (material.GetTexture(PropertyName) is Texture2D texture2D && texture2D != null)
                {
                    textures.Add(new PropAndTexture(PropertyName, texture2D));
                }

            }
            return textures;
        }


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