using System;
using net.rs64.TexTransCore;

namespace net.rs64.MultiLayerImage.Parser.PSD.AdditionalLayerInfo
{

    [Serializable, AdditionalLayerInfoParser("Lr32")]
    internal class Lr32 : AdditionalLayerInfoBase
    {
        public LayerInformationParser.LayerInfo AdditionalLayerInformation;
        public override void ParseAddLY(bool isPSB, SubSpanStream stream)
        {
            var layerInfo = AdditionalLayerInformation = new LayerInformationParser.LayerInfo();
            layerInfo.LayersInfoSectionLength = Length;
            if (layerInfo.LayersInfoSectionLength == 0) { return; }
            LayerInformationParser.ParseLayerRecordAndChannelImage(isPSB, layerInfo, stream);

        }
    }
}
