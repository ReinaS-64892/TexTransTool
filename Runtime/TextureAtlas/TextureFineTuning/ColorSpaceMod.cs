using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace net.rs64.TexTransTool.TextureAtlas.FineSetting
{
    internal struct ColorSpaceMod : IAddFineTuning
    {
        public string PropertyNames;
        public PropertySelect Select;
        public bool Linier;

        public ColorSpaceMod(string propertyNames, PropertySelect select, bool linier)
        {
            PropertyNames = propertyNames;
            Select = select;
            Linier = linier;
        }

        public void AddSetting(List<TexFineTuningTarget> propAndTextures)
        {
            foreach (var target in FineSettingUtil.FilteredTarget(PropertyNames, Select, propAndTextures))
            {
                var colorSpaceData = target.TuningDataList.Find(I => I is ColorSpaceData) as ColorSpaceData;
                if (colorSpaceData != null)
                {
                    colorSpaceData.Linier = Linier;
                }
                else
                {
                    target.TuningDataList.Add(new ColorSpaceData() { Linier = Linier });
                }
            }

        }
    }

    internal class ColorSpaceData : ITuningData
    {
        public bool Linier = true;
    }

    internal class ColorSpaceApplicant : ITuningApplicant
    {
        public int Order => -34;

        public void ApplyTuning(List<TexFineTuningTarget> texFineTuningTargets)
        {
            foreach (var texf in texFineTuningTargets)
            {
                var colorSpaceData = texf.TuningDataList.Find(I => I is ColorSpaceData) as ColorSpaceData;
                if (colorSpaceData == null) { continue; }
                if (colorSpaceData.Linier == !texf.Texture2D.isDataSRGB) { continue; }
                // GraphicsFormatUtility.GetLinearFormat()
                // GraphicsFormatUtility.IsSRGBFormat()

                var newTex = new Texture2D(texf.Texture2D.width, texf.Texture2D.height, TextureFormat.RGBA32, texf.Texture2D.mipmapCount > 1, colorSpaceData.Linier);
                var pixelData = texf.Texture2D.GetPixelData<Color32>(0);
                newTex.SetPixelData(pixelData, 0); pixelData.Dispose();
                newTex.Apply();
                newTex.name = texf.Texture2D.name;
                texf.Texture2D = newTex;
            }
        }
    }
}