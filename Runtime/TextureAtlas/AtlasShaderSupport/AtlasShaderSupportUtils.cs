using System;
using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransCore;
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
        internal static AtlasShaderSupportScriptableObject s_defaultSupporter;

        [TexTransInitialize]
        public static void Initialize()
        {
            var atlasShaderSupportList = TexTransCoreRuntime.LoadAssetsAtType.Invoke(typeof(AtlasShaderSupportScriptableObject)).Cast<AtlasShaderSupportScriptableObject>().ToList();

            if (s_defaultSupporter == null)
            {
                s_defaultSupporter = ScriptableObject.CreateInstance<AtlasShaderSupportScriptableObject>();
                s_defaultSupporter.SupportedShaderComparer = new AnythingShader();
                s_defaultSupporter.AtlasTargetDefines = new() { new() { TexturePropertyName = "_MainTex", AtlasDefineConstraints = new Anything(), BakePropertyNames = new() } };
            }

            atlasShaderSupportList.Add(s_defaultSupporter);
            s_atlasShaderSupportList = atlasShaderSupportList;
        }

        public AtlasShaderSupportScriptableObject GetAtlasShaderSupporter(Material mat)
        {
            return s_atlasShaderSupportList.First(i => i.SupportedShaderComparer.ThisSupported(mat));
        }

    }

}
