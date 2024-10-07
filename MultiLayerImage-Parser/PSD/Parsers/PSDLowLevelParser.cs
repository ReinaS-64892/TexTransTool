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
        public static PSDLowLevelData Parse(string path)
        {
            using (var fileStream = File.OpenRead(path))
            {
                var nativePSDData = new byte[fileStream.Length];
                fileStream.Read(nativePSDData);
                return Parse(nativePSDData);
            }
        }
        public static PSDLowLevelData Parse(Span<byte> psdByte)
        {
            var psd = new PSDLowLevelData();

            // Signature ...

            var spanStream = new SubSpanStream(psdByte);

            if (!spanStream.ReadSubStream(4).Span.SequenceEqual(OctBPSSignature)) { throw new System.Exception(); }
            //if (!spanStream.ReadSubStream(2).Span.SequenceEqual(new byte[] { 0x00, 0x01 })) { throw new System.Exception(); }
            psd.Version = spanStream.ReadUInt16();
            switch (psd.Version)
            {
                case 1: { psd.IsPSB = false; break; }
                case 2: { psd.IsPSB = true; break; }

                default: throw new System.Exception("Unsupported Version PSD or PSB");
            }

            if (!spanStream.ReadSubStream(6).Span.SequenceEqual(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 })) { throw new System.Exception(); }

            // File Header Section

            psd.Channels = spanStream.ReadUInt16();
            psd.Height = spanStream.ReadUInt32();
            psd.Width = spanStream.ReadUInt32();
            psd.BitDepth = spanStream.ReadUInt16();
            psd.ColorMode = (PSDLowLevelData.ColorModeEnum)spanStream.ReadUInt16();

            // Color Mode Data Section

            psd.ColorModeDataSectionLength = spanStream.ReadUInt32();
            psd.ColorDataStartIndex = spanStream.ReadSubStream((int)psd.ColorModeDataSectionLength).FirstToPosition;

            // Image Resources Section

            psd.ImageResourcesSectionLength = spanStream.ReadUInt32();
            if (psd.ImageResourcesSectionLength > 0)
            {
                psd.ImageResources = PaseImageResourceBlocks(spanStream.ReadSubStream((int)psd.ImageResourcesSectionLength));
            }


            // LayerAndMaskInformationSection
            psd.LayerAndMaskInformationSectionLength = psd.IsPSB is false ? spanStream.ReadUInt32() : spanStream.ReadUInt64();
            if (psd.LayerAndMaskInformationSectionLength > 0)
            {
                var layerAndMaskInfoStream = spanStream.ReadSubStream((int)psd.LayerAndMaskInformationSectionLength);
                psd.LayerInfo = LayerInformationParser.PaseLayerInfo(psd.IsPSB, ref layerAndMaskInfoStream);
                psd.GlobalLayerMaskInfo = GlobalLayerMaskInformationParser.PaseGlobalLayerMaskInformation(ref layerAndMaskInfoStream);
                psd.CanvasTypeAdditionalLayerInfo = AdditionalLayerInfo.AdditionalLayerInformationParser.PaseAdditionalLayerInfos(psd.IsPSB, layerAndMaskInfoStream, true);
            }


            // Image　Data　Section
            psd.ImageDataCompression = spanStream.ReadUInt16();
            psd.ImageDataStartIndex = spanStream.Position;

            return psd;
        }
        public static void LoadImageData(Span<byte> psdByte, PSDLowLevelData data, Span<byte> write)
        {
            var imageDataSlice = psdByte.Slice((int)data.ImageDataStartIndex, (int)(psdByte.Length - data.ImageDataStartIndex));
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

            public uint ColorModeDataSectionLength;
            public long ColorDataStartIndex;

            // Image Resources Section

            public uint ImageResourcesSectionLength;
            public List<ImageResourceBlock> ImageResources;

            // LayerAndMaskInformationSection

            public ulong LayerAndMaskInformationSectionLength;

            public LayerInfo LayerInfo;
            public GlobalLayerMaskInfo GlobalLayerMaskInfo;
            public AdditionalLayerInfo.AdditionalLayerInfoBase[] CanvasTypeAdditionalLayerInfo;

            //ImageDataSection
            public ushort ImageDataCompression;
            public long ImageDataStartIndex;
        }
    }
}
