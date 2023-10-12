using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using static net.rs64.PSD.parser.LayerRecordParser;


namespace net.rs64.PSD.parser
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


        public static ChannelImageData PaseChannelImageData(ref SubSpanStream stream, LayerRecord refLayerRecord, int ChannelInformationIndex)
        {
            var channelImageData = new ChannelImageData();
            channelImageData.CompressionRawUshort = stream.ReadUInt16();
            channelImageData.Compression = (ChannelImageData.CompressionEnum)channelImageData.CompressionRawUshort;
            var channelInfo = refLayerRecord.ChannelInformationArray[ChannelInformationIndex];
            var Rect = channelInfo.ChannelID != ChannelInformation.ChannelIDEnum.UserLayerMask ? refLayerRecord.RectTangle : refLayerRecord.LayerMaskAdjustmentLayerData.RectTangle;
            var imageLength = (uint)Mathf.Abs(channelInfo.CorrespondingChannelDataLength - 2);
            switch (channelImageData.Compression)
            {
                case ChannelImageData.CompressionEnum.RawData:
                    {
                        channelImageData.ImageData = stream.ReadSubStream((int)imageLength).Span.ToArray();
                        break;
                    }
                case ChannelImageData.CompressionEnum.RLECompressed:
                    {
                        channelImageData.ImageData = ParseRLECompressed(stream.ReadSubStream((int)imageLength), (uint)Rect.GetWidth(), (uint)Rect.GetHeight());
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
                        return channelImageData;
                    }
            }
            return channelImageData;
        }





        private static byte[] ParseRLECompressed(SubSpanStream rLEStream, uint Width, uint Height)
        {
            var rawDataList = new byte[(int)(Width * Height)];
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
                var raeWith = ParseRLECompressedWidthLine(withStream, Width);
                raeWith.CopyTo(rawDataList, pos);
                pos += raeWith.Length;
            }


            return rawDataList;
        }

        private static byte[] ParseRLECompressedWidthLine(SubSpanStream withStream, uint width)
        {
            var rawDataList = new byte[(int)width];
            var pos = 0;

            while (withStream.Position < withStream.Length)
            {
                var runLength = (sbyte)withStream.ReadByte();
                if (runLength >= 0)
                {
                    var count = Mathf.Abs(runLength) + 1;
                    var subSpan = withStream.ReadSubStream(count).Span;
                    for (var i = 0; subSpan.Length > i; i += 1)
                    {
                        rawDataList[pos] = subSpan[i];
                        pos += 1;
                    }
                }
                else
                {
                    var count = Mathf.Abs(runLength) + 1;
                    var value = (byte)withStream.ReadByte();
                    for (var i = 0; count > i; i += 1)
                    {
                        rawDataList[pos] = value;
                        pos += 1;
                    }
                }
            }

            return rawDataList;
        }

    }
}