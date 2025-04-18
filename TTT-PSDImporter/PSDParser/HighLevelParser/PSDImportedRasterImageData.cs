using System;
using net.rs64.TexTransTool.MultiLayerImage.LayerData;
using static net.rs64.PSDParser.ChannelImageDataParser;
using static net.rs64.PSDParser.LayerRecordParser;

namespace net.rs64.TexTransTool.PSDParser
{
    [Serializable]
    public abstract class AbstractPSDImportedRasterImageData : ImportRasterImageData
    {
        public RectTangle RectTangle;
    }
    [Serializable]
    public class PSDImportedRasterImageData : AbstractPSDImportedRasterImageData
    {
        public ChannelImageData R;
        public ChannelImageData G;
        public ChannelImageData B;
        public ChannelImageData A;

    }

    [Serializable]
    public class PSDImportedRasterMaskImageData : AbstractPSDImportedRasterImageData
    {
        public byte DefaultValue;
        public ChannelImageData MaskImage;

    }
}
