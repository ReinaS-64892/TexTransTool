using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static net.rs64.PSDParser.LayerRecordParser;
using net.rs64.PSDParser.AdditionalLayerInfo;
using System.Threading.Tasks;
using Unity.Collections;
using System.Buffers.Binary;
using System.IO.Compression;
using net.rs64.ParserUtility;


namespace net.rs64.PSDParser
{
    public static class ChannelImageDataParser
    {
        [Serializable]
        public class ChannelInformation
        {
            public short ChannelIDRawShort;
            public ChannelIDEnum ChannelID;
            public enum ChannelIDEnum
            {
                Red = 0,
                Green = 1,
                Blue = 2,
                Transparency = -1,
                UserLayerMask = -2,
                RealUserLayerMask = -3,
            }

            public ulong CorrespondingChannelDataLength;
        }

        [Serializable]
        public class ChannelImageData
        {
            public ushort CompressionRawUshort;
            public CompressionEnum Compression;
            public enum CompressionEnum
            {
                RawData = 0,
                RLECompressed = 1,
                ZIPWithoutPrediction = 2,
                ZIPWithPrediction = 3,
            }

            //PSDのどこからどこまでの範囲にあるかを指す
            public BinaryAddress ImageDataAddress;

            public void GetImageData(byte[] psdBytes, RectTangle rect, Span<byte> imageSourceBuf, Span<byte> writeSpan)
            {
                var isPSB = BinaryPrimitives.ReadInt16BigEndian(psdBytes.AsSpan(4, 2)) == 2;
                var bitDepth = BinaryPrimitives.ReadInt16BigEndian(psdBytes.AsSpan(22, 2));
                if (writeSpan.Length != GetImageByteCount(rect, bitDepth)) { throw new ArgumentException(); }
                if (imageSourceBuf.Length != ImageDataAddress.Length) { throw new ArgumentException(); }

                psdBytes.LongCopyTo(ImageDataAddress.StartAddress, imageSourceBuf);

                if (isPSB is false)
                {
                    switch (bitDepth)
                    {
                        case 1: { Decompress1Bit(imageSourceBuf, rect, writeSpan); return; }
                        case 8: { Decompress8Bit(imageSourceBuf, rect, writeSpan); return; }
                        case 16: { Decompress16Bit(imageSourceBuf, rect, writeSpan); return; }
                        case 32: { Decompress32Bit(imageSourceBuf, rect, writeSpan); return; }
                    }
                }
                else
                {
                    switch (bitDepth)
                    {
                        case 1: { Decompress1BitWithPSB(imageSourceBuf, rect, writeSpan); return; }
                        case 8: { Decompress8BitWithPSB(imageSourceBuf, rect, writeSpan); return; }
                        case 16: { Decompress16BitWithPSB(imageSourceBuf, rect, writeSpan); return; }
                        case 32: { Decompress32BitWithPSB(imageSourceBuf, rect, writeSpan); return; }
                    }
                }
            }

            private void Decompress1Bit(ReadOnlySpan<byte> imageSourceData, RectTangle rect, Span<byte> writeSpan)
            {
                switch (Compression)
                {
                    case CompressionEnum.RawData: { imageSourceData.CopyTo(writeSpan); return; }


                    default:
                        { throw new NotSupportedException($"{Compression}-1Bit は未知な圧縮です！！！"); }
                }
            }
            private void Decompress8Bit(ReadOnlySpan<byte> imageSourceData, RectTangle rect, Span<byte> writeSpan)
            {

                switch (Compression)
                {
                    case CompressionEnum.RawData: { imageSourceData.CopyTo(writeSpan); return; }
                    case CompressionEnum.RLECompressed: { ParseRLECompressed(imageSourceData, (uint)rect.GetWidth(), (uint)rect.GetHeight(), writeSpan); return; }


                    default:
                        { throw new NotSupportedException($"{Compression}-8Bit は未知な圧縮です！！！"); }
                }
            }
            private void Decompress16Bit(ReadOnlySpan<byte> imageSourceData, RectTangle rect, Span<byte> writeSpan)
            {

                switch (Compression)
                {
                    case CompressionEnum.RawData: { imageSourceData.CopyTo(writeSpan); return; }

                    case CompressionEnum.ZIPWithPrediction:
                        {
                            DecompressZlib(imageSourceData, writeSpan);
                            UnpackPrediction16Bit(writeSpan, rect);
                            return;
                        }


                    default:
                        { throw new NotSupportedException($"{Compression}-16Bit は未知な圧縮です！！！"); }
                }
            }
            private void Decompress32Bit(ReadOnlySpan<byte> imageSourceData, RectTangle rect, Span<byte> writeSpan)
            {

                switch (Compression)
                {
                    case CompressionEnum.RawData: { imageSourceData.CopyTo(writeSpan); return; }

                    case CompressionEnum.ZIPWithPrediction:
                        {
                            DecompressZlib(imageSourceData, writeSpan);
                            UnpackPrediction32Bit(writeSpan, rect);
                            return;
                        }


                    default:
                        { throw new NotSupportedException($"{Compression}-32Bit は未知な圧縮です！！！"); }
                }
            }

