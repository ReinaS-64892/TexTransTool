using System;
using System.IO;
using System.Linq;
using static net.rs64.PSD.parser.GlobalLayerMaskInformationParser;
using static net.rs64.PSD.parser.LayerInformationParser;
using static net.rs64.PSD.parser.PSDParserImageResourceBlocksParser;

namespace net.rs64.PSD.parser
{

    //https://www.adobe.com/devnet-apps/photoshop/fileformatashtml/

    public static class PSDLowLevelParser
    {
        public static readonly byte[] OctBPSSignature = new byte[] { 0x38, 0x42, 0x50, 0x53 };
        public static readonly byte[] OctBIMSignature = new byte[] { 0x38, 0x42, 0x49, 0x4D };
        public static PSDLowLevelData Pase(string path)
        {
            return Pase(File.OpenRead(path));
        }
        public static PSDLowLevelData Pase(Stream stream)
        {
            var psd = new PSDLowLevelData();

            // Signature ...

            if (!stream.ReadBytes(4).SequenceEqual(OctBPSSignature)) { throw new System.Exception(); }
            if (!stream.ReadBytes(2).SequenceEqual(new byte[] { 0x00, 0x01 })) { throw new System.Exception(); }
            if (!stream.ReadBytes(6).SequenceEqual(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 })) { throw new System.Exception(); }

            // File Header Section

            psd.channels = stream.ReadByteToUInt16();
            psd.height = stream.ReadByteToUInt32();
            psd.width = stream.ReadByteToUInt32();
            psd.Depth = stream.ReadByteToUInt16();
            psd.ColorMode = (PSDLowLevelData.ColorModeEnum)stream.ReadByteToUInt16();

            // Color Mode Data Section

            psd.ColorModeDataSectionLength = stream.ReadByteToUInt32();
            psd.ColorData = stream.ReadBytes(psd.ColorModeDataSectionLength);

            // Image Resources Section

            psd.ImageResourcesSectionLength = stream.ReadByteToUInt32();
            psd.ImageResources = PaseImageResourceBlocks(stream.ReadBytes(psd.ImageResourcesSectionLength));

            // LayerAndMaskInformationSection

            psd.LayerAndMaskInformationSectionLength = stream.ReadByteToUInt32();
            psd.LayerInfo = LayerInformationParser.PaseLayerInfo(new MemoryStream(stream.ReadBytes(psd.LayerAndMaskInformationSectionLength)));

            return psd;
        }


        [Serializable]
        public class PSDLowLevelData
        {
            // File Header Section
            public ushort channels;
            public uint height;
            public uint width;
            public ushort Depth;
            public ColorModeEnum ColorMode;
            public enum ColorModeEnum : ushort
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
            public ImageResourceBlock[] ImageResources;

            // LayerAndMaskInformationSection

            public uint LayerAndMaskInformationSectionLength;

            public LayerInfo LayerInfo;
            public GlobalLayerMaskInfo GlobalLayerMaskInfo;
        }
    }
}