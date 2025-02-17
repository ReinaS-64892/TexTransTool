using System.Collections.Generic;
using net.rs64.TexTransTool.MultiLayerImage.LayerData;
using static net.rs64.PSDParser.ChannelImageDataParser;
using static net.rs64.PSDParser.ChannelImageDataParser.ChannelInformation;
using static net.rs64.PSDParser.LayerRecordParser;
using System.Linq;
using net.rs64.PSDParser.AdditionalLayerInfo;
using static net.rs64.TexTransTool.PSDParser.PSDHighLevelParser;

namespace net.rs64.TexTransTool.PSDParser
{
    [SpecialInfoOf(typeof(SoCo))]
    internal class SolidColorLayerParser : ISpecialLayerParser
    {
        public AbstractLayerData Perse(HighLevelParserContext ctx, LayerRecord record, Dictionary<ChannelIDEnum, ChannelImageData> channelInfoAndImage)
        {
            var solidColorData = new SolidColorLayerData();
            var soCo = record.AdditionalLayerInformation.First(i => i is SoCo) as SoCo;

            solidColorData.CopyFromRecord(record, channelInfoAndImage);
            solidColorData.Color = new() { R = (float)(soCo.R / byte.MaxValue), G = (float)(soCo.G / byte.MaxValue), B = (float)(soCo.B / byte.MaxValue) };

            return solidColorData;
        }
    }

}
