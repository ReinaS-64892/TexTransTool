using System;
using net.rs64.MultiLayerImage.LayerData;
using System.Linq;
using System.Collections.Generic;
using static net.rs64.MultiLayerImage.Parser.PSD.ChannelImageDataParser.ChannelInformation;
using static net.rs64.MultiLayerImage.Parser.PSD.ChannelImageDataParser;
using net.rs64.TexTransUnityCore.BlendTexture;

namespace net.rs64.MultiLayerImage.Parser.PSD
{


    internal enum PSDBlendMode
    {
        PassThrough,
        Normal,
        Dissolve,
        Darken,
        Multiply,
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
            abstractLayer.BlendTypeKey = ResolveGlow(ConvertBlendType(BlendModeKeyToEnum(layerRecord.BlendModeKey)), layerRecord.AdditionalLayerInformation).ToString();
            abstractLayer.LayerMask = PSDHighLevelParser.ParseLayerMask(layerRecord, channelImageData);

        }

        private static TTTBlendTypeKeyEnum ResolveGlow(TTTBlendTypeKeyEnum tttBlendTypeKeyEnum, AdditionalLayerInfo.AdditionalLayerInfoBase[] additionalLayerInformation)
        {
            switch (tttBlendTypeKeyEnum)
            {
                default: return tttBlendTypeKeyEnum;

                case TTTBlendTypeKeyEnum.Addition:
                    {
                        var tsly = GetTransparencyShapesLayerData(additionalLayerInformation);
                        if (tsly == null || tsly.TransparencyShapesLayer == true) { return tttBlendTypeKeyEnum; }
                        return TTTBlendTypeKeyEnum.AdditionGlow;
                    }
                case TTTBlendTypeKeyEnum.ColorDodge:
                    {
                        var tsly = GetTransparencyShapesLayerData(additionalLayerInformation);
                        if (tsly == null || tsly.TransparencyShapesLayer == true) { return tttBlendTypeKeyEnum; }
                        return TTTBlendTypeKeyEnum.ColorDodgeGlow;
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
                    return PSDBlendMode.Multiply;
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

        public static TTTBlendTypeKeyEnum ConvertBlendType(PSDBlendMode pSDBlendMode)
        {
            switch (pSDBlendMode)
            {
                default:
                // case PSDBlendMode.PassThrough:
                //     return TTTBlendTypeKeyEnum;
                case PSDBlendMode.Normal:
                    return TTTBlendTypeKeyEnum.Normal;
                case PSDBlendMode.Dissolve:
                    return TTTBlendTypeKeyEnum.Dissolve;
                case PSDBlendMode.Darken:
                    return TTTBlendTypeKeyEnum.DarkenOnly;
                case PSDBlendMode.Multiply:
                    return TTTBlendTypeKeyEnum.Mul;
                case PSDBlendMode.ColorBurn:
                    return TTTBlendTypeKeyEnum.ColorBurn;
                case PSDBlendMode.LinearBurn:
                    return TTTBlendTypeKeyEnum.LinearBurn;
                case PSDBlendMode.DarkerColor:
                    return TTTBlendTypeKeyEnum.DarkenColorOnly;
                case PSDBlendMode.Lighten:
                    return TTTBlendTypeKeyEnum.LightenOnly;
                case PSDBlendMode.Screen:
                    return TTTBlendTypeKeyEnum.Screen;
                case PSDBlendMode.ColorDodge:
                    return TTTBlendTypeKeyEnum.ColorDodge;
                case PSDBlendMode.LinearDodge:
                    return TTTBlendTypeKeyEnum.Addition;
                case PSDBlendMode.LighterColor:
                    return TTTBlendTypeKeyEnum.LightenColorOnly;
                case PSDBlendMode.Overlay:
                    return TTTBlendTypeKeyEnum.Overlay;
                case PSDBlendMode.SoftLight:
                    return TTTBlendTypeKeyEnum.SoftLight;
                case PSDBlendMode.HardLight:
                    return TTTBlendTypeKeyEnum.HardLight;
                case PSDBlendMode.VividLight:
                    return TTTBlendTypeKeyEnum.VividLight;
                case PSDBlendMode.LinearLight:
                    return TTTBlendTypeKeyEnum.LinearLight;
                case PSDBlendMode.PinLight:
                    return TTTBlendTypeKeyEnum.PinLight;
                case PSDBlendMode.HardMix:
                    return TTTBlendTypeKeyEnum.HardMix;
                case PSDBlendMode.Difference:
                    return TTTBlendTypeKeyEnum.Difference;
                case PSDBlendMode.Exclusion:
                    return TTTBlendTypeKeyEnum.Exclusion;
                case PSDBlendMode.Divide:
                    return TTTBlendTypeKeyEnum.Divide;
                case PSDBlendMode.Hue:
                    return TTTBlendTypeKeyEnum.Hue;
                case PSDBlendMode.Saturation:
                    return TTTBlendTypeKeyEnum.Saturation;
                case PSDBlendMode.Color:
                    return TTTBlendTypeKeyEnum.Color;
                case PSDBlendMode.Luminosity:
                    return TTTBlendTypeKeyEnum.Luminosity;
            }
        }
    }
}
