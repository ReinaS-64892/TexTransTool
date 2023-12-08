#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using static net.rs64.TexTransTool.TextureAtlas.FineSetting.Compress;

namespace net.rs64.TexTransTool.TextureAtlas.FineSetting
{
    internal struct Compress : IAddFineTuning
    {
        public FormatQuality FormatQualityValue;
        public TextureCompressionQuality CompressionQuality;
        public string PropertyNames;
        public PropertySelect Select;

        public Compress(FormatQuality formatQuality, TextureCompressionQuality compressionQuality, string propertyNames, PropertySelect select)
        {
            FormatQualityValue = formatQuality;
            CompressionQuality = compressionQuality;
            PropertyNames = propertyNames;
            Select = select;

        }

        public enum FormatQuality
        {
            None,
            Low,
            Normal,
            High,
        }
        public void AddSetting(List<TexFineTuningTarget> propAndTextures)
        {
            foreach (var target in FineSettingUtil.FilteredTarget(PropertyNames, Select, propAndTextures))
            {
                var compressionQualityData = target.TuningDataList.Find(I => I is CompressionQualityData) as CompressionQualityData;
                if (compressionQualityData != null)
                {
                    compressionQualityData.FormatQualityValue = FormatQualityValue;
                    compressionQualityData.CompressionQuality = CompressionQuality;
                }
                else
                {
                    target.TuningDataList.Add(new CompressionQualityData() { FormatQualityValue = FormatQualityValue, CompressionQuality = CompressionQuality });
                }
            }
        }

    }

    internal class CompressionQualityData : ITuningData
    {
        public Compress.FormatQuality FormatQualityValue = Compress.FormatQuality.Normal;
        public TextureCompressionQuality CompressionQuality = TextureCompressionQuality.Normal;
    }
    internal class CompressionQualityApplicant : ITuningApplicant
    {
        public int Order => 0;

        public void ApplyTuning(List<TexFineTuningTarget> texFineTuningTargets)
        {
            // Delegated to ITextureManager
        }
        public static TextureFormat GetTextureFormat(FormatQuality formatQuality)
        {
            var textureFormat = TextureFormat.RGBA32;
#if UNITY_STANDALONE_WIN
            switch (formatQuality)
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
            switch (formatQuality)
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
#endif
