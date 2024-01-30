using System;

namespace net.rs64.MultiLayerImageParser.PSD
{
    internal static partial class AdditionalLayerInformationParser
    {
        [Serializable, AdditionalLayerInfoParser("luni")]
        internal class luni : AdditionalLayerInfo
        {
            public string LayerName;

            public override void ParseAddLY(SubSpanStream stream)
            {
                var byteLength = stream.ReadUInt32() * 2;
                LayerName = stream.ReadSubStream((int)byteLength).Span.ParseBigUTF16();
            }
        }
        /*
        パスカル文字列ではない、しっかりとしたレイヤーの名前、存在する場合はこっちが優先されるべきだろう。
        */

    }
}