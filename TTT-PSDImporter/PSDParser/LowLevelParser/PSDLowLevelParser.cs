using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static net.rs64.PSDParser.GlobalLayerMaskInformationParser;
using static net.rs64.PSDParser.LayerInformationParser;
using static net.rs64.PSDParser.PSDParserImageResourceBlocksParser;
using Unity.Collections;
using net.rs64.ParserUtility;

namespace net.rs64.PSDParser
{

    //https://www.adobe.com/devnet-apps/photoshop/fileformatashtml/

    public static class PSDLowLevelParser
    {
        public static readonly byte[] OctBPSSignature = new byte[] { 0x38, 0x42, 0x50, 0x53 };
        public static readonly byte[] OctBIMSignature = new byte[] { 0x38, 0x42, 0x49, 0x4D };
        public static readonly byte[] ZeroPadding = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        public static PSDLowLevelData Parse(string path)
        {
            return Parse(File.ReadAllBytes(path));
        }
        public static PSDLowLevelData Parse(byte[] psdByte)
        {
            var psd = new PSDLowLevelData();

            // Signature ...

            var bsStream = new BinarySectionStream(psdByte);

            if (bsStream.Signature(OctBPSSignature) is false) { throw new System.Exception(); }
            psd.Version = bsStream.ReadUInt16();
            switch (psd.Version)
            {
                case 1: { psd.IsPSB = false; break; }
                case 2: { psd.IsPSB = true; break; }

                default: throw new System.Exception("Unsupported Version PSD or PSB");
            }

            if (bsStream.Signature(ZeroPadding) is false) { throw new System.Exception(); }

            // File Header Section

            psd.Channels = bsStream.ReadUInt16();
            psd.Height = bsStream.ReadUInt32();
            psd.Width = bsStream.ReadUInt32();
            psd.BitDepth = bsStream.ReadUInt16();
            psd.ColorMode = (PSDLowLevelData.ColorModeEnum)bsStream.ReadUInt16();

            // Color Mode Data Section

            var colorModeDataSectionLength = bsStream.ReadUInt32();
            psd.ColorDataSection = bsStream.ReadToAddress(colorModeDataSectionLength);

            // Image Resources Section

            var imageResourcesSectionLength = bsStream.ReadUInt32();
            psd.ImageResourcesSection = bsStream.PeekToAddress(imageResourcesSectionLength);
            if (psd.ImageResourcesSection.Length > 0)
            {
                psd.ImageResources = PaseImageResourceBlocks(bsStream.ReadSubSection(psd.ImageResourcesSection.Length));
            }


            // LayerAndMaskInformationSection
            var layerAndMaskInformationSectionLength = psd.IsPSB is false ? bsStream.ReadUInt32() : bsStream.ReadUInt64();
            psd.LayerAndMaskInformationSection = bsStream.PeekToAddress((long)layerAndMaskInformationSectionLength);
            if (psd.LayerAndMaskInformationSection.Length > 0)
            {
                var layerAndMaskInfoStream = bsStream.ReadSubSection(psd.LayerAndMaskInformationSection.Length);
                psd.LayerInfo = LayerInformationParser.PaseLayerInfo(psd.IsPSB, layerAndMaskInfoStream);
                if ((layerAndMaskInfoStream.Length - layerAndMaskInfoStream.Position) > 0) { psd.GlobalLayerMaskInfo = GlobalLayerMaskInformationParser.PaseGlobalLayerMaskInformation(layerAndMaskInfoStream); }
                if ((layerAndMaskInfoStream.Length - layerAndMaskInfoStream.Position) > 0) { psd.CanvasTypeAdditionalLayerInfo = AdditionalLayerInfo.AdditionalLayerInformationParser.PaseAdditionalLayerInfos(psd.IsPSB, layerAndMaskInfoStream, true); }
            }


            // Image　Data　Section
            psd.RawImageDataCompression = bsStream.ReadUInt16();
            psd.ImageDataCompression = (ChannelImageDataParser.ChannelImageData.CompressionEnum)psd.RawImageDataCompression;
            psd.ImageDataBinaryAddress = bsStream.ReadToAddress(bsStream.Length - bsStream.Position);

            return psd;
        }
        [Serializable]
        public struct PSDImageDataSectionData
        {
            public bool IsPSB;
            public ushort Channels;
            public uint Height;
            public uint Width;
            public ushort BitDepth;
            public ChannelImageDataParser.ChannelImageData.CompressionEnum ImageDataCompression;
            public BinaryAddress ImageDataBinaryAddress;

