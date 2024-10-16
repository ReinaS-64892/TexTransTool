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

    [SpecialInfoOf(typeof(AdditionalLayerInfo.hue2))]
    [SpecialInfoOf(typeof(AdditionalLayerInfo.hueOld))]
    internal class HSLorHSVAdjustmentParser : ISpecialLayerParser
    {
        public AbstractLayerData Perse(HighLevelParserContext ctx, LayerRecord record, Dictionary<ChannelIDEnum, ChannelImageData> channelInfoAndImage)
        {
            var hue = record.AdditionalLayerInformation.First(i => i is AdditionalLayerInfo.hue) as AdditionalLayerInfo.hue;

            if (hue.Colorization) { Debug.Log($"Colorization of {record.LayerName} is no supported"); }

            if (ctx.ImportMode is not PSDImportMode.ClipStudioPaint)
            {
                var hueData = new HSLAdjustmentLayerData();

                hueData.CopyFromRecord(record, channelInfoAndImage);

                hueData.Hue = hue.Hue / (float)(hue.IsOld is false ? 180f : 100f);
                hueData.Saturation = hue.Saturation / 100f;
                hueData.Lightness = hue.Lightness / 100f;

                return hueData;
            }
            else
            {
                var hueData = new HSVAdjustmentLayerData();

                hueData.CopyFromRecord(record, channelInfoAndImage);

                hueData.Hue = hue.Hue / (float)(hue.IsOld is false ? 180f : 100f);
                hueData.Saturation = hue.Saturation / 100f;
                hueData.Value = hue.Lightness / 100f;

                return hueData;

            }
        }
    }

}
