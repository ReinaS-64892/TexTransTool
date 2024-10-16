using System;
using System.Collections.Generic;
using net.rs64.MultiLayerImage.LayerData;
using static net.rs64.MultiLayerImage.Parser.PSD.ChannelImageDataParser;
using static net.rs64.MultiLayerImage.Parser.PSD.ChannelImageDataParser.ChannelInformation;
using static net.rs64.MultiLayerImage.Parser.PSD.LayerRecordParser;
using System.Linq;
using net.rs64.TexTransCore;
using static net.rs64.MultiLayerImage.Parser.PSD.PSDHighLevelParser;

namespace net.rs64.MultiLayerImage.Parser.PSD
{
    [SpecialInfoOf(typeof(AdditionalLayerInfo.SoCo))]
    internal class SolidColorLayerParser : ISpecialLayerParser
    {
        public AbstractLayerData Perse(HighLevelParserContext ctx, LayerRecord record, Dictionary<ChannelIDEnum, ChannelImageData> channelInfoAndImage)
        {
            var solidColorData = new SolidColorLayerData();
            var soCo = record.AdditionalLayerInformation.First(i => i is AdditionalLayerInfo.SoCo) as AdditionalLayerInfo.SoCo;

            solidColorData.CopyFromRecord(record, channelInfoAndImage);
            solidColorData.Color = soCo.Color;

            return solidColorData;
        }
    }

}