            public PSDImageDataSectionData(PSDLowLevelData psdLowData)
            {
                IsPSB = psdLowData.IsPSB;
                Channels = psdLowData.Channels;
                Height = psdLowData.Height;
                Width = psdLowData.Width;
                BitDepth = psdLowData.BitDepth;
                ImageDataCompression = psdLowData.ImageDataCompression;
                ImageDataBinaryAddress = psdLowData.ImageDataBinaryAddress;
            }
        }
        public static void LoadImageDataSectionChannelImageArray(Span<byte> imageDataSection, PSDImageDataSectionData data, Span<byte> write)
        {
            if ((ChannelImageDataParser.GetImageByteCount((int)data.Width, (int)data.Height, data.BitDepth) * data.Channels) != write.Length) { throw new ArgumentException(); }
            if (data.IsPSB is false)
                switch (data.BitDepth, data.Channels, data.ImageDataCompression)
                {
                    case (1, 1, ChannelImageDataParser.ChannelImageData.CompressionEnum.RawData):
                        {
                            ChannelImageDataParser.UnpackingBitArray(imageDataSection, write);
                            break;
                        }

                    case (8, 3, ChannelImageDataParser.ChannelImageData.CompressionEnum.RLECompressed):
                        {
                            ChannelImageDataParser.ParseRLECompressed(imageDataSection, data.Width, data.Height * data.Channels, write);
                            break;
                        }
                    case (8, 4, ChannelImageDataParser.ChannelImageData.CompressionEnum.RLECompressed):
                        {

                            ChannelImageDataParser.ParseRLECompressed(imageDataSection, data.Width, data.Height * data.Channels, write);
                            break;
                        }

                    case (16, 4, ChannelImageDataParser.ChannelImageData.CompressionEnum.RawData):
                        {
                            imageDataSection.CopyTo(write);
                            for (var i = 0; (write.Length / 2) > i; i += 1) { UshortEndianFlip(write.Slice(i * 2, 2)); }
                            break;
                        }

                    case (32, 4, ChannelImageDataParser.ChannelImageData.CompressionEnum.RawData):
                        {
                            imageDataSection.CopyTo(write);
                            for (var i = 0; (write.Length / 4) > i; i += 1) { FloatEndianFlip(write.Slice(i * 4, 4)); }
                            break;
                        }

                }
            else
                switch (data.BitDepth, data.Channels, data.ImageDataCompression)
                {
                    case (8, 4, ChannelImageDataParser.ChannelImageData.CompressionEnum.RLECompressed):
                        {
                            ChannelImageDataParser.ParseRLECompressedWithPSB(imageDataSection, data.Width, data.Height * data.Channels, write);
                            break;
                        }

                }

            static void FloatEndianFlip(Span<byte> bytes)
            {
                (bytes[0], bytes[3]) = (bytes[3], bytes[0]);
                (bytes[1], bytes[2]) = (bytes[2], bytes[1]);
            }
            static void UshortEndianFlip(Span<byte> bytes)
            {
                (bytes[0], bytes[1]) = (bytes[1], bytes[0]);
            }
        }
        public static void ExpandChannelRToRGBPlusFillA(Span<byte> write, ReadOnlySpan<byte> read)
        {
            if ((write.Length / read.Length) != 4) { throw new ArgumentException(); }

            for (var i = 0; read.Length > i; i += 1)
            {
                var wi = i * 4;
                write[wi + 0] = read[i];
                write[wi + 1] = read[i];
                write[wi + 2] = read[i];
                write[wi + 3] = byte.MaxValue;
            }
        }
        public static void PackingR8ImageArray3ToRGBA32(PSDImageDataSectionData data, Span<byte> write, ReadOnlySpan<byte> read)
        {
            var channelPixelCount = (int)data.Width * (int)data.Height;
            var channelParByte = channelPixelCount;

            var r = read.Slice(channelParByte * 0, channelParByte);
            var g = read.Slice(channelParByte * 1, channelParByte);
            var b = read.Slice(channelParByte * 2, channelParByte);

            for (var i = 0; channelPixelCount > i; i += 1)
            {
                var wi = i * 4;
                write[wi + 0] = r[i];
                write[wi + 1] = g[i];
                write[wi + 2] = b[i];
                write[wi + 3] = byte.MaxValue;
            }

            write.CopyTo(write);
        }

