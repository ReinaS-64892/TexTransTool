using System;

namespace net.rs64.MultiLayerImage.Parser.PSD.AdditionalLayerInfo
{
    [Serializable, AdditionalLayerInfoParser("lnsr")]
    public class lnsr : AdditionalLayerInfoBase
    {
        public int IDForLayerName;

        public override void ParseAddLY(bool isPSB,BinarySectionStream stream)
        {
            IDForLayerName = stream.ReadInt32();
        }
    }
    /*
    レイヤー名のソースセッティングらしいが、謎
    */

}
