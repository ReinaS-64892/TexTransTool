using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static net.rs64.MultiLayerImageParser.PSD.GlobalLayerMaskInformationParser;
using static net.rs64.MultiLayerImageParser.PSD.LayerInformationParser;
using static net.rs64.MultiLayerImageParser.PSD.PSDParserImageResourceBlocksParser;

namespace net.rs64.MultiLayerImageParser.PSD
{

    //https://www.adobe.com/devnet-apps/photoshop/fileformatashtml/

    internal static class PSDLowLevelParser
    {
        public static readonly byte[] OctBPSSignature = new byte[] { 0x38, 0x42, 0x50, 0x53 };
        public static readonly byte[] OctBIMSignature = new byte[] { 0x38, 0x42, 0x49, 0x4D };
        public static PSDLowLevelData Parse(string path)
        {
            return Parse(File.ReadAllBytes(path));
        }
        public static PSDLowLevelData Parse(byte[] psdByte)
        {
            var psd = new PSDLowLevelData();

            // Signature ...

            var spanStream = new SubSpanStream(psdByte.AsSpan());

            if (!spanStream.ReadSubStream(4).Span.SequenceEqual(OctBPSSignature)) { throw new System.Exception(); }
            if (!spanStream.ReadSubStream(2).Span.SequenceEqual(new byte[] { 0x00, 0x01 })) { throw new System.Exception(); }
            if (!spanStream.ReadSubStream(6).Span.SequenceEqual(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 })) { throw new System.Exception(); }

            // File Header Section

            psd.channels = spanStream.ReadUInt16();
            psd.height = spanStream.ReadUInt32();
            psd.width = spanStream.ReadUInt32();
            psd.Depth = spanStream.ReadUInt16();
            psd.ColorMode = (PSDLowLevelData.ColorModeEnum)spanStream.ReadUInt16();

            // Color Mode Data Section

            psd.ColorModeDataSectionLength = spanStream.ReadUInt32();
            psd.ColorData = spanStream.ReadSubStream((int)psd.ColorModeDataSectionLength).Span.ToArray();

            // Image Resources Section

            psd.ImageResourcesSectionLength = spanStream.ReadUInt32();
            psd.ImageResources = PaseImageResourceBlocks(spanStream.ReadSubStream((int)psd.ImageResourcesSectionLength));

            // LayerAndMaskInformationSection

            psd.LayerAndMaskInformationSectionLength = spanStream.ReadUInt32();
            psd.LayerInfo = LayerInformationParser.PaseLayerInfo(spanStream.ReadSubStream((int)psd.LayerAndMaskInformationSectionLength));

            return psd;
        }


        [Serializable]
        internal class PSDLowLevelData : IDisposable
        {
            // File Header Section
            public ushort channels;
            public uint height;
            public uint width;
            public ushort Depth;
            public ColorModeEnum ColorMode;
            internal enum ColorModeEnum : ushort
            {
                Bitmap = 0,
                Grayscale = 1,
                Indexed = 2,
                RGB = 3,
                CMYK = 4,
                Multichannel = 7,
                Duotone = 8,
                Lab = 9,
            }

            // Color Mode Data Section

            public uint ColorModeDataSectionLength;
            public byte[] ColorData;

            // Image Resources Section

            public uint ImageResourcesSectionLength;
            public List<ImageResourceBlock> ImageResources;

            // LayerAndMaskInformationSection

            public uint LayerAndMaskInformationSectionLength;

            public LayerInfo LayerInfo;
            public GlobalLayerMaskInfo GlobalLayerMaskInfo;

            public void Dispose()
            {
                LayerInfo.Dispose();
            }
        }
    }
}