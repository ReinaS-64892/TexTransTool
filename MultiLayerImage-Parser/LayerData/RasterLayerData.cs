using System;

namespace net.rs64.MultiLayerImage.LayerData
{
    [Serializable]
    internal class RasterLayerData : AbstractLayerData
    {
        public ImportRasterImageData RasterTexture;
    }

    internal class EmptyOrUnsupported : RasterLayerData { }
}
