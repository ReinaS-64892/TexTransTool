using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using static net.rs64.MultiLayerImageParser.PSD.LayerRecordParser;
using System.Threading.Tasks;
using Debug = UnityEngine.Debug;


namespace net.rs64.MultiLayerImageParser.PSD
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

            public int CorrespondingChannelDataLength;
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
            [NonSerialized] public byte[] ImageData;
        }


        public static (ChannelImageData data, Task<byte[]> DecompressTask) PaseChannelImageData(ref SubSpanStream stream, LayerRecord refLayerRecord, int ChannelInformationIndex)
        {
            var channelImageData = new ChannelImageData();
            channelImageData.CompressionRawUshort = stream.ReadUInt16();
            channelImageData.Compression = (ChannelImageData.CompressionEnum)channelImageData.CompressionRawUshort;
            var channelInfo = refLayerRecord.ChannelInformationArray[ChannelInformationIndex];
            var Rect = channelInfo.ChannelID != ChannelInformation.ChannelIDEnum.UserLayerMask ? refLayerRecord.RectTangle : refLayerRecord.LayerMaskAdjustmentLayerData.RectTangle;
            var imageLength = (uint)Mathf.Abs(channelInfo.CorrespondingChannelDataLength - 2);
            Task<byte[]> task = null;
            switch (channelImageData.Compression)
            {
                case ChannelImageData.CompressionEnum.RawData:
                    {
                        channelImageData.ImageData = stream.ReadSubStream((int)imageLength).Span.ToArray();
                        break;
                    }
                case ChannelImageData.CompressionEnum.RLECompressed:
                    {
                        var imageSpan = stream.ReadSubStream((int)imageLength);
                        var buffer = ArrayPool<byte>.Shared.Rent(imageSpan.Length);
                        imageSpan.Span.CopyTo(buffer.AsSpan());
                        task = Task.Run(() => ParseRLECompressed(buffer, (uint)Rect.GetWidth(), (uint)Rect.GetHeight()));
                        break;
                    }
                case ChannelImageData.CompressionEnum.ZIPWithoutPrediction:
                    {
                        channelImageData.ImageData = stream.ReadSubStream((int)imageLength).Span.ToArray();
                        Debug.LogWarning("ZIPWithoutPredictionは現在非対応です。");
                        break;
                    }
                case ChannelImageData.CompressionEnum.ZIPWithPrediction:
                    {
                        channelImageData.ImageData = stream.ReadSubStream((int)imageLength).Span.ToArray();
                        Debug.LogWarning("ZIPWithPredictionは現在非対応です。");
                        break;
                    }
                default:
                    {
                        Debug.LogError("PaseError:" + channelImageData.Compression);
                        return (channelImageData, task);
                    }
            }
            return (channelImageData, task);
        }





        private static byte[] ParseRLECompressed(byte[] RentBufBytes, uint Width, uint Height)
        {
            var rLEStream = new SubSpanStream(RentBufBytes);
            var rawDataArray = new byte[(int)(Width * Height)];
            var pos = 0;
            var lengthShorts = new ushort[Height];

            for (var i = 0; Height > i; i += 1)
            {
                lengthShorts[i] = rLEStream.ReadUInt16();
            }

            for (var widthIndex = 0; lengthShorts.Length > widthIndex; widthIndex += 1)
            {
                var widthLength = lengthShorts[widthIndex];
                if (widthLength == 0) { continue; }

                var withStream = rLEStream.ReadSubStream(widthLength);
                var RawWithRendBuf = ParseRLECompressedWidthLine(withStream, Width);
                Array.Copy(RawWithRendBuf, 0, rawDataArray, pos, Width);
                ArrayPool<byte>.Shared.Return(RawWithRendBuf);
                pos += (int)Width;
            }


            ArrayPool<byte>.Shared.Return(RentBufBytes);
            return rawDataArray;
        }

        private static byte[] ParseRLECompressedWidthLine(SubSpanStream withStream, uint width)
        {
            var rawDataBuf = ArrayPool<byte>.Shared.Rent((int)width);
            var pos = 0;

            while (withStream.Position < withStream.Length)
            {
                var runLength = (sbyte)withStream.ReadByte();
                if (runLength >= 0)
                {
                    var count = runLength + 1;
                    var subSpan = withStream.ReadSubStream(count).Span;
                    for (var i = 0; subSpan.Length > i; i += 1)
                    {
                        rawDataBuf[pos] = subSpan[i];
                        pos += 1;
                    }
                    // subSpan.CopyTo(rawDataBuf.AsSpan(pos, count));// なぜか遅い
                    // pos += count;
                }
                else
                {
                    var count = Mathf.Abs(runLength) + 1;
                    var value = (byte)withStream.ReadByte();
                    for (var i = 0; count > i; i += 1)
                    {
                        rawDataBuf[pos] = value;
                        pos += 1;
                    }
                    // rawDataBuf.AsSpan(pos, count).Fill(value);
                    // pos += count;
                }
            }

            return rawDataBuf;
        }

    }
}