using System;
using System.Collections.Generic;
using System.IO;

namespace net.rs64.PSD.parser
{
    public static class PSDParserImageResourceBlocksParser
    {
        [Serializable]
        public class ImageResourceBlock
        {
            public ushort UniqueIdentifier;
            public string PascalStringName;
            public uint ActualDataSizeFollows;
            public byte[] ResourceData;
        }
        public static ImageResourceBlock[] PaseImageResourceBlocks(SubSpanStream stream)
        {
            var ImageResourceBlockList = new List<ImageResourceBlock>();

            while (stream.Position < stream.Length)
            {
                if (!ParserUtility.Signature(stream, PSDLowLevelParser.OctBIMSignature)) { throw new Exception(); }
                var nowIRB = new ImageResourceBlock();

                nowIRB.UniqueIdentifier = stream.ReadUInt16();
                nowIRB.PascalStringName = ParserUtility.ReadPascalString(stream);

                nowIRB.ActualDataSizeFollows = stream.ReadUInt32();
                nowIRB.ResourceData = stream.ReadSubStream((int)nowIRB.ActualDataSizeFollows).Span.ToArray();

                ImageResourceBlockList.Add(nowIRB);
            }

            return ImageResourceBlockList.ToArray();
        }
    }
}