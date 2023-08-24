#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rs64.TexTransTool.ShaderSupport;
using UnityEngine;

namespace Rs64.TexTransTool.TexturAtlas
{
    public class AtlasShaderSupportUtili
    {
        AtlasDefaultShaderSupprot _defaultShaderSupprot;
        List<IAtlasShaderSupport> _shaderSupports;

        public PropatyBakeSetting BakeSetting = PropatyBakeSetting.NotBake;
        public AtlasShaderSupportUtili()
        {
            _defaultShaderSupprot = new AtlasDefaultShaderSupprot();
            _shaderSupports = ShaderSupportUtili.GetInterfaseInstans<IAtlasShaderSupport>(new Type[] { typeof(AtlasDefaultShaderSupprot) });
        }

        public void AddRecord(Material material)
        {
            var supportShederI = FindSupportI(material);
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
            List<PropAndTexture> allTexs;
            var supportShederI = FindSupportI(material);

            if (supportShederI != null) { allTexs = supportShederI.GetPropertyAndTextures(material, BakeSetting); }
            else { allTexs = _defaultShaderSupprot.GetPropertyAndTextures(material, BakeSetting); }

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
            var supportShederI = FindSupportI(material);
            if (supportShederI != null)
            {
                supportShederI.MaterialCustomSetting(material);
            }
        }
        public IAtlasShaderSupport FindSupportI(Material material)
        {
            return _shaderSupports.Find(i => { return i.IsThisShader(material); });
        }
    }
}
#endif