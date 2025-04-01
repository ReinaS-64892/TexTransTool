#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Serialization;

namespace net.rs64.TexTransTool.TextureAtlas.FineTuning
{
    [Serializable]
    public class ColorSpace : ITextureFineTuning
    {
        [FormerlySerializedAs("Linear")] public bool AsLinear = true;

        [Obsolete("V4SaveData", true)] public PropertyName PropertyNames = PropertyName.DefaultValue;
        public List<PropertyName> PropertyNameList = new() { PropertyName.DefaultValue };
        public PropertySelect Select = PropertySelect.Equal;

        public ColorSpace() { }
        [Obsolete("V4SaveData", true)]
        public ColorSpace(PropertyName propertyNames, PropertySelect select, bool linear)
        {
            PropertyNames = propertyNames;
            Select = select;
            AsLinear = linear;
        }
        public ColorSpace(List<PropertyName> propertyNames, PropertySelect select, bool linear)
        {
            PropertyNameList = propertyNames;
            Select = select;
            AsLinear = linear;
        }
        void AddSetting(Dictionary<string, TexFineTuningHolder> texFineTuningTargets)
        {
            foreach (var target in FineTuningUtil.FilteredTarget(PropertyNameList, Select, texFineTuningTargets))
            {
                var tuningDataHolder = target.Value;
                tuningDataHolder.Get<ColorSpaceData>().AsLinear = AsLinear;
            }

        }
        void ITextureFineTuning.AddSetting(Dictionary<string, TexFineTuningHolder> texFineTuningTargets)
        {
            AddSetting(texFineTuningTargets);
        }
    }

    internal class ColorSpaceData : ITuningData
    {
        public bool AsLinear = false;
    }

    internal class ColorSpaceApplicant : ITuningProcessor
    {
        public int Order => -34;

        public void ProcessingTuning(TexFineTuningProcessingContext ctx)
        {
            foreach (var tuning in ctx.TuningHolder)
            {
                var tuningHolder = tuning.Value;
                var colorSpaceData = tuningHolder.Find<ColorSpaceData>();
                if (colorSpaceData == null) { continue; }
                // GraphicsFormatUtility.GetLinearFormat()
                // GraphicsFormatUtility.IsSRGBFormat()

                ctx.ProcessingHolder[tuning.Key].TextureDescriptor.AsLinear = colorSpaceData.AsLinear;

                // var newTex = new Texture2D(tuningHolder.Texture2D.width, tuningHolder.Texture2D.height, TextureFormat.RGBA32, tuningHolder.Texture2D.mipmapCount > 1, colorSpaceData.AsLinear);
                // var pixelData = tuningHolder.Texture2D.GetPixelData<Color32>(0);
                // newTex.SetPixelData(pixelData, 0); pixelData.Dispose();
                // newTex.Apply();
                // newTex.name = tuningHolder.Texture2D.name;
                // tuningHolder.Texture2D = newTex;
            }
        }
    }
}