            public static void DecompressZlib(ReadOnlySpan<byte> imageSourceData, Span<byte> writeSpan)
            {
                using (var memStream = new MemoryStream(imageSourceData.ToArray(), 2, imageSourceData.Length - 2))
                using (var gzipStream = new DeflateStream(memStream, System.IO.Compression.CompressionMode.Decompress))
                    gzipStream.Read(writeSpan);

            }
            public static void UnpackPrediction16Bit(Span<byte> sourceAndWriteSpan, RectTangle rect)
            {
                UnpackPrediction16Bit(sourceAndWriteSpan, rect.GetWidth(), rect.GetHeight());
            }
            public static void UnpackPrediction16Bit(Span<byte> sourceAndWriteSpan, int width, int height)
            {

                var byteCount = 2;

                var withByteSize = width * byteCount;

                for (var y = 0; height > y; y += 1)
                {
                    var index = y * withByteSize;
                    var widthBuffer = sourceAndWriteSpan.Slice(index, withByteSize);

                    //なんか...16bit int が predilection としてそのまま並んでいるらしい...
                    // 16bit PSD は 半精度浮動小数点ではない。 16Bit 整数だ。

                    widthBuffer.Slice(0, byteCount).Reverse();
                    for (var i = 1; width > i; i += 1)
                    {
                        var x = i * byteCount;
                        var left = widthBuffer.Slice(x, byteCount);
                        var right = widthBuffer.Slice(x - byteCount, byteCount);

                        BinaryPrimitives.WriteInt16LittleEndian(left, (short)(BinaryPrimitives.ReadInt16BigEndian(left) + BinaryPrimitives.ReadInt16LittleEndian(right)));
                    }
                }
            }


            public static void UnpackPrediction32Bit(Span<byte> sourceAndWriteSpan, RectTangle rect)
            {
                UnpackPrediction32Bit(sourceAndWriteSpan, rect.GetWidth(), rect.GetHeight());
            }
            public static void UnpackPrediction32Bit(Span<byte> sourceAndWriteSpan, int width, int height)
            {
                var byteCount = 4;

                var withByteSize = width * byteCount;
                Span<byte> widthBuffer = stackalloc byte[withByteSize];

                for (var y = 0; height > y; y += 1)
                {
                    var index = y * withByteSize;
                    sourceAndWriteSpan.Slice(index, withByteSize).CopyTo(widthBuffer);

                    for (var i = 1; widthBuffer.Length > i; i += 1)
                    {
                        widthBuffer[i] += widthBuffer[i - 1];
                    }

                    // 上位 8bit づつ横すべてがいい感じに並んでる
                    // 32Bit PSD は float こと 単精度浮動小数点だ。

                    var zero = widthBuffer.Slice(width * 0, width);
                    var one = widthBuffer.Slice(width * 1, width);
                    var tow = widthBuffer.Slice(width * 2, width);
                    var three = widthBuffer.Slice(width * 3, width);

                    for (var x = 0; width > x; x += 1)
                    {
                        var writeIndex = index + (x * byteCount);
                        // BigEndian から LittleEndian にここで反転させる。
                        sourceAndWriteSpan[writeIndex + 3] = zero[x];
                        sourceAndWriteSpan[writeIndex + 2] = one[x];
                        sourceAndWriteSpan[writeIndex + 1] = tow[x];
                        sourceAndWriteSpan[writeIndex + 0] = three[x];
                    }
                }
            }

            private void Decompress1BitWithPSB(ReadOnlySpan<byte> imageSourceData, RectTangle rect, Span<byte> writeSpan)
            {
                switch (Compression)
                {
                    case CompressionEnum.RawData: { imageSourceData.CopyTo(writeSpan); return; }


                    default:
                        { throw new NotSupportedException($"{Compression}-1Bit は未知な圧縮です！！！"); }
                }
            }
            private void Decompress8BitWithPSB(ReadOnlySpan<byte> imageSourceData, RectTangle rect, Span<byte> writeSpan)
            {

                switch (Compression)
                {
                    case CompressionEnum.RawData: { imageSourceData.CopyTo(writeSpan); return; }
                    case CompressionEnum.RLECompressed: { ParseRLECompressedWithPSB(imageSourceData, (uint)rect.GetWidth(), (uint)rect.GetHeight(), writeSpan); return; }


                    default:
                        { throw new NotSupportedException($"{Compression}-8Bit は未知な圧縮です！！！"); }
                }
            }
            private void Decompress16BitWithPSB(ReadOnlySpan<byte> imageSourceData, RectTangle rect, Span<byte> writeSpan)
            {

                switch (Compression)
                {
                    case CompressionEnum.RawData: { imageSourceData.CopyTo(writeSpan); return; }

                    case CompressionEnum.ZIPWithPrediction:
                        {
                            DecompressZlib(imageSourceData, writeSpan);
                            UnpackPrediction16Bit(writeSpan, rect);
                            return;
                        }


                    default:
                        { throw new NotSupportedException($"{Compression}-16Bit は未知な圧縮です！！！"); }
                }
            }
            private void Decompress32BitWithPSB(ReadOnlySpan<byte> imageSourceData, RectTangle rect, Span<byte> writeSpan)
            {

                switch (Compression)
                {
                    case CompressionEnum.RawData: { imageSourceData.CopyTo(writeSpan); return; }

                    case CompressionEnum.ZIPWithPrediction:
                        {
                            DecompressZlib(imageSourceData, writeSpan);
                            UnpackPrediction32Bit(writeSpan, rect);
                            return;
                        }


                    default:
                        { throw new NotSupportedException($"{Compression}-32Bit は未知な圧縮です！！！"); }
                }
            }










