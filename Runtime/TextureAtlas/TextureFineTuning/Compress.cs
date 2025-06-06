#nullable enable
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace net.rs64.TexTransTool.TextureAtlas.FineTuning
{
    [Serializable]
    public class Compress : ITextureFineTuning
    {
        public FormatQuality FormatQualityValue = FormatQuality.Normal;
        public bool UseOverride = false;
        public TextureFormat OverrideTextureFormat = TextureFormat.BC7;
        [Range(0, 100)] public int CompressionQuality = 50;

        public List<PropertyName> PropertyNameList = new() { PropertyName.DefaultValue };
        public PropertySelect Select = PropertySelect.Equal;

        public Compress() { }
        public Compress(FormatQuality formatQuality, bool overrideFormat, TextureFormat overrideTextureFormat, int compressionQuality, List<PropertyName> propertyNames, PropertySelect select)
        {
            FormatQualityValue = formatQuality;
            UseOverride = overrideFormat;
            OverrideTextureFormat = overrideTextureFormat;
            CompressionQuality = compressionQuality;
            PropertyNameList = propertyNames;
            Select = select;
        }

        void AddSetting(Dictionary<string, TexFineTuningHolder> texFineTuningTargets)
        {
            foreach (var target in FineTuningUtil.FilteredTarget(PropertyNameList, Select, texFineTuningTargets))
            {
                var tuningHolder = target.Value;
                var compressionQualityData = tuningHolder.Get<TextureCompressionTuningData>();

                compressionQualityData.FormatQualityValue = FormatQualityValue;
                compressionQualityData.CompressionQuality = CompressionQuality;

                compressionQualityData.UseOverride = UseOverride;
                compressionQualityData.OverrideTextureFormat = OverrideTextureFormat;

            }
        }

        void ITextureFineTuning.AddSetting(Dictionary<string, TexFineTuningHolder> texFineTuningTargets)
        {
            AddSetting(texFineTuningTargets);
        }
    }
    internal class TextureCompressionTuningData : TextureCompressionData, ITuningData { }

    internal class CompressionQualityApplicant : ITuningProcessor
    {
        public int Order => 0;

        public void ProcessingTuning(TexFineTuningProcessingContext ctx)
        {
            foreach (var tuning in ctx.TuningHolder)
            {
                var tuningHolder = tuning.Value;
                var compressSetting = tuningHolder.Find<TextureCompressionTuningData>();
                if (compressSetting == null) { continue; }

                ctx.ProcessingHolder[tuning.Key].TextureDescriptor.TextureFormat = compressSetting;
            }
        }
    }

}
