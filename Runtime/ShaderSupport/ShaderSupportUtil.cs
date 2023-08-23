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
        DefaultShaderSupprot _defaultShaderSupprot;
        List<IShaderSupport> _shaderSupports;

        public bool IsGenerateNewTextureForMergePropaty = false;
        public ShaderSupportUtili()
        {
            _defaultShaderSupprot = new DefaultShaderSupprot();
            _shaderSupports = ShaderSupportUtili.GetSupprotInstans();
        }

        public void AddRecord(Material material)
        {
            IShaderSupport supportShederI = FindSupportI(material);
            if (supportShederI != null)
            {
                supportShederI.AddRecord(material);
            }
        }
        public void ClearRecord()
        {
            foreach (var i in _shaderSupports)
            {
                i.ClearRecord();
            }
        }

        public List<PropAndTexture> GetTextures(Material material)
        {
            var allTexs = new List<PropAndTexture>();
            IShaderSupport supportShederI = FindSupportI(material);

            if (supportShederI != null) { allTexs = supportShederI.GetPropertyAndTextures(material, IsGenerateNewTextureForMergePropaty); }
            else { allTexs = _defaultShaderSupprot.GetPropertyAndTextures(material, IsGenerateNewTextureForMergePropaty); }

            var textures = new List<PropAndTexture>();
            foreach (var tex in allTexs)
            {
                if (tex.Texture2D != null)
                {
                    textures.Add(tex);
                }
            }

            return textures;
        }
        public void MaterialCustomSetting(Material material)
        {
            IShaderSupport SupportShederI = FindSupportI(material);
            if (SupportShederI != null)
            {
                SupportShederI.MaterialCustomSetting(material);
            }
        }

        public Dictionary<string, PropertyNameAndDisplayName[]> GetPropatyNames()
        {
            var PropatyNames = new Dictionary<string, PropertyNameAndDisplayName[]> { { _defaultShaderSupprot.DisplayShaderName, _defaultShaderSupprot.GetPropatyNames } };
            foreach (var i in _shaderSupports)
            {
                PropatyNames.Add(i.DisplayShaderName, i.GetPropatyNames);
            }
            return PropatyNames;
        }



        public IShaderSupport FindSupportI(Material material)
        {
            return _shaderSupports.Find(i => { return material.shader.name.Contains(i.SupprotShaderName); });
        }

        public static List<IShaderSupport> GetSupprotInstans()
        {
            var shaderSupport = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(I => I.GetTypes())
                //.Where(I => I != typeof(IShaderSupport) && I != typeof(object)  && I.IsAssignableFrom(typeof(IShaderSupport))) // なぜか...この方法だとうまくいかなかった...
                .Where(I => I.GetInterfaces().Any(I2 => I2 == typeof(IShaderSupport)))
                .Where(I => !I.IsAbstract && I != typeof(DefaultShaderSupprot))
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
            return shaderSupport;
        }


    }
}
#endif