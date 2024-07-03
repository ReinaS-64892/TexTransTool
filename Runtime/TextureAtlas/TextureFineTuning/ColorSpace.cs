using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace net.rs64.TexTransTool.TextureAtlas.FineTuning
{
    [Serializable]
    public class ColorSpace : ITextureFineTuning
    {
        public PropertyName PropertyNames;
        public PropertySelect Select;
        public bool Linear;

        public ColorSpace() { }
        public ColorSpace(PropertyName propertyNames, PropertySelect select, bool linear)
        {
            PropertyNames = propertyNames;
            Select = select;
            Linear = linear;
        }
        public static ColorSpace Default => new(PropertyName.DefaultValue, PropertySelect.Equal, true);

        public void AddSetting(Dictionary<string, TexFineTuningHolder> texFineTuningTargets)
        {
            foreach (var target in FineTuningUtil.FilteredTarget(PropertyNames, Select, texFineTuningTargets))
            {
                var tuningDataHolder = target.Value;
                tuningDataHolder.Get<ColorSpaceData>().Linear = Linear;
            }

        }
    }

    internal class ColorSpaceData : ITuningData
    {
        public bool Linear = true;
    }

    internal class ColorSpaceApplicant : ITuningApplicant
    {
        public int Order => -34;

        public void ApplyTuning(Dictionary<string, TexFineTuningHolder> texFineTuningTargets)
        {
            foreach (var texf in texFineTuningTargets)
            {
                var tuningHolder = texf.Value;
                var colorSpaceData = tuningHolder.Find<ColorSpaceData>();
                if (colorSpaceData == null) { continue; }
                if (colorSpaceData.Linear == !tuningHolder.Texture2D.isDataSRGB) { continue; }
                // GraphicsFormatUtility.GetLinearFormat()
                // GraphicsFormatUtility.IsSRGBFormat()

                var newTex = new Texture2D(tuningHolder.Texture2D.width, tuningHolder.Texture2D.height, TextureFormat.RGBA32, tuningHolder.Texture2D.mipmapCount > 1, colorSpaceData.Linear);
                var pixelData = tuningHolder.Texture2D.GetPixelData<Color32>(0);
                newTex.SetPixelData(pixelData, 0); pixelData.Dispose();
                newTex.Apply();
                newTex.name = tuningHolder.Texture2D.name;
                tuningHolder.Texture2D = newTex;
            }
        }
    }
}
