using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Unity.Collections;
using static net.rs64.MultiLayerImage.Parser.PSD.ChannelImageDataParser;
using static net.rs64.MultiLayerImage.Parser.PSD.LayerRecordParser;

namespace net.rs64.MultiLayerImage.Parser.PSD
{
    internal static class LayerInformationParser
    {
        [Serializable]
        internal class LayerInfo
        {
            public ulong LayersInfoSectionLength;
            public int LayerCount;
            public int LayerCountAbsValue;

            public List<LayerRecord> LayerRecords;
            public List<ChannelImageData> ChannelImageData;

        }
        public static LayerInfo PaseLayerInfo(bool isPSB, ref SubSpanStream stream)
        {
            var layerInfo = new LayerInfo();
            layerInfo.LayersInfoSectionLength = isPSB is false ? stream.ReadUInt32() : stream.ReadUInt64();

            if (layerInfo.LayersInfoSectionLength == 0) { return layerInfo; }

            ParseLayerRecordAndChannelImage(isPSB, layerInfo, stream.ReadSubStream((int)layerInfo.LayersInfoSectionLength));

            return layerInfo;
        }

        public static void ParseLayerRecordAndChannelImage(bool isPSB, LayerInfo layerInfo, SubSpanStream layerInfoStream)
        {
            layerInfo.LayerCount = layerInfoStream.ReadInt16();
            layerInfo.LayerCountAbsValue = Math.Abs(layerInfo.LayerCount);

            var LayerRecordList = new List<LayerRecord>();
            for (int i = 0; layerInfo.LayerCountAbsValue > i; i += 1)
            {
                LayerRecordList.Add(PaseLayerRecord(isPSB, ref layerInfoStream));
            }
            layerInfo.LayerRecords = LayerRecordList;

            var channelImageData = new List<ChannelImageData>();
            for (int i = 0; layerInfo.LayerCountAbsValue > i; i += 1)
            {
                for (int Ci = 0; layerInfo.LayerRecords[i].ChannelInformationArray.Length > Ci; Ci += 1)
                {
                    channelImageData.Add(PaseChannelImageData(ref layerInfoStream, layerInfo.LayerRecords[i], Ci));
                }
            }
            layerInfo.ChannelImageData = channelImageData;
        }
    }
}
