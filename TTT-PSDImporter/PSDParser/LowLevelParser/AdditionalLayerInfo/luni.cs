using System;
using net.rs64.ParserUtility;

namespace net.rs64.PSDParser.AdditionalLayerInfo
{
    [Serializable, AdditionalLayerInfoParser("luni")]
    public class luni : AdditionalLayerInfoBase
    {
        public string LayerName;

        public override void ParseAddLY(bool isPSB,BinarySectionStream stream)
        {
            var byteLength = stream.ReadUInt32() * 2;
            var uniStrBuf = new byte[byteLength].AsSpan();
            stream.ReadToSpan(uniStrBuf);
            LayerName = uniStrBuf.ParseBigUTF16();
        }
    }
    /*
    パスカル文字列ではない、しっかりとしたレイヤーの名前、存在する場合はこっちが優先されるべきだろう。
    UnicodeString は末尾に 2Byte の null が含まれると PSD のスペックシートには書かれているが、あれは最近の PSD には存在しないため嘘である可能性が高い。
    おそらく、文字列が存在しないとき、0文字の場合に 空の文字が入っているため、それのことを指しているのかもしれない。
    */

}
