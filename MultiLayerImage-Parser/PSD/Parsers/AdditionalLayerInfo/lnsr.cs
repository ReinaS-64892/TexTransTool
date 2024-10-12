using System;

namespace net.rs64.MultiLayerImage.Parser.PSD.AdditionalLayerInfo
{
    [Serializable, AdditionalLayerInfoParser("lnsr")]
    internal class lnsr : AdditionalLayerInfoBase
    {
        public int IDForLayerName;

        public override void ParseAddLY(bool isPSB,SubSpanStream stream)
        {
            IDForLayerName = stream.ReadInt32();
        }
    }
    /*
    レイヤー名のソースセッティングらしいが、謎
    */

}
