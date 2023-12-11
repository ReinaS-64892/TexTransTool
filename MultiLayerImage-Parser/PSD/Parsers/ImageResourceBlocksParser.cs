using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace net.rs64.MultiLayerImageParser.PSD
{
    internal static class PSDParserImageResourceBlocksParser
    {
        [Serializable]
        internal class ImageResourceBlock
        {
            public ushort UniqueIdentifier;
            public string PascalStringName;
            public uint ActualDataSizeFollows;
            public byte[] ResourceData;
        }
        public static List<ImageResourceBlock> PaseImageResourceBlocks(SubSpanStream stream)
        {
            var imageResourceBlockList = new List<ImageResourceBlock>();

            while (stream.Position < stream.Length)
            {
                if (!ParserUtility.Signature(ref stream, PSDLowLevelParser.OctBIMSignature)) { throw new Exception(); }
                var nowIRB = new ImageResourceBlock();

                nowIRB.UniqueIdentifier = stream.ReadUInt16();
                nowIRB.PascalStringName = ParserUtility.ReadPascalString(ref stream);

                nowIRB.ActualDataSizeFollows = stream.ReadUInt32();
                nowIRB.ResourceData = stream.ReadSubStream((int)nowIRB.ActualDataSizeFollows).Span.ToArray();

                imageResourceBlockList.Add(nowIRB);
            }

            return imageResourceBlockList;
        }
    }
}