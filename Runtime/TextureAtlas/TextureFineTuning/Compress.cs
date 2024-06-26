using System;
using System.Collections.Generic;
using UnityEngine;

namespace net.rs64.TexTransTool.TextureAtlas.FineTuning
{
    [Serializable]
    public struct Compress : ITextureFineTuning
    {
        public FormatQuality FormatQualityValue;
        public bool UseOverride;
        public TextureFormat OverrideTextureFormat;
        [Range(0, 100)] public int CompressionQuality;
        public PropertyName PropertyNames;
        public PropertySelect Select;


        public Compress(FormatQuality formatQuality, bool overrideFormat, TextureFormat overrideTextureFormat, int compressionQuality, PropertyName propertyNames, PropertySelect select)
        {
            FormatQualityValue = formatQuality;
            UseOverride = overrideFormat;
            OverrideTextureFormat = overrideTextureFormat;
            CompressionQuality = compressionQuality;
            PropertyNames = propertyNames;
            Select = select;

        }

        public static Compress Default => new(FormatQuality.High, false, TextureFormat.DXT5, 50, PropertyName.DefaultValue, PropertySelect.Equal);

        public void AddSetting(Dictionary<string, TexFineTuningHolder> texFineTuningTargets)
        {
            foreach (var target in FineTuningUtil.FilteredTarget(PropertyNames, Select, texFineTuningTargets))
            {
                var tuningHolder = target.Value;
                var compressionQualityData = tuningHolder.Get<CompressionQualityData>();

                compressionQualityData.FormatQualityValue = FormatQualityValue;
                compressionQualityData.CompressionQuality = CompressionQuality;

                compressionQualityData.UseOverride = UseOverride;
                compressionQualityData.OverrideTextureFormat = OverrideTextureFormat;

            }
        }

    }

    public enum FormatQuality
    {
        None,
        Low,
        Normal,
        High,
    }
    [Serializable]
    public class CompressionQualityData : ITuningData
    {
        public FormatQuality FormatQualityValue = FormatQuality.Normal;

        public bool UseOverride;
        public TextureFormat OverrideTextureFormat;


        [Range(0, 100)] public int CompressionQuality = 50;
    }
    internal class CompressionQualityApplicant : ITuningApplicant
    {
        public int Order => 0;

        public void ApplyTuning(Dictionary<string, TexFineTuningHolder> texFineTuningTargets)
        {
            // Delegated to ITextureManager
        }
        public static TextureFormat GetTextureFormat(CompressionQualityData compressionQualityData)
        {
            if (compressionQualityData.UseOverride) { return compressionQualityData.OverrideTextureFormat; }

            var textureFormat = TextureFormat.RGBA32;
#if UNITY_STANDALONE_WIN
            switch (compressionQualityData.FormatQualityValue)
            {
                case FormatQuality.None:
                    textureFormat = TextureFormat.RGBA32;
                    break;
                case FormatQuality.Low:
                    textureFormat = TextureFormat.DXT1;
                    break;
                default:
                case FormatQuality.Normal:
                    textureFormat = TextureFormat.DXT5;
                    break;
                case FormatQuality.High:
                    textureFormat = TextureFormat.BC7;
                    break;
            }
#elif UNITY_ANDROID
            switch (compressionQualityData.FormatQualityValue)
            {
                case FormatQuality.None:
                    textureFormat = TextureFormat.RGBA32;
                    break;
                case FormatQuality.Low:
                    textureFormat = TextureFormat.ASTC_8x8;
                    break;
                default:
                case FormatQuality.Normal:
                    textureFormat = TextureFormat.ASTC_6x6;
                    break;
                case FormatQuality.High:
                    textureFormat = TextureFormat.ASTC_4x4;
                    break;
            }
#endif
            return textureFormat;
        }
    }

}
