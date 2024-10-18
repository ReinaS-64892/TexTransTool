using System;

namespace net.rs64.MultiLayerImage.LayerData
{
    [Serializable]
    public class RasterLayerData : AbstractLayerData
    {
        public ImportRasterImageData RasterTexture;
    }

    public class EmptyOrUnsupported : RasterLayerData { }
}