            public static int GetImageByteCount(RectTangle rect, int bitDepth)
            {
                return GetImageByteCount(rect.GetWidth(), rect.GetHeight(), bitDepth);
            }
            public static int GetImageByteCount(int width, int height, int bitDepth)
            {
                return width * height * BitDepthToByteCount(bitDepth);
            }

            public static int BitDepthToByteCount(int bitDepth)
            {
                return Math.Max(bitDepth / 8, 1);
            }

            internal static void HeightInvert<T>(Span<T> lowMap, Span<T> dist, int width, int height) where T : struct
            {
                for (var y = 0; height > y; y += 1)
                {
                    var from = lowMap.Slice((height - 1 - y) * width, width);
                    from.CopyTo(dist.Slice(y * width, width));
                }
            }





        }


        public static ChannelImageData PaseChannelImageData(BinarySectionStream stream, LayerRecord refLayerRecord, int channelInformationIndex)
        {
            var channelImageData = new ChannelImageData();

            channelImageData.CompressionRawUshort = stream.ReadUInt16();
            channelImageData.Compression = (ChannelImageData.CompressionEnum)channelImageData.CompressionRawUshort;

            var imageLength = refLayerRecord.ChannelInformationArray[channelInformationIndex].CorrespondingChannelDataLength - 2;
            channelImageData.ImageDataAddress = stream.ReadToAddress((long)imageLength);
            return channelImageData;
        }






        public static void ParseRLECompressed(ReadOnlySpan<byte> imageDataSpan, uint Width, uint Height, Span<byte> write)
        {
            int intWidth = (int)Width;
            var position = (int)Height * 2;

            for (var i = 0; Height > i; i += 1)
            {
                var writeSpan = write.Slice(i * intWidth, intWidth);

                var widthRLEBytesCount = BinaryPrimitives.ReadUInt16BigEndian(imageDataSpan.Slice(i * 2, 2));
                if (widthRLEBytesCount == 0) { writeSpan.Fill(byte.MinValue); continue; }

                var rleEncodedSpan = imageDataSpan.Slice(position, widthRLEBytesCount);
                position += widthRLEBytesCount;

                ParseRLECompressedWidthLine(rleEncodedSpan, writeSpan);
            }
        }

        public static void ParseRLECompressedWithPSB(ReadOnlySpan<byte> imageDataSpan, uint Width, uint Height, Span<byte> write)
        {
            int intWidth = (int)Width;
            var position = (int)Height * 4;

            for (var i = 0; Height > i; i += 1)
            {
                var writeSpan = write.Slice(i * intWidth, intWidth);

                var widthRLEBytesCount = BinaryPrimitives.ReadUInt32BigEndian(imageDataSpan.Slice(i * 4, 4));
                if (widthRLEBytesCount == 0) { writeSpan.Fill(byte.MinValue); continue; }

                var rleEncodedSpan = imageDataSpan.Slice(position, (int)widthRLEBytesCount);
                position += (int)widthRLEBytesCount;

                ParseRLECompressedWidthLine(rleEncodedSpan, writeSpan);
            }
        }
        private static void ParseRLECompressedWidthLine(ReadOnlySpan<byte> readRLEBytes, Span<byte> writeWidthLine)
        {
            var writePos = 0;
            var readPos = 0;

            while (readPos < readRLEBytes.Length)
            {
                var runLength = (sbyte)readRLEBytes[readPos++];
                if (runLength >= 0)
                {
                    var count = runLength + 1;
                    var subSpan = readRLEBytes.Slice(readPos, count);
                    var writeSpan = writeWidthLine.Slice(writePos, count);

                    subSpan.CopyTo(writeSpan);

                    writePos += count;
                    readPos += count;
                }
                else
                {
                    var count = (-runLength) + 1;
                    var value = readRLEBytes[readPos++];

                    writeWidthLine.Slice(writePos, count).Fill(value);

                    writePos += count;
                }
            }

        }





    }
}
