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
            public BinaryAddress LayersInfoSection;
            public int LayerCount;
            public int LayerCountAbsValue;

            public List<LayerRecord> LayerRecords;
            public List<ChannelImageData> ChannelImageData;

        }
        public static LayerInfo PaseLayerInfo(bool isPSB, BinarySectionStream stream)
        {
            var layerInfo = new LayerInfo();
            var layersInfoSectionLength = isPSB is false ? stream.ReadUInt32() : stream.ReadUInt64();
            layerInfo.LayersInfoSection = stream.PeekToAddress((long)layersInfoSectionLength);

            if (layerInfo.LayersInfoSection.Length == 0) { return layerInfo; }

            ParseLayerRecordAndChannelImage(isPSB, layerInfo, stream.ReadSubSection(layerInfo.LayersInfoSection.Length));

            return layerInfo;
        }

        public static void ParseLayerRecordAndChannelImage(bool isPSB, LayerInfo layerInfo, BinarySectionStream layerInfoStream)
        {
            layerInfo.LayerCount = layerInfoStream.ReadInt16();
            layerInfo.LayerCountAbsValue = Math.Abs(layerInfo.LayerCount);

            var LayerRecordList = new List<LayerRecord>();
            for (int i = 0; layerInfo.LayerCountAbsValue > i; i += 1)
            {
                LayerRecordList.Add(PaseLayerRecord(isPSB, layerInfoStream));
            }
            layerInfo.LayerRecords = LayerRecordList;

            var channelImageData = new List<ChannelImageData>();
            for (int i = 0; layerInfo.LayerCountAbsValue > i; i += 1)
            {
                for (int Ci = 0; layerInfo.LayerRecords[i].ChannelInformationArray.Length > Ci; Ci += 1)
                {
                    channelImageData.Add(PaseChannelImageData(layerInfoStream, layerInfo.LayerRecords[i], Ci));
                }
            }
            layerInfo.ChannelImageData = channelImageData;
        }
    }
}