        public static void PackingR8ImageArray4ToRGBA32(PSDImageDataSectionData data, Span<byte> write, ReadOnlySpan<byte> read)
        {
            var channelPixelCount = (int)data.Width * (int)data.Height;
            var channelParByte = channelPixelCount;

            var r = read.Slice(channelParByte * 0, channelParByte);
            var g = read.Slice(channelParByte * 1, channelParByte);
            var b = read.Slice(channelParByte * 2, channelParByte);
            var a = read.Slice(channelParByte * 3, channelParByte);

            for (var i = 0; channelPixelCount > i; i += 1)
            {
                var wi = i * data.Channels;
                write[wi + 0] = r[i];
                write[wi + 1] = g[i];
                write[wi + 2] = b[i];
                write[wi + 3] = a[i];
            }
        }
        public static void PackingRUshortImageArray4ToRGBAUshort(PSDImageDataSectionData data, Span<byte> write, ReadOnlySpan<byte> read)
        {
            var channelPixelCount = (int)data.Width * (int)data.Height;
            var channelParByte = (int)data.Width * (int)data.Height * 2;

            var r = read.Slice(channelParByte * 0, channelParByte);
            var g = read.Slice(channelParByte * 1, channelParByte);
            var b = read.Slice(channelParByte * 2, channelParByte);
            var a = read.Slice(channelParByte * 3, channelParByte);

            for (var i = 0; channelPixelCount > i; i += 1)
            {
                var readIndex = i * 2;
                var writeIndex = readIndex * 4;

                r.Slice(readIndex, 2).CopyTo(write.Slice(writeIndex + (2 * 0), 2));
                g.Slice(readIndex, 2).CopyTo(write.Slice(writeIndex + (2 * 1), 2));
                b.Slice(readIndex, 2).CopyTo(write.Slice(writeIndex + (2 * 2), 2));
                a.Slice(readIndex, 2).CopyTo(write.Slice(writeIndex + (2 * 3), 2));
            }
        }
        public static void PackingRFloatImageArray4ToRGBAFloat(PSDImageDataSectionData data, Span<byte> write, ReadOnlySpan<byte> read)
        {
            var channelPixelCount = (int)data.Width * (int)data.Height;
            var channelParByte = (int)data.Width * (int)data.Height * 4;

            var r = read.Slice(channelParByte * 0, channelParByte);
            var g = read.Slice(channelParByte * 1, channelParByte);
            var b = read.Slice(channelParByte * 2, channelParByte);
            var a = read.Slice(channelParByte * 3, channelParByte);

            for (var i = 0; channelPixelCount > i; i += 1)
            {
                var readIndex = i * 4;
                var writeIndex = readIndex * 4;

                r.Slice(readIndex, 4).CopyTo(write.Slice(writeIndex + (4 * 0), 4));
                g.Slice(readIndex, 4).CopyTo(write.Slice(writeIndex + (4 * 1), 4));
                b.Slice(readIndex, 4).CopyTo(write.Slice(writeIndex + (4 * 2), 4));
                a.Slice(readIndex, 4).CopyTo(write.Slice(writeIndex + (4 * 3), 4));
            }
        }



        [Serializable]
        public class PSDLowLevelData
        {
            // File Header Section
            public ushort Version;
            public bool IsPSB;

            public ushort Channels;
            public uint Height;
            public uint Width;
            public ushort BitDepth;
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
            public BinaryAddress ColorDataSection;

            // Image Resources Section

            public BinaryAddress ImageResourcesSection;
            public List<ImageResourceBlock> ImageResources;

            // LayerAndMaskInformationSection

            public BinaryAddress LayerAndMaskInformationSection;

            public LayerInfo LayerInfo;
            public GlobalLayerMaskInfo GlobalLayerMaskInfo;
            public AdditionalLayerInfo.AdditionalLayerInfoBase[] CanvasTypeAdditionalLayerInfo;

            //ImageDataSection
            public ushort RawImageDataCompression;
            public ChannelImageDataParser.ChannelImageData.CompressionEnum ImageDataCompression;
            public BinaryAddress ImageDataBinaryAddress;
        }
    }
}
