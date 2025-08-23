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
    [SpecialInfoOf(typeof(grdm))]
    internal class PhotoshopGradationMapLayerParser : ISpecialLayerParser
    {
        public AbstractLayerData Perse(HighLevelParserContext ctx, LayerRecord record, Dictionary<ChannelIDEnum, ChannelImageData> channelInfoAndImage)
        {
            var gradData = new PhotoshopGradationMapLayerData();
            var grdm = record.AdditionalLayerInformation.First(i => i is grdm) as grdm;

            gradData.CopyFromRecord(record, channelInfoAndImage);

            gradData.IsGradientReversed = grdm.IsGradientReversed;
            gradData.IsGradientDithered = grdm.IsGradientDithered;
            gradData.InteropMethod = grdm.GradientInteropMethodKey switch
            {
                "Gcls" => GradientInteropMethod.Classic,
                "Perc" => GradientInteropMethod.Perceptual,
                "Lnr " => GradientInteropMethod.Linear,
                "Smoo" => GradientInteropMethod.Smooth,
                "\0\0\fm" => GradientInteropMethod.Stripes,

                _ => GradientInteropMethod.Classic,
            };
            gradData.Smoothens = grdm.Smoothens / 4096f;

            gradData.ColorKeys = grdm.ColorKeys.Select(c => new ColorKey()
            {
                KeyLocation = c.KeyLocation / 4096f,
                MidLocation = c.MidLocation / 100f,
                Color = new()
                {
                    R = c.Red / (float)ushort.MaxValue,
                    G = c.Green / (float)ushort.MaxValue,
                    B = c.Blue / (float)ushort.MaxValue,
                }
            }).ToArray();

            gradData.TransparencyKeys = grdm.TransparencyKeys.Select(c => new TransparencyKey()
            {
                KeyLocation = c.KeyLocation / 4096f,
                MidLocation = c.MidLocation / 100f,
                Transparency = c.Transparency / 255f,
            }).ToArray();

            return gradData;
        }
    }

}
