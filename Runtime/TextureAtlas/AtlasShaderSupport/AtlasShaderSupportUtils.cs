using System;
using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransCoreForUnity;
using net.rs64.TexTransTool.TextureAtlas.AtlasScriptableObject;
using UnityEngine;

namespace net.rs64.TexTransTool.TextureAtlas
{
    internal class AtlasShaderSupportUtils
    {
        internal static List<AtlasShaderSupportScriptableObject> s_atlasShaderSupportList;
        internal static AtlasShaderSupportScriptableObject s_defaultSupporter;
        internal static AtlasShaderSupportScriptableObject DefaultSupporter
        {
            get
            {
                if (s_defaultSupporter == null)
                {
                    s_defaultSupporter = ScriptableObject.CreateInstance<AtlasShaderSupportScriptableObject>();
                    s_defaultSupporter.SupportedShaderComparer = new AnythingShader();
                    s_defaultSupporter.AtlasTargetDefines = new() { new() { TexturePropertyName = "_MainTex", AtlasDefineConstraints = new Anything(), BakePropertyDescriptions = new() } };
                    s_defaultSupporter.Priority = int.MaxValue;
                }
                return s_defaultSupporter;
            }
        }

        [TexTransInitialize]
        public static void Initialize()
        {
            SupporterInit();
            TexTransCoreRuntime.NewAssetListen[typeof(AtlasShaderSupportScriptableObject)] = SupporterInit;

            static void SupporterInit()
            {
                s_atlasShaderSupportList = TexTransCoreRuntime.LoadAssetsAtType.Invoke(typeof(AtlasShaderSupportScriptableObject)).Cast<AtlasShaderSupportScriptableObject>().ToList();
            }
        }



        public AtlasShaderSupportScriptableObject GetAtlasShaderSupporter(Material mat)
        {
            s_atlasShaderSupportList.RemoveAll(i => i == null);
            s_atlasShaderSupportList.Sort((l, r) => l.Priority - r.Priority);

            return s_atlasShaderSupportList.FirstOrDefault(i => i.SupportedShaderComparer.ThisSupported(mat)) ?? DefaultSupporter;
        }

    }

}
