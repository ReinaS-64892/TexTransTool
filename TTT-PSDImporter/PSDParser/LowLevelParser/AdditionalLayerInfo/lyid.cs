using System;
using net.rs64.ParserUtility;

namespace net.rs64.PSDParser.AdditionalLayerInfo
{
    [Serializable, AdditionalLayerInfoParser("lyid")]
    public class lyid : AdditionalLayerInfoBase
    {
        public int LayerID;

        public override void ParseAddLY(bool isPSB,BinarySectionStream stream)
        {
            LayerID = stream.ReadInt32();
        }
    }

    /*
    レイヤーを名前が変更されたとしても追跡できるID指定できる追加情報らしいが、クリスタはこれを出力していないためか、多くの PSD には存在しないためIDとして使う事はできない。
    */

}
