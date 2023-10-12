using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using static net.rs64.PSD.parser.ChannelImageDataParser;
using static net.rs64.PSD.parser.LayerRecordParser;

namespace net.rs64.PSD.parser
{
    public static class LayerInformationParser
    {
        [Serializable]
        public class LayerInfo
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
            layerInfo.LayerCount = stream.ReadInt16();
            layerInfo.LayerCountAbsValue = Mathf.Abs(layerInfo.LayerCount);

            // var firstPos = stream.Position;

            var LayerRecordList = new List<LayerRecord>();
            for (int i = 0; layerInfo.LayerCountAbsValue > i; i += 1)
            {
                LayerRecordList.Add(PaseLayerRecord(ref stream));
            }
            layerInfo.LayerRecords = LayerRecordList;

            // var movedLength = stream.Position - firstPos;
            // Debug.Log($"moved length:{movedLength} LayersInfoSectionLength:{layerInfo.LayersInfoSectionLength}");

            var channelImageDataAndTask = new List<(ChannelImageData, Task<byte[]>)>();
            for (int i = 0; layerInfo.LayerCountAbsValue > i; i += 1)
            {
                for (int Ci = 0; layerInfo.LayerRecords[i].ChannelInformationArray.Length > Ci; Ci += 1)
                {
                    channelImageDataAndTask.Add(PaseChannelImageData(ref stream, layerInfo.LayerRecords[i], Ci));
                }
            }
            var channelImageDataList = AwaitDecompress(channelImageDataAndTask).Result;
            layerInfo.ChannelImageData = channelImageDataList;

            return layerInfo;
        }

        private static async Task<List<ChannelImageData>> AwaitDecompress(List<(ChannelImageData, Task<byte[]>)> channelImageDataAndTask)
        {
            var channelImageDataList = new List<ChannelImageData>(channelImageDataAndTask.Count);
            foreach (var cidATask in channelImageDataAndTask)
            {
                var channelImageData = cidATask.Item1;
                if (cidATask.Item2 != null) { channelImageData.ImageData = await cidATask.Item2.ConfigureAwait(false); }
                channelImageDataList.Add(channelImageData);
            }
            return channelImageDataList;
        }
    }
}