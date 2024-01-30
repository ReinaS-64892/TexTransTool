using System;

namespace net.rs64.MultiLayerImage.Parser.PSD
{
    internal static partial class AdditionalLayerInformationParser
    {
        [Serializable, AdditionalLayerInfoParser("lsct")]
        internal class lsct : AdditionalLayerInfo
        {
            public SelectionDividerTypeEnum SelectionDividerType;
            public string BlendModeKey;
            public int SubType;

            public enum SelectionDividerTypeEnum
            {
                AnyOther = 0,
                OpenFolder = 1,
                ClosedFolder = 2,
                BoundingSectionDivider = 3,
            }

            public override void ParseAddLY(SubSpanStream stream)
            {
                SelectionDividerType = (lsct.SelectionDividerTypeEnum)stream.ReadUInt32();
                if (Length >= 12)
                {
                    stream.ReadSubStream(4);
                    BlendModeKey = stream.ReadSubStream(4).Span.ParseUTF8();
                }
                if (Length >= 16)
                {
                    SubType = stream.ReadInt32();
                }
            }
        }

        /*
        PSDにはフォルダという概念はなく、この追加情報を持った空のレイヤーでいい感じに囲んで誤魔化すことによって実現されているようだ。
        */

    }
}