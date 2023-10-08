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


        public static ChannelImageData PaseChannelImageData(Stream stream, LayerRecord refLayerRecord, int ChannelInformationIndex)
        {
            var channelImageData = new ChannelImageData();
            channelImageData.CompressionRawUshort = stream.ReadByteToUInt16();
            channelImageData.Compression = (ChannelImageData.CompressionEnum)channelImageData.CompressionRawUshort;
            var channelInfo = refLayerRecord.ChannelInformationArray[ChannelInformationIndex];
            var Rect = channelInfo.ChannelID != ChannelInformation.ChannelIDEnum.UserLayerMask ? refLayerRecord.RectTangle : refLayerRecord.LayerMaskAdjustmentLayerData.RectTangle;
            var imageLength = (uint)Mathf.Abs(channelInfo.CorrespondingChannelDataLength - 2);
            switch (channelImageData.Compression)
            {
                case ChannelImageData.CompressionEnum.RawData:
                    {
                        channelImageData.ImageData = stream.ReadBytes(imageLength);
                        break;
                    }
                case ChannelImageData.CompressionEnum.RLECompressed:
                    {
                        channelImageData.ImageData = ParseRLECompressed(new MemoryStream(stream.ReadBytes(imageLength)), (uint)Rect.GetHeight());
                        break;
                    }
                case ChannelImageData.CompressionEnum.ZIPWithoutPrediction:
                    {
                        channelImageData.ImageData = stream.ReadBytes(imageLength);
                        Debug.LogWarning("ZIPWithoutPredictionは現在非対応です。");
                        break;
                    }
                case ChannelImageData.CompressionEnum.ZIPWithPrediction:
                    {
                        channelImageData.ImageData = stream.ReadBytes(imageLength);
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





        private static byte[] ParseRLECompressed(Stream rLEStream, uint Height)
        {
            var rawDataList = new List<byte>();
            var lengthShorts = new ushort[Height];

            for (var i = 0; Height > i; i += 1)
            {
                lengthShorts[i] = BitConverter.ToUInt16(ParserUtility.ConvertLittleEndian(rLEStream.ReadBytes(2)), 0);
            }

            foreach (var widthLength in lengthShorts)
            {
                if (widthLength == 0) { continue; }
                var withStream = new MemoryStream(rLEStream.ReadBytes(widthLength));
                rawDataList.AddRange(ParseRLECompressedWidthLine(withStream));
            }


            return rawDataList.ToArray();
        }

        private static List<byte> ParseRLECompressedWidthLine(MemoryStream withStream)
        {
            var rawDataList = new List<byte>();

            while (withStream.Position < withStream.Length)
            {
                var runLength = (sbyte)withStream.ReadByte();
                if (runLength >= 0)
                {
                    var count = (uint)Mathf.Abs(runLength) + 1;
                    rawDataList.AddRange(withStream.ReadBytes(count));
                }
                else
                {
                    var count = (uint)Mathf.Abs(runLength) + 1;
                    var value = (byte)withStream.ReadByte();
                    var addArray = new byte[count];
                    for (var i = 0; addArray.Length > i; i += 1)
                    {
                        addArray[i] = value;
                    }
                    rawDataList.AddRange(addArray);
                }
            }

            return rawDataList;
        }

    }
}