#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransTool.TextureAtlas.AAOCode;
using net.rs64.TexTransTool.Utils;
using UnityEngine;

namespace net.rs64.TexTransTool.TextureAtlas
{
    public interface ITTShaderTextureUsageInformation
    {
        int Priority { get; }
        bool IsSupportShader(Shader shader);

        void GetMaterialTextureUVUsage(ITTTextureUVUsageWriter writer);
    }
    public interface ITTTextureUVUsageWriter
    {
        // なぜこのあたりの Property の取得にインターフェースを用いさせるかと言うと ... 今後 TTT 内で完全に Unity のマテリアルを捨てる可能性がなくはないから。
        int GetInteger(string propertyName);
        int GetInt(string propertyName);
        float GetFloat(string propertyName);
        Vector4 GetVector(string propertyName);
        bool IsShaderKeywordEnabled(string keywordName);

        void WriteTextureUVUsage(string propertyName, UsageUVChannel uVChannel);
    }
    public enum UsageUVChannel
    {
        Unknown = 0,

        UV0 = 1,
        UV1 = 2,
        UV2 = 3,
        UV3 = 4,
        UV4 = 5,
        UV5 = 6,
        UV6 = 7,
        UV7 = 8,
    }

    public class MaterialInformationTranslator : IMaterialInformationCallbackAbstractionInterface
    {
        private readonly ITTTextureUVUsageWriter _uvUsageWriter;

        public MaterialInformationTranslator(ITTTextureUVUsageWriter uvUsageWriter)
        {
            _uvUsageWriter = uvUsageWriter;
        }

        public float? GetFloat(string propertyName, bool considerAnimation = true)
        {
            return _uvUsageWriter.GetFloat(propertyName);
        }

        public int? GetInt(string propertyName, bool considerAnimation = true)
        {
            return _uvUsageWriter.GetInt(propertyName);
        }

        public int? GetInteger(string propertyName, bool considerAnimation = true)
        {
            return _uvUsageWriter.GetInteger(propertyName);
        }

        public Vector4? GetVector(string propertyName, bool considerAnimation = true)
        {
            return _uvUsageWriter.GetVector(propertyName);
        }

        public bool? IsShaderKeywordEnabled(string keywordName)
        {
            return _uvUsageWriter.IsShaderKeywordEnabled(keywordName);
        }


        public void RegisterTextureUVUsage(string textureMaterialPropertyName, SamplerStateInformation samplerState, UsingUVChannels uvChannels, Matrix2x3? uvMatrix)
        {
            var ttUVChannel = (int)uvChannels switch
            {
                1 => UsageUVChannel.UV0,
                2 => UsageUVChannel.UV1,
                4 => UsageUVChannel.UV2,
                8 => UsageUVChannel.UV3,
                16 => UsageUVChannel.UV4,
                32 => UsageUVChannel.UV5,
                64 => UsageUVChannel.UV6,
                128 => UsageUVChannel.UV7,
                _ => UsageUVChannel.Unknown,
            };
            _uvUsageWriter.WriteTextureUVUsage(textureMaterialPropertyName, ttUVChannel);
        }

        public void RegisterVertexIndexUsage()
        {
            // Do nothing
        }
        public void RegisterOtherUVUsage(UsingUVChannels uvChannel)
        {
            // Do nothing
        }
    }

    internal static class TTShaderTextureUsageInformationUtil
    {
        static ITTShaderTextureUsageInformation[]? s_information;
        public static IReadOnlyDictionary<string, UsageUVChannel> GetContainsUVUsage(Material material)
        {
            if (s_information is null)
            {
                s_information = InterfaceUtility.GetInterfaceInstance<ITTShaderTextureUsageInformation>().ToArray();
                Array.Sort(s_information, (l, r) => l.Priority - r.Priority);
            }
            var shader = material.shader;
            var info = s_information.FirstOrDefault(i => i.IsSupportShader(shader));

            if (info is null) { return new Dictionary<string, UsageUVChannel>(); }

            var provider = new MaterialUVUsageProvider(material);
            info.GetMaterialTextureUVUsage(provider);
            return provider.UVUSage;
        }
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
    class MaterialUVUsageProvider : ITTTextureUVUsageWriter
    {
        Material _material;
        Dictionary<string, UsageUVChannel> _uvUsage = new();
        public IReadOnlyDictionary<string, UsageUVChannel> UVUSage => _uvUsage;
        public MaterialUVUsageProvider(Material material)
        {
            _material = material;
        }
        public float GetFloat(string propertyName)
        {
            return _material.HasFloat(propertyName) ? _material.GetFloat(propertyName) : 0f;
        }

        public int GetInt(string propertyName)
        {
            return _material.HasInt(propertyName) ? _material.GetInt(propertyName) : 0;
        }

        public int GetInteger(string propertyName)
        {
            return _material.HasInteger(propertyName) ? _material.GetInteger(propertyName) : 0;
        }

        public Vector4 GetVector(string propertyName)
        {
            return _material.HasVector(propertyName) ? _material.GetVector(propertyName) : Vector4.zero;
        }

        public bool IsShaderKeywordEnabled(string keywordName)
        {
            return _material.IsKeywordEnabled(keywordName);
        }

        public void WriteTextureUVUsage(string propertyName, UsageUVChannel uVChannel)
        {
            if (_material.HasTexture(propertyName) is false) { return; }
            _uvUsage[propertyName] = uVChannel;
        }
    }
}
