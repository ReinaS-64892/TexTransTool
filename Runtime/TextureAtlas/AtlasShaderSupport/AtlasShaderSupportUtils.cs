using System;
using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransCore.TransTextureCore.Utils;
using net.rs64.TexTransTool.TextureAtlas.AtlasScriptableObject;
using UnityEngine;
using UnityEngine.Profiling;
using TexLU = net.rs64.TexTransCore.BlendTexture.TextureBlend;
using TexUT = net.rs64.TexTransCore.TransTextureCore.Utils.TextureUtility;

namespace net.rs64.TexTransTool.TextureAtlas
{
    internal class AtlasShaderSupportUtils
    {
        internal static List<AtlasShaderSupportScriptableObject> s_atlasShaderSupportList;

        public AtlasShaderSupportScriptableObject GetAtlasShaderSupporter(Material mat)
        {
            return s_atlasShaderSupportList.First(i => i.SupportedShaderComparer.ThisSupported(mat));
        }

    }

}
