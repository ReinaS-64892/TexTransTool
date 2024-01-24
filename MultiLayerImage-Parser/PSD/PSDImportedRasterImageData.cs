using System;
using net.rs64.MultiLayerImageParser.LayerData;
using UnityEngine;
using static net.rs64.MultiLayerImageParser.PSD.ChannelImageDataParser;
using static net.rs64.MultiLayerImageParser.PSD.LayerRecordParser;

namespace net.rs64.MultiLayerImageParser.PSD
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