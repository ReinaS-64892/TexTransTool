#nullable enable
using net.rs64.TexTransTool.TextureAtlas.AAOCode;
using UnityEngine;

namespace net.rs64.TexTransTool.TextureAtlas
{
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
}
