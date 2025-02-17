using System;
using net.rs64.ParserUtility;

namespace net.rs64.PSDParser.AdditionalLayerInfo
{
    [Serializable, AdditionalLayerInfoParser("tsly")]
    public class tsly : AdditionalLayerInfoBase
    {
        public bool TransparencyShapesLayer;

        public override void ParseAddLY(bool isPSB, BinarySectionStream stream)
        {
            TransparencyShapesLayer = stream.ReadByte() == 1;

            //Padding
            stream.ReadByte();
            stream.ReadByte();
            stream.ReadByte();
        }

        /*
        レイヤー名のソースセッティングらしいが、謎
        */

    }
}
