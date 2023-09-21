
using System;
using UnityEngine;

namespace net.rs64.PSD.parser
{

    [Serializable]
    public abstract class AbstractLayer
    {
        public string LayerName;
        public bool TransparencyProtected;
        public bool Visible;
        public float Opacity;
        public bool Clipping;
        public PSDBlendMode BlendMode;
        public LayerMask LayerMask;

        public void CopyFromRecord(LayerRecordParser.LayerRecord layerRecord)
        {
            LayerName = layerRecord.LayerName;
            TransparencyProtected = layerRecord.LayerFlag.HasFlag(LayerRecordParser.LayerRecord.LayerFlagEnum.TransparencyProtected);
            Visible = !layerRecord.LayerFlag.HasFlag(LayerRecordParser.LayerRecord.LayerFlagEnum.NotVisible);
            Opacity = (float)layerRecord.Opacity / byte.MaxValue;
            Clipping = layerRecord.Clipping != 0;
            BlendMode = BlendModeKeyToEnum(layerRecord.BlendModeKey);
        }

        private PSDBlendMode BlendModeKeyToEnum(string blendModeStr)
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
    }

    public class LayerMask
    {
        public bool LayerMaskDisabled;
        public Texture2D MaskTexture;
        public Vector2Int MaskPivot;
    }

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
        Overlay, SoftLight,
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
}
