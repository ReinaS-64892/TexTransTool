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
        public static ImageResourceBlock[] PaseImageResourceBlocks(byte[] InputBytes)
        {
            var stream = new MemoryStream(InputBytes);
            var ImageResourceBlockList = new List<ImageResourceBlock>();

            while (stream.Position < stream.Length)
            {
                if (!ParserUtility.Signature(stream, PSDLowLevelParser.OctBIMSignature)) { throw new Exception(); }
                var nowIRB = new ImageResourceBlock();

                nowIRB.UniqueIdentifier = stream.ReadByteToUInt16();
                nowIRB.PascalStringName = ParserUtility.ReadPascalString(stream);

                nowIRB.ActualDataSizeFollows = stream.ReadByteToUInt32();
                nowIRB.ResourceData = stream.ReadBytes(nowIRB.ActualDataSizeFollows);

                ImageResourceBlockList.Add(nowIRB);
            }

            return ImageResourceBlockList.ToArray();
        }
    }
}