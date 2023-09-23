#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using net.rs64.TexTransTool.ShaderSupport;
using UnityEngine;

namespace net.rs64.TexTransTool.TextureAtlas
{
    public class AtlasShaderSupportUtils
    {
        AtlasDefaultShaderSupport _defaultShaderSupport;
        List<IAtlasShaderSupport> _shaderSupports;

        public PropertyBakeSetting BakeSetting = PropertyBakeSetting.NotBake;
        public AtlasShaderSupportUtils()
        {
            _defaultShaderSupport = new AtlasDefaultShaderSupport();
            _shaderSupports = InterfaceUtility.GetInterfaceInstance<IAtlasShaderSupport>(new Type[] { typeof(AtlasDefaultShaderSupport) });
        }

        public void AddRecord(Material material)
        {
            var supportShaderI = FindSupportI(material);
            if (supportShaderI != null)
            {
                supportShaderI.AddRecord(material);
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
            List<PropAndTexture> allTex;
            var supportShaderI = FindSupportI(material);

            if (supportShaderI != null) { allTex = supportShaderI.GetPropertyAndTextures(material, BakeSetting); }
            else { allTex = _defaultShaderSupport.GetPropertyAndTextures(material, BakeSetting); }

            var textures = new List<PropAndTexture>();
            foreach (var tex in allTex)
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
            var supportShaderI = FindSupportI(material);
            if (supportShaderI != null)
            {
                supportShaderI.MaterialCustomSetting(material);
            }
        }
        public IAtlasShaderSupport FindSupportI(Material material)
        {
            return _shaderSupports.Find(i => { return i.IsThisShader(material); });
        }
    }
}
#endif
