using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using net.rs64.ParserUtility;

namespace net.rs64.PSDParser
{
    public static class PSDParserImageResourceBlocksParser
    {
        [Serializable]
        public class ImageResourceBlock
        {
            public ushort UniqueIdentifier;
            public string PascalStringName;
            public uint ActualDataSizeFollows;
            public BinaryAddress ActualDataSizeFollowsAddress;
        }
        public static List<ImageResourceBlock> PaseImageResourceBlocks(BinarySectionStream stream)
        {
            var imageResourceBlockList = new List<ImageResourceBlock>();

            while (stream.Position < stream.Length)
            {
                if (stream.Signature(PSDLowLevelParser.OctBIMSignature) is false) { throw new Exception(); }
                var nowIRB = new ImageResourceBlock();

                nowIRB.UniqueIdentifier = stream.ReadUInt16();

                var strLength = stream.ReadByte();
                if (strLength == 0)
                {
                    stream.ReadByte();
                    nowIRB.PascalStringName = null;
                }
                else
                {
                    Span<byte> pascalStrBuf = stackalloc byte[strLength];
                    stream.ReadToSpan(pascalStrBuf);
                    nowIRB.PascalStringName = Encoding.GetEncoding("shift-jis").GetString(pascalStrBuf);

                    if (strLength % 2 == 0) { stream.ReadByte(); }
                    else { /* null文字は偶数の時にしか存在しないらしい...??????????*/  }
                }

                nowIRB.ActualDataSizeFollows = stream.ReadUInt32();
                //ここなぜか 2の倍数にパディング入れないといけないの謎
                var actualLength = nowIRB.ActualDataSizeFollows % 2 == 0 ? nowIRB.ActualDataSizeFollows : nowIRB.ActualDataSizeFollows + 1;
                nowIRB.ActualDataSizeFollowsAddress = stream.ReadToAddress(actualLength);

                imageResourceBlockList.Add(nowIRB);
            }

            return imageResourceBlockList;
        }
    }
}
