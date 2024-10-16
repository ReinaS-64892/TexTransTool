#nullable enable
using System;

namespace net.rs64.TexTransCore.MultiLayerImageCanvas
{
    public class SelectiveColorAdjustment : TTGrabBlending
    {
        public Vector4 RedsCMYK;
        public Vector4 YellowsCMYK;
        public Vector4 GreensCMYK;
        public Vector4 CyansCMYK;
        public Vector4 BluesCMYK;
        public Vector4 MagentasCMYK;
        public Vector4 WhitesCMYK;
        public Vector4 NeutralsCMYK;
        public Vector4 BlacksCMYK;
        public bool IsAbsolute;
        public SelectiveColorAdjustment(ITTComputeKey computeKey, Vector4 red, Vector4 yellow, Vector4 green, Vector4 cyan, Vector4 blue, Vector4 magenta, Vector4 white, Vector4 neutral, Vector4 black, bool isAbsolute) : base(computeKey)
        {
            RedsCMYK = red;
            YellowsCMYK = yellow;
            GreensCMYK = green;
            CyansCMYK = cyan;
            BluesCMYK = blue;
            MagentasCMYK = magenta;
            WhitesCMYK = white;
            NeutralsCMYK = neutral;
            BlacksCMYK = black;
            IsAbsolute = isAbsolute;
        }

    }
}
