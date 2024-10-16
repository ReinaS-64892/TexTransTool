using System;

namespace net.rs64.MultiLayerImage.Parser.PSD.AdditionalLayerInfo
{
    [Serializable, AdditionalLayerInfoParser("luni")]
    internal class luni : AdditionalLayerInfoBase
    {
        public string LayerName;

        public override void ParseAddLY(bool isPSB,SubSpanStream stream)
        {
            var byteLength = stream.ReadUInt32() * 2;
            LayerName = stream.ReadSubStream((int)byteLength).Span.ParseBigUTF16();
        }
    }
    /*
    パスカル文字列ではない、しっかりとしたレイヤーの名前、存在する場合はこっちが優先されるべきだろう。
    UnicodeString は末尾に 2Byte の null が含まれると PSD のスペックシートには書かれているが、あれは最近の PSD には存在しないため嘘である可能性が高い。
    おそらく、文字列が存在しないとき、0文字の場合に 空の文字が入っているため、それのことを指しているのかもしれない。
    */

}
