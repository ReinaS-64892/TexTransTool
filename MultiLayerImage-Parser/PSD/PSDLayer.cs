using System;
using net.rs64.MultiLayerImage.LayerData;
using System.Linq;
using System.Collections.Generic;
using static net.rs64.MultiLayerImage.Parser.PSD.ChannelImageDataParser.ChannelInformation;
using static net.rs64.MultiLayerImage.Parser.PSD.ChannelImageDataParser;

namespace net.rs64.MultiLayerImage.Parser.PSD
{


    internal enum PSDBlendMode //TTTの物と揃っている
    {
        PassThrough,
        Normal,
        Dissolve,
        Darken,
        Mul,
        ColorBurn,
        LinearBurn,
        DarkerColor,
        Lighten,
        Screen,
        ColorDodge,
        LinearDodge,
        LighterColor,
        Overlay,
        SoftLight,
        HardLight,
        VividLight,
        LinearLight,
        PinLight,
        HardMix,
        Difference,
        Exclusion,
        Subtract,
        Divide,
        Hue,
        Saturation,
        Color,
        Luminosity,
    }
    [Serializable]
    internal static class PSDLayer
    {

        public static void CopyFromRecord(this AbstractLayerData abstractLayer, LayerRecordParser.LayerRecord layerRecord, Dictionary<ChannelIDEnum, ChannelImageData> channelImageData)
        {
            abstractLayer.LayerName = layerRecord.LayerName;
            abstractLayer.TransparencyProtected = layerRecord.LayerFlag.HasFlag(LayerRecordParser.LayerRecord.LayerFlagEnum.TransparencyProtected);
            abstractLayer.Visible = layerRecord.LayerFlag.HasFlag(LayerRecordParser.LayerRecord.LayerFlagEnum.NotVisible) is false;
            abstractLayer.Opacity = (float)layerRecord.Opacity / byte.MaxValue;
            abstractLayer.Clipping = layerRecord.Clipping != 0;
            abstractLayer.BlendTypeKey = BlendModeKeyToEnum(layerRecord.BlendModeKey).ToString();
            abstractLayer.LayerMask = PSDHighLevelParser.ParseLayerMask(layerRecord, channelImageData);

        }

        internal static string ResolveGlow(string blendTypeKey, AdditionalLayerInfo.AdditionalLayerInfoBase[] additionalLayerInformation)
        {
            switch (blendTypeKey)
            {
                default: return blendTypeKey;

                case "Addition":
                case "ColorDodge":
                    {
                        var tsly = GetTransparencyShapesLayerData(additionalLayerInformation);
                        if (tsly == null || tsly.TransparencyShapesLayer == true) { return blendTypeKey; }
                        return blendTypeKey + "Glow";
                    }
            }

            AdditionalLayerInfo.tsly GetTransparencyShapesLayerData(AdditionalLayerInfo.AdditionalLayerInfoBase[] additionalLayerInformation)
            {
                return additionalLayerInformation.FirstOrDefault(IsTSLY) as AdditionalLayerInfo.tsly;
                static bool IsTSLY(AdditionalLayerInfo.AdditionalLayerInfoBase additionalLayerInfo) => additionalLayerInfo is AdditionalLayerInfo.tsly;
            }
        }

        public static PSDBlendMode BlendModeKeyToEnum(string blendModeStr)
        {
            switch (blendModeStr.ToLower())
            {
                case "pass":
                    return PSDBlendMode.PassThrough;
                case "norm":
                    return PSDBlendMode.Normal;
                case "diss":
                    return PSDBlendMode.Dissolve;
                case "dark":
                    return PSDBlendMode.Darken;
                case "mul ":
                    return PSDBlendMode.Mul;
                case "idiv":
                    return PSDBlendMode.ColorBurn;
                case "lbrn":
                    return PSDBlendMode.LinearBurn;
                case "dkcl":
                    return PSDBlendMode.DarkerColor;
                case "lite":
                    return PSDBlendMode.Lighten;
                case "scrn":
                    return PSDBlendMode.Screen;
                case "div ":
                    return PSDBlendMode.ColorDodge;
                case "lddg":
                    return PSDBlendMode.LinearDodge;
                case "lgcl":
                    return PSDBlendMode.LighterColor;
                case "over":
                    return PSDBlendMode.Overlay;
                case "slit":
                    return PSDBlendMode.SoftLight;
                case "hlit":
                    return PSDBlendMode.HardLight;
                case "vlit":
                    return PSDBlendMode.VividLight;
                case "llit":
                    return PSDBlendMode.LinearLight;
                case "plit":
                    return PSDBlendMode.PinLight;
                case "hmix":
                    return PSDBlendMode.HardMix;
                case "diff":
                    return PSDBlendMode.Difference;
                case "smud":
                    return PSDBlendMode.Exclusion;
                case "fsub":
                    return PSDBlendMode.Subtract;
                case "fdiv":
                    return PSDBlendMode.Divide;
                case "hue ":
                    return PSDBlendMode.Hue;
                case "sat ":
                    return PSDBlendMode.Saturation;
                case "colr":
                    return PSDBlendMode.Color;
                case "lum ":
                    return PSDBlendMode.Luminosity;
                default:
                    throw new ArgumentException("Invalid blend mode string: " + blendModeStr);
            }
        }

    }
}
