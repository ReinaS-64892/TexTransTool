using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static net.rs64.MultiLayerImage.Parser.PSD.LayerRecordParser;
using System.Threading.Tasks;
using Unity.Collections;
using System.Buffers.Binary;


namespace net.rs64.MultiLayerImage.Parser.PSD
{
    internal static class ChannelImageDataParser
    {
        [Serializable]
        internal class ChannelInformation
        {
            public short ChannelIDRawShort;
            public ChannelIDEnum ChannelID;
            internal enum ChannelIDEnum
            {
                Red = 0,
                Green = 1,
                Blue = 2,
                Transparency = -1,
                UserLayerMask = -2,
                RealUserLayerMask = -3,
            }

            public int CorrespondingChannelDataLength;
        }

        [Serializable]
        internal class ChannelImageData
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
            public int StartIndex;
            public int Length;

            public Span<byte> GetImageData(byte[] psdBytes, RectTangle thisRect)
            {
                var data = psdBytes.AsSpan(StartIndex, Length);
                switch (Compression)
                {
                    case CompressionEnum.RawData:
                        {
                            return data;

                        }
                    case CompressionEnum.RLECompressed:
                        {
                            return ParseRLECompressed(data, (uint)thisRect.GetWidth(), (uint)thisRect.GetHeight());
                        }
                    default:
                        throw new NotSupportedException("ZIP圧縮のPSDは非対応です。");
                }
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


        public static ChannelImageData PaseChannelImageData(ref SubSpanStream stream, LayerRecord refLayerRecord, int channelInformationIndex)
        {
            var channelImageData = new ChannelImageData();

            channelImageData.CompressionRawUshort = stream.ReadUInt16();
            channelImageData.Compression = (ChannelImageData.CompressionEnum)channelImageData.CompressionRawUshort;

            var imageLength = (uint)Math.Abs(refLayerRecord.ChannelInformationArray[channelInformationIndex].CorrespondingChannelDataLength - 2);

            var imageData = stream.ReadSubStream((int)imageLength);
            channelImageData.StartIndex = (int)imageData.FirstToPosition;
            channelImageData.Length = imageData.Length;

            return channelImageData;
        }






        private static Span<byte> ParseRLECompressed(Span<byte> imageDataSpan, uint Width, uint Height)
        {
            var rawDataArray = new byte[(int)(Width * Height)].AsSpan();

            int intWidth = (int)Width;
            var position = (int)Height * 2;

            for (var i = 0; Height > i; i += 1)
            {
                var writeSpan = rawDataArray.Slice(i * intWidth, intWidth);

                var widthRLEBytesCount = BinaryPrimitives.ReadUInt16BigEndian(imageDataSpan.Slice(i * 2, 2));
                if (widthRLEBytesCount == 0) { writeSpan.Fill(byte.MinValue); continue; }

                var rleEncodedSpan = imageDataSpan.Slice(position, widthRLEBytesCount);
                position += widthRLEBytesCount;

                ParseRLECompressedWidthLine(writeSpan, rleEncodedSpan);
            }

            return rawDataArray;
        }

        private static void ParseRLECompressedWidthLine(Span<byte> writeWidthLine, Span<byte> readRLEBytes)
        {
            var writePos = 0;
            var readPos = 0;

            while (readPos < readRLEBytes.Length)
            {
                var runLength = (sbyte)readRLEBytes[readPos++]; ;
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
