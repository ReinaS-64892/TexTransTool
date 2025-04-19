#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransTool.Utils;
using UnityEngine;

namespace net.rs64.TexTransTool.TextureAtlas
{

    public static class TTShaderTextureUsageInformationRegistry
    {
        internal static Dictionary<Shader, ITTShaderTextureUsageInformation> s_information = new();

        public static IDisposable RegisterTTShaderTextureUsageInformation(Shader shader, ITTShaderTextureUsageInformation information)
        {
            s_information[shader] = information;
            return new InformationHolder(shader, information);
        }
        class InformationHolder : IDisposable
        {
            Shader _target;
            ITTShaderTextureUsageInformation _information;

            public InformationHolder(Shader shader, ITTShaderTextureUsageInformation information)
            {
                _target = shader;
                _information = information;
            }
            public void Dispose()
            {
                if (s_information.TryGetValue(_target, out var i))
                    if (i == _information)
                        s_information.Remove(_target);
            }
        }
    }
    internal static class TTShaderTextureUsageInformationUtil
    {

        public static IReadOnlyDictionary<string, UsageUVChannel> GetContainsUVUsage(Material material)
        {
            if (TTShaderTextureUsageInformationRegistry.s_information.TryGetValue(material.shader, out var info) is false)
                info = new FallbackShaderTextureUsage();

            var provider = new MaterialUVUsageProvider(material);
            info.GetMaterialTextureUVUsage(provider);
            return provider.UVUSage;
        }


        class FallbackShaderTextureUsage : ITTShaderTextureUsageInformation
        {
            public int Priority => int.MaxValue;

            public bool IsSupportShader(Shader shader) { return true; }

            public void GetMaterialTextureUVUsage(ITTTextureUVUsageWriter writer)
            {
                writer.WriteTextureUVUsage("_MainTex", UsageUVChannel.UV0);
            }
        }
    }
}
