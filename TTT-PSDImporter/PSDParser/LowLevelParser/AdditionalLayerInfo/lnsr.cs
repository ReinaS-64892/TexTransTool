using System;
using net.rs64.ParserUtility;

namespace net.rs64.PSDParser.AdditionalLayerInfo
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
