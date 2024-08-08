using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace net.rs64.TexTransTool.TextureAtlas.FineTuning
{
    [Serializable]
    public class Compress : ITextureFineTuning
    {
        public FormatQuality FormatQualityValue = FormatQuality.Normal;
        public bool UseOverride = false;
        public TextureFormat OverrideTextureFormat = TextureFormat.BC7;
        [Range(0, 100)] public int CompressionQuality = 50;
        
        [Obsolete("V4SaveData", true)] public PropertyName PropertyNames = PropertyName.DefaultValue;
        public List<PropertyName> PropertyNameList = new() { PropertyName.DefaultValue };
        public PropertySelect Select = PropertySelect.Equal;

        public Compress() { }
        [Obsolete("V4SaveData", true)]
        public Compress(FormatQuality formatQuality, bool overrideFormat, TextureFormat overrideTextureFormat, int compressionQuality, PropertyName propertyNames, PropertySelect select)
        {
            FormatQualityValue = formatQuality;
            UseOverride = overrideFormat;
            OverrideTextureFormat = overrideTextureFormat;
            CompressionQuality = compressionQuality;
            PropertyNames = propertyNames;
            Select = select;
        }
        public Compress(FormatQuality formatQuality, bool overrideFormat, TextureFormat overrideTextureFormat, int compressionQuality, List<PropertyName> propertyNames, PropertySelect select)
        {
            FormatQualityValue = formatQuality;
            UseOverride = overrideFormat;
            OverrideTextureFormat = overrideTextureFormat;
            CompressionQuality = compressionQuality;
            PropertyNameList = propertyNames;
            Select = select;
        }

        public void AddSetting(Dictionary<string, TexFineTuningHolder> texFineTuningTargets)
        {
            foreach (var target in FineTuningUtil.FilteredTarget(PropertyNameList, Select, texFineTuningTargets))
            {
                var tuningHolder = target.Value;
                var compressionQualityData = tuningHolder.Get<TextureCompressionData>();

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
    public class TextureCompressionData : ITuningData, ITTTextureFormat
    {
        public FormatQuality FormatQualityValue = FormatQuality.Normal;

        public bool UseOverride = false;
        public TextureFormat OverrideTextureFormat = TextureFormat.BC7;

        [Range(0, 100)] public int CompressionQuality = 50;

        public virtual (TextureFormat CompressFormat, int Quality) Get(Texture2D texture2D)
        {
            if (UseOverride) { return (OverrideTextureFormat, CompressionQuality); }

#if UNITY_STANDALONE_WIN
            var hasAlpha = HasAlphaChannel(texture2D);
#else
            var hasAlpha = true;
#endif
            TextureFormat textureFormat = GetQuality2TextureFormat(FormatQualityValue, hasAlpha);
            return (textureFormat, CompressionQuality);
        }

        public static TextureFormat GetQuality2TextureFormat(FormatQuality formatQualityValue, bool hasAlpha)
        {
            var textureFormat = TextureFormat.RGBA32;
#if UNITY_STANDALONE_WIN
            switch (formatQualityValue, hasAlpha)
            {
                case (FormatQuality.None, false):
                case (FormatQuality.None, true):
                    textureFormat = TextureFormat.RGBA32;
                    break;
                case (FormatQuality.Low, false):
                case (FormatQuality.Normal, false):
                    textureFormat = TextureFormat.DXT1;
                    break;
                default:
                case (FormatQuality.Low, true):
                case (FormatQuality.Normal, true):
                    textureFormat = TextureFormat.DXT5;
                    break;
                case (FormatQuality.High, false):
                case (FormatQuality.High, true):
                    textureFormat = TextureFormat.BC7;
                    break;
            }
#elif UNITY_ANDROID
            switch (formatQualityValue)
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

        public bool HasAlphaChannel(Texture2D texture2D)
        {
            if (GraphicsFormatUtility.HasAlphaChannel(texture2D.format) is false) { return false; }
            if (texture2D.format != TextureFormat.RGBA32) { return true; }//RGBA32以外の実装はいったんしない TODO

            var containsAlpha = false;
            var span = texture2D.GetRawTextureData<Color32>().AsReadOnlySpan();
            for (int i = 0; span.Length > i; i += 1)
            {
                containsAlpha |= span[i].a != 255 && span[i].a != 254;
            }
            return containsAlpha;
        }

    }
    internal class CompressionQualityApplicant : ITuningApplicant
    {
        public int Order => 0;

        public void ApplyTuning(Dictionary<string, TexFineTuningHolder> texFineTuningTargets)
        {
            // Delegated to ITextureManager
        }
    }

}
