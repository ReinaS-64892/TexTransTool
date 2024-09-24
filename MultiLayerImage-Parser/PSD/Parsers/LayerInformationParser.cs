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
            public uint LayersInfoSectionLength;
            public int LayerCount;
            public int LayerCountAbsValue;

            public List<LayerRecord> LayerRecords;
            public List<ChannelImageData> ChannelImageData;

        }
        public static LayerInfo PaseLayerInfo(SubSpanStream stream)
        {
            var layerInfo = new LayerInfo();
            layerInfo.LayersInfoSectionLength = stream.ReadUInt32();
            if (layerInfo.LayersInfoSectionLength == 0) //32Bit PSD で存在することが確認された 謎の 28Byte への迂回
            {
                stream.Position -= 4;
                stream.ReadSubStream(28);
                layerInfo.LayersInfoSectionLength = stream.ReadUInt32();
            }
            layerInfo.LayerCount = stream.ReadInt16();
            layerInfo.LayerCountAbsValue = Math.Abs(layerInfo.LayerCount);

            // var firstPos = stream.Position;

            var LayerRecordList = new List<LayerRecord>();
            for (int i = 0; layerInfo.LayerCountAbsValue > i; i += 1)
            {
                LayerRecordList.Add(PaseLayerRecord(ref stream));
            }
            layerInfo.LayerRecords = LayerRecordList;

            // var movedLength = stream.Position - firstPos;
            // Debug.Log($"moved length:{movedLength} LayersInfoSectionLength:{layerInfo.LayersInfoSectionLength}");

            var channelImageData = new List<ChannelImageData>();
            for (int i = 0; layerInfo.LayerCountAbsValue > i; i += 1)
            {
                for (int Ci = 0; layerInfo.LayerRecords[i].ChannelInformationArray.Length > Ci; Ci += 1)
                {
                    channelImageData.Add(PaseChannelImageData(ref stream, layerInfo.LayerRecords[i], Ci));
                }
            }
            layerInfo.ChannelImageData = channelImageData;

            return layerInfo;
        }
    }
}
