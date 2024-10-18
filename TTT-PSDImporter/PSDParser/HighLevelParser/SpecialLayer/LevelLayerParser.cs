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
    [SpecialInfoOf(typeof(AdditionalLayerInfo.levl))]
    internal class LevelLayerParser : ISpecialLayerParser
    {
        public AbstractLayerData Perse(HighLevelParserContext ctx, LayerRecord record, Dictionary<ChannelIDEnum, ChannelImageData> channelInfoAndImage)
        {
            var levelData = new LevelAdjustmentLayerData();
            var levl = record.AdditionalLayerInformation.First(i => i is AdditionalLayerInfo.levl) as AdditionalLayerInfo.levl;

            levelData.CopyFromRecord(record, channelInfoAndImage);

            levelData.RGB = Convert(levl.RGB);
            levelData.Red = Convert(levl.Red);
            levelData.Green = Convert(levl.Green);
            levelData.Blue = Convert(levl.Blue);

            return levelData;

            static LevelAdjustmentLayerData.LevelData Convert(AdditionalLayerInfo.levl.LevelData levelData)
            {
                var data = new LevelAdjustmentLayerData.LevelData();

                data.InputFloor = levelData.InputFloor / 255f;
                data.InputCeiling = levelData.InputCeiling / 255f;
                data.OutputFloor = levelData.OutputFloor / 255f;
                data.OutputCeiling = levelData.OutputCeiling / 255f;
                data.Gamma = levelData.Gamma * 0.01f;

                return data;
            }
        }
    }

}
