using System;
using net.rs64.ParserUtility;

namespace net.rs64.PSDParser.AdditionalLayerInfo
{

    [Serializable, AdditionalLayerInfoParser("lsct")]
    public class lsct : AdditionalLayerInfoBase
    {
        public SelectionDividerTypeEnum SelectionDividerType;
        public string BlendModeKey;
        public uint SubType;

        public enum SelectionDividerTypeEnum
        {
            AnyOther = 0,
            OpenFolder = 1,
            ClosedFolder = 2,
            BoundingSectionDivider = 3,
        }

        public override void ParseAddLY(bool isPSB,BinarySectionStream stream)
        {
            SelectionDividerType = (lsct.SelectionDividerTypeEnum)stream.ReadUInt32();
            if (Address.Length >= 12)
            {
                stream.ReadSubSection(4);

                Span<byte> keyBuf = stackalloc byte[4];
                stream.ReadToSpan(keyBuf);
                BlendModeKey = keyBuf.ParseASCII();
            }
            if (Address.Length >= 16)
            {
                SubType = stream.ReadUInt32();
            }
        }


        /*
        PSDにはフォルダという概念はなく、この追加情報を持った空のレイヤーでいい感じに囲んで誤魔化すことによって実現されているようだ。
        */

    }
    [Serializable, AdditionalLayerInfoParser("lsdk")] internal class lsdk : lsct { }
    //謎のスペックに存在しない別の名前のやつ...スペックが更新されてないからそれにないんだろうね
}
