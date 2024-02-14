using System;

namespace net.rs64.MultiLayerImage.Parser.PSD
{
    internal static partial class AdditionalLayerInformationParser
    {
        [Serializable, AdditionalLayerInfoParser("tsly")]
        internal class tsly : AdditionalLayerInfo
        {
            public bool TransparencyShapesLayer;

            public override void ParseAddLY(SubSpanStream stream)
            {
                TransparencyShapesLayer = stream.ReadByte() == 1;

                //Padding
                stream.ReadByte();
                stream.ReadByte();
                stream.ReadByte();
            }
        }
        /*
        レイヤー名のソースセッティングらしいが、謎
        */

    }
}