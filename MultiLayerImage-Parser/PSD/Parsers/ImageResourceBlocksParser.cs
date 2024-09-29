using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace net.rs64.MultiLayerImage.Parser.PSD
{
    internal static class PSDParserImageResourceBlocksParser
    {
        [Serializable]
        internal class ImageResourceBlock
        {
            public ushort UniqueIdentifier;
            public string PascalStringName;
            public uint ActualDataSizeFollows;
            public uint ActualDataSizeFollowsPlusPadding;
            public long ActualDataStartIndex;
        }
        public static List<ImageResourceBlock> PaseImageResourceBlocks(SubSpanStream stream)
        {
            var imageResourceBlockList = new List<ImageResourceBlock>();

            while (stream.Position < stream.Length)
            {
                if (!ParserUtility.Signature(ref stream, PSDLowLevelParser.OctBIMSignature)) { throw new Exception(); }
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
                    nowIRB.PascalStringName = Encoding.GetEncoding("shift-jis").GetString(stream.ReadSubStream(strLength).Span);
                    if (strLength % 2 == 0) { stream.ReadByte(); }
                    else { /* null文字は偶数の時にしか存在しないらしい...??????????*/  }
                }

                nowIRB.ActualDataSizeFollows = stream.ReadUInt32();
                //ここなぜか 2の倍数にパディング入れないといけないの謎
                var actualLength = nowIRB.ActualDataSizeFollows % 2 == 0 ? nowIRB.ActualDataSizeFollows : nowIRB.ActualDataSizeFollows + 1;
                nowIRB.ActualDataSizeFollowsPlusPadding = actualLength;
                nowIRB.ActualDataStartIndex = stream.ReadSubStream((int)actualLength).FirstToPosition;

                imageResourceBlockList.Add(nowIRB);
            }

            return imageResourceBlockList;
        }
    }
}
