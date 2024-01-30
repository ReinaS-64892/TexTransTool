using System;

namespace net.rs64.MultiLayerImageParser.PSD
{
    internal static partial class AdditionalLayerInformationParser
    {
        [Serializable, AdditionalLayerInfoParser("lnsr")]
        internal class lnsr : AdditionalLayerInfo
        {
            public int IDForLayerName;

            public override void ParseAddLY(SubSpanStream stream)
            {
                IDForLayerName = stream.ReadInt32();
            }
        }
        /*
        レイヤー名のソースセッティングらしいが、謎
        */

    }
}