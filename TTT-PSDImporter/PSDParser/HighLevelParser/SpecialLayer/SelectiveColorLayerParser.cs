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
    [SpecialInfoOf(typeof(AdditionalLayerInfo.selc))]
    internal class SelectiveColorLayerParser : ISpecialLayerParser
    {
        public AbstractLayerData Perse(HighLevelParserContext ctx, LayerRecord record, Dictionary<ChannelIDEnum, ChannelImageData> channelInfoAndImage)
        {
            var selectiveColorData = new SelectiveColorLayerData();
            var selc = record.AdditionalLayerInformation.First(i => i is AdditionalLayerInfo.selc) as AdditionalLayerInfo.selc;

            selectiveColorData.CopyFromRecord(record, channelInfoAndImage);

            selectiveColorData.RedsCMYK = selc.RedsCMYK;
            selectiveColorData.YellowsCMYK = selc.YellowsCMYK;
            selectiveColorData.GreensCMYK = selc.GreensCMYK;
            selectiveColorData.CyansCMYK = selc.CyansCMYK;
            selectiveColorData.BluesCMYK = selc.BluesCMYK;
            selectiveColorData.MagentasCMYK = selc.MagentasCMYK;
            selectiveColorData.WhitesCMYK = selc.WhitesCMYK;
            selectiveColorData.NeutralsCMYK = selc.NeutralsCMYK;
            selectiveColorData.BlacksCMYK = selc.BlacksCMYK;

            selectiveColorData.IsAbsolute = selc.IsAbsolute;

            return selectiveColorData;
        }
    }

}
