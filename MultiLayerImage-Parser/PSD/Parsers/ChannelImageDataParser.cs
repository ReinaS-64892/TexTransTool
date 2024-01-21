using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using static net.rs64.MultiLayerImageParser.PSD.LayerRecordParser;
using System.Threading.Tasks;
using Debug = UnityEngine.Debug;
using Unity.Collections;


namespace net.rs64.MultiLayerImageParser.PSD
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
        internal class ChannelImageData : IDisposable
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
            [NonSerialized] public NativeArray<byte> ImageData;

            public void Dispose()
            {
                ImageData.Dispose();
            }
        }


        public static (ChannelImageData data, Task<NativeArray<byte>> DecompressTask) PaseChannelImageData(ref SubSpanStream stream, LayerRecord refLayerRecord, int channelInformationIndex)
        {
            var channelImageData = new ChannelImageData();
            channelImageData.CompressionRawUshort = stream.ReadUInt16();
            channelImageData.Compression = (ChannelImageData.CompressionEnum)channelImageData.CompressionRawUshort;
            var channelInfo = refLayerRecord.ChannelInformationArray[channelInformationIndex];
            var Rect = channelInfo.ChannelID != ChannelInformation.ChannelIDEnum.UserLayerMask ? refLayerRecord.RectTangle : refLayerRecord.LayerMaskAdjustmentLayerData.RectTangle;
            var imageLength = (uint)Mathf.Abs(channelInfo.CorrespondingChannelDataLength - 2);
            Task<NativeArray<byte>> task = null;
            switch (channelImageData.Compression)
            {
                case ChannelImageData.CompressionEnum.RawData:
                    {
                        var imageDataSpan = stream.ReadSubStream((int)imageLength).Span;
                        channelImageData.ImageData = new NativeArray<byte>(imageDataSpan.Length, Allocator.Persistent);
                        channelImageData.ImageData.CopyFrom(imageDataSpan);
                        break;
                    }
                case ChannelImageData.CompressionEnum.RLECompressed:
                    {
                        var imageSpan = stream.ReadSubStream((int)imageLength);
                        var buffer = new NativeArray<byte>(imageSpan.Length, Allocator.Persistent);
                        imageSpan.Span.CopyTo(buffer);
                        task = Task.Run(() => ParseRLECompressed(buffer, (uint)Rect.GetWidth(), (uint)Rect.GetHeight()));
                        break;
                    }
                case ChannelImageData.CompressionEnum.ZIPWithoutPrediction:
                    {
                        var imageDataSpan = stream.ReadSubStream((int)imageLength).Span;
                        channelImageData.ImageData = new NativeArray<byte>(imageDataSpan.Length, Allocator.Persistent);
                        channelImageData.ImageData.CopyFrom(imageDataSpan);
                        Debug.LogWarning("ZIPWithoutPredictionは現在非対応です。");
                        break;
                    }
                case ChannelImageData.CompressionEnum.ZIPWithPrediction:
                    {
                        var imageDataSpan = stream.ReadSubStream((int)imageLength).Span;
                        channelImageData.ImageData = new NativeArray<byte>(imageDataSpan.Length, Allocator.Persistent);
                        channelImageData.ImageData.CopyFrom(imageDataSpan);
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





        private static NativeArray<byte> ParseRLECompressed(NativeArray<byte> RentBufBytes, uint Width, uint Height)
        {
            var rLEStream = new SubSpanStream(RentBufBytes);
            var rawDataArray = new NativeArray<byte>((int)(Width * Height), Allocator.Persistent);
            var lengthShorts = new NativeArray<ushort>((int)Height, Allocator.Persistent);
            var pos = 0;

            for (var i = 0; Height > i; i += 1)
            {
                lengthShorts[i] = rLEStream.ReadUInt16();
            }

            for (var widthIndex = 0; lengthShorts.Length > widthIndex; widthIndex += 1)
            {
                var widthLength = lengthShorts[widthIndex];
                if (widthLength == 0) { continue; }

                var withStream = rLEStream.ReadSubStream(widthLength);
                using (var RawWithRendBuf = ParseRLECompressedWidthLine(withStream, Width))
                {

                    var toSlice = rawDataArray.Slice(pos, (int)Width);
                    RawWithRendBuf.CopyTo(toSlice);
                    pos += (int)Width;

                }
            }


            RentBufBytes.Dispose();
            lengthShorts.Dispose();
            return rawDataArray;
        }

        private static NativeArray<byte> ParseRLECompressedWidthLine(SubSpanStream withStream, uint width)
        {
            var rawDataBuf = new NativeArray<byte>((int)width, Allocator.Persistent);
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