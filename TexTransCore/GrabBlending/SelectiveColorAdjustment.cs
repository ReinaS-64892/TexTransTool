#nullable enable
using System;
using System.Numerics;

namespace net.rs64.TexTransCore.MultiLayerImageCanvas
{
    public class SelectiveColorAdjustment : ITTGrabBlending
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
        public SelectiveColorAdjustment(Vector4 red, Vector4 yellow, Vector4 green, Vector4 cyan, Vector4 blue, Vector4 magenta, Vector4 white, Vector4 neutral, Vector4 black, bool isAbsolute)
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
