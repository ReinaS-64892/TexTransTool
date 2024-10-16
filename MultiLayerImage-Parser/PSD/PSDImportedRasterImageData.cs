using System;
using net.rs64.MultiLayerImage.LayerData;
using static net.rs64.MultiLayerImage.Parser.PSD.ChannelImageDataParser;
using static net.rs64.MultiLayerImage.Parser.PSD.LayerRecordParser;

namespace net.rs64.MultiLayerImage.Parser.PSD
{
    [Serializable]
    internal abstract class AbstractPSDImportedRasterImageData : ImportRasterImageData
    {
        public RectTangle RectTangle;
    }
    [Serializable]
    internal class PSDImportedRasterImageData : AbstractPSDImportedRasterImageData
    {
        public ChannelImageData R;
        public ChannelImageData G;
        public ChannelImageData B;
        public ChannelImageData A;

    }

    [Serializable]
    internal class PSDImportedRasterMaskImageData : AbstractPSDImportedRasterImageData
    {
        public byte DefaultValue;
        public ChannelImageData MaskImage;

    }
}
