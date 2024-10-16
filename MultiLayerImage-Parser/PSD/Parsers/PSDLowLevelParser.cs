using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static net.rs64.MultiLayerImage.Parser.PSD.GlobalLayerMaskInformationParser;
using static net.rs64.MultiLayerImage.Parser.PSD.LayerInformationParser;
using static net.rs64.MultiLayerImage.Parser.PSD.PSDParserImageResourceBlocksParser;
using Unity.Collections;

namespace net.rs64.MultiLayerImage.Parser.PSD
{

    //https://www.adobe.com/devnet-apps/photoshop/fileformatashtml/

    internal static class PSDLowLevelParser
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
        public static void LoadImageData(byte[] psdByte, PSDLowLevelData data, Span<byte> write)
        {
            var buffer = new byte[data.ImageDataBinaryAddress.Length];
            psdByte.CopyTo(buffer, data.ImageDataBinaryAddress.StartAddress);
            var imageDataSlice = buffer.AsSpan();
            if ((ChannelImageDataParser.ChannelImageData.GetImageByteCount((int)data.Width, (int)data.Height, data.BitDepth) * data.Channels) != write.Length) { throw new ArgumentException(); }
            if (data.IsPSB is false)
                switch (data.BitDepth, data.Channels, (ChannelImageDataParser.ChannelImageData.CompressionEnum)data.ImageDataCompression)
                {
                    case (1, 1, ChannelImageDataParser.ChannelImageData.CompressionEnum.RawData):
                        {
                            UnpackingBitArray(imageDataSlice, write);
                            break;
                        }


                    case (8, 3, ChannelImageDataParser.ChannelImageData.CompressionEnum.RLECompressed):
                        {
                            ChannelImageDataParser.ParseRLECompressed(imageDataSlice, data.Width, data.Height * data.Channels, write);
                            PackingRGB32(data, write);
                            break;
                        }
                    case (8, 4, ChannelImageDataParser.ChannelImageData.CompressionEnum.RLECompressed):
                        {

                            ChannelImageDataParser.ParseRLECompressed(imageDataSlice, data.Width, data.Height * data.Channels, write);
                            PackingRGBA32(data, write);
                            break;
                        }


                    case (16, 4, ChannelImageDataParser.ChannelImageData.CompressionEnum.RawData):
                        {
                            imageDataSlice.CopyTo(write);
                            PackingRGBAUshort(data, write);
                            break;
                        }



                    case (32, 4, ChannelImageDataParser.ChannelImageData.CompressionEnum.RawData):
                        {
                            imageDataSlice.CopyTo(write);
                            PackingRGBAFloat(data, write);
                            break;
                        }

                }
            else
                switch (data.BitDepth, data.Channels, (ChannelImageDataParser.ChannelImageData.CompressionEnum)data.ImageDataCompression)
                {
                    case (8, 4, ChannelImageDataParser.ChannelImageData.CompressionEnum.RLECompressed):
                        {
                            ChannelImageDataParser.ParseRLECompressedWithPSB(imageDataSlice, data.Width, data.Height * data.Channels, write);
                            PackingRGBA32(data, write);
                            break;
                        }

                }

            static void PackingRGBA32(PSDLowLevelData data, Span<byte> write)
            {
                var width = (int)data.Width;
                var height = (int)data.Height;
                var channelByteCount = width * height;

                Span<byte> tempBuf = new byte[write.Length];

                var r = write.Slice(channelByteCount * 0, channelByteCount);
                var g = write.Slice(channelByteCount * 1, channelByteCount);
                var b = write.Slice(channelByteCount * 2, channelByteCount);
                var a = write.Slice(channelByteCount * 3, channelByteCount);

                for (var i = 0; channelByteCount > i; i += 1)
                {
                    var wi = i * data.Channels;
                    tempBuf[wi + 0] = r[i];
                    tempBuf[wi + 1] = g[i];
                    tempBuf[wi + 2] = b[i];
                    tempBuf[wi + 3] = a[i];
                }

                tempBuf.CopyTo(write);
            }
            static void PackingRGBAFloat(PSDLowLevelData data, Span<byte> write)
            {
                var width = (int)data.Width;
                var height = (int)data.Height;
                var channelByteCount = width * height * 4;
                var pixelCount = width * height;

                for (var i = 0; (write.Length / 4) > i; i += 1) { FloatEndianFlip(write.Slice(i * 4, 4)); }

                Span<byte> tempBuf = new byte[write.Length];

                var r = write.Slice(channelByteCount * 0, channelByteCount);
                var g = write.Slice(channelByteCount * 1, channelByteCount);
                var b = write.Slice(channelByteCount * 2, channelByteCount);
                var a = write.Slice(channelByteCount * 3, channelByteCount);

                for (var i = 0; pixelCount > i; i += 1)
                {
                    var fi = i * 4;
                    var wi = fi * data.Channels;

                    r.Slice(fi, 4).CopyTo(tempBuf.Slice(wi + 4 * 0, 4));
                    g.Slice(fi, 4).CopyTo(tempBuf.Slice(wi + 4 * 1, 4));
                    b.Slice(fi, 4).CopyTo(tempBuf.Slice(wi + 4 * 2, 4));
                    a.Slice(fi, 4).CopyTo(tempBuf.Slice(wi + 4 * 3, 4));
                }

                tempBuf.CopyTo(write);

            }
            static void FloatEndianFlip(Span<byte> bytes)
            {
                (bytes[0], bytes[3]) = (bytes[3], bytes[0]);
                (bytes[1], bytes[2]) = (bytes[2], bytes[1]);
            }
            static void PackingRGBAUshort(PSDLowLevelData data, Span<byte> write)
            {
                var width = (int)data.Width;
                var height = (int)data.Height;
                var channelByteCount = width * height * 2;
                var pixelCount = width * height;

                for (var i = 0; (write.Length / 2) > i; i += 1) { UshortEndianFlip(write.Slice(i * 2, 2)); }

                Span<byte> tempBuf = new byte[write.Length];

                var r = write.Slice(channelByteCount * 0, channelByteCount);
                var g = write.Slice(channelByteCount * 1, channelByteCount);
                var b = write.Slice(channelByteCount * 2, channelByteCount);
                var a = write.Slice(channelByteCount * 3, channelByteCount);

                for (var i = 0; pixelCount > i; i += 1)
                {
                    var fi = i * 2;
                    var wi = fi * data.Channels;

                    r.Slice(fi, 2).CopyTo(tempBuf.Slice(wi + 2 * 0, 2));
                    g.Slice(fi, 2).CopyTo(tempBuf.Slice(wi + 2 * 1, 2));
                    b.Slice(fi, 2).CopyTo(tempBuf.Slice(wi + 2 * 2, 2));
                    a.Slice(fi, 2).CopyTo(tempBuf.Slice(wi + 2 * 3, 2));
                }

                tempBuf.CopyTo(write);

            }
            static void UshortEndianFlip(Span<byte> bytes)
            {
                (bytes[0], bytes[1]) = (bytes[1], bytes[0]);
            }

            static void PackingRGB32(PSDLowLevelData data, Span<byte> write)
            {
                var width = (int)data.Width;
                var height = (int)data.Height;
                var channelByteCount = width * height;

                Span<byte> tempBuf = new byte[write.Length];

                var r = write.Slice(channelByteCount * 0, channelByteCount);
                var g = write.Slice(channelByteCount * 1, channelByteCount);
                var b = write.Slice(channelByteCount * 2, channelByteCount);

                for (var i = 0; channelByteCount > i; i += 1)
                {
                    var wi = i * 3;
                    tempBuf[wi + 0] = r[i];
                    tempBuf[wi + 1] = g[i];
                    tempBuf[wi + 2] = b[i];
                }

                tempBuf.CopyTo(write);
            }

            static void UnpackingBitArray(Span<byte> imageDataSlice, Span<byte> write)
            {
                for (var i = 0; imageDataSlice.Length > i; i += 1)
                {
                    var val = imageDataSlice[i];

                    var wi = i * 8;

                    write[wi + 7] = (val & 0b00000001) == 0 ? byte.MaxValue : byte.MinValue;
                    write[wi + 6] = (val & 0b00000010) == 0 ? byte.MaxValue : byte.MinValue;
                    write[wi + 5] = (val & 0b00000100) == 0 ? byte.MaxValue : byte.MinValue;
                    write[wi + 4] = (val & 0b00001000) == 0 ? byte.MaxValue : byte.MinValue;
                    write[wi + 3] = (val & 0b00010000) == 0 ? byte.MaxValue : byte.MinValue;
                    write[wi + 2] = (val & 0b00100000) == 0 ? byte.MaxValue : byte.MinValue;
                    write[wi + 1] = (val & 0b01000000) == 0 ? byte.MaxValue : byte.MinValue;
                    write[wi + 0] = (val & 0b10000000) == 0 ? byte.MaxValue : byte.MinValue;
                }
            }
        }


        [Serializable]
        internal class PSDLowLevelData
        {
            // File Header Section
            public ushort Version;
            public bool IsPSB;

            public ushort Channels;
            public uint Height;
            public uint Width;
            public ushort BitDepth;
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
