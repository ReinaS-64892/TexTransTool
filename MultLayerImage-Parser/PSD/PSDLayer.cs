using System;
using net.rs64.TexTransCore.BlendTexture;
using net.rs64.TexTransCore.Layer;

namespace net.rs64.PSD.parser
{


    public enum PSDBlendMode
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
    public static class PSDLayer
    {

        public static void CopyFromRecord(this AbstractLayer abstractLayer, LayerRecordParser.LayerRecord layerRecord)
        {
            abstractLayer.LayerName = layerRecord.LayerName;
            abstractLayer.TransparencyProtected = layerRecord.LayerFlag.HasFlag(LayerRecordParser.LayerRecord.LayerFlagEnum.TransparencyProtected);
            abstractLayer.Visible = !layerRecord.LayerFlag.HasFlag(LayerRecordParser.LayerRecord.LayerFlagEnum.NotVisible);
            abstractLayer.Opacity = (float)layerRecord.Opacity / byte.MaxValue;
            abstractLayer.Clipping = layerRecord.Clipping != 0;
            abstractLayer.BlendMode = ConvertBlendType(BlendModeKeyToEnum(layerRecord.BlendModeKey));
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

        public static BlendType ConvertBlendType(PSDBlendMode pSDBlendMode)
        {
            switch (pSDBlendMode)
            {
                // case PSDBlendMode.PassThrough:
                //     return BlendType;
                case PSDBlendMode.Normal:
                    return BlendType.Normal;
                // case PSDBlendMode.Dissolve:
                //     return BlendType;
                case PSDBlendMode.Darken:
                    return BlendType.DarkenOnly;
                case PSDBlendMode.Multiply:
                    return BlendType.Mul;
                case PSDBlendMode.ColorBurn:
                    return BlendType.ColorBurn;
                case PSDBlendMode.LinearBurn:
                    return BlendType.LinearBurn;
                // case PSDBlendMode.DarkerColor:
                //     return BlendType;
                case PSDBlendMode.Lighten:
                    return BlendType.LightenOnly;
                case PSDBlendMode.Screen:
                    return BlendType.Screen;
                case PSDBlendMode.ColorDodge:
                    return BlendType.ColorDodge;
                // case PSDBlendMode.LinearDodge:
                //     return BlendType;
                // case PSDBlendMode.LighterColor:
                //     return BlendType;
                case PSDBlendMode.Overlay:
                    return BlendType.Overlay;
                case PSDBlendMode.SoftLight:
                    return BlendType.SoftLight;
                case PSDBlendMode.HardLight:
                    return BlendType.HardLight;
                case PSDBlendMode.VividLight:
                    return BlendType.VividLight;
                case PSDBlendMode.LinearLight:
                    return BlendType.LinearLight;
                // case PSDBlendMode.PinLight:
                //     return BlendType;
                // case PSDBlendMode.HardMix:
                //     return BlendType;
                case PSDBlendMode.Difference:
                    return BlendType.Difference;
                // case PSDBlendMode.Exclusion:
                //     return BlendType;
                case PSDBlendMode.Divide:
                    return BlendType.Divide;
                case PSDBlendMode.Hue:
                    return BlendType.Hue;
                case PSDBlendMode.Saturation:
                    return BlendType.Saturation;
                case PSDBlendMode.Color:
                    return BlendType.Color;
                case PSDBlendMode.Luminosity:
                    return BlendType.Luminosity;
                default:
                    throw new NotSupportedException();
            }
        }
    }
}