using System;
using System.Numerics;
using net.rs64.TexTransCore;

namespace net.rs64.MultiLayerImage.Parser.PSD.AdditionalLayerInfo
{
    [Serializable, AdditionalLayerInfoParser("selc")]
    internal class selc : AdditionalLayerInfoBase
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
        public override void ParseAddLY(bool isPSB,SubSpanStream stream)
        {
            Debug.Assert(stream.ReadInt16() == 1);

            IsAbsolute = stream.ReadInt16() == 1;

            _ = ParseSelectiveColorRecord(ref stream);//予約領域

            RedsCMYK = ParseSelectiveColorRecord(ref stream);
            YellowsCMYK = ParseSelectiveColorRecord(ref stream);
            GreensCMYK = ParseSelectiveColorRecord(ref stream);
            CyansCMYK = ParseSelectiveColorRecord(ref stream);
            BluesCMYK = ParseSelectiveColorRecord(ref stream);
            MagentasCMYK = ParseSelectiveColorRecord(ref stream);
            WhitesCMYK = ParseSelectiveColorRecord(ref stream);
            NeutralsCMYK = ParseSelectiveColorRecord(ref stream);
            BlacksCMYK = ParseSelectiveColorRecord(ref stream);

            static Vector4 ParseSelectiveColorRecord(ref SubSpanStream subSpanStream)
            {
                return new Vector4(ParseAsFloat(subSpanStream.ReadInt16()), ParseAsFloat(subSpanStream.ReadInt16()), ParseAsFloat(subSpanStream.ReadInt16()), ParseAsFloat(subSpanStream.ReadInt16()));
                static float ParseAsFloat(short v) => v * 0.01f;
            }

        }
    }

    /*
    Selective Color Layer
    */

}
