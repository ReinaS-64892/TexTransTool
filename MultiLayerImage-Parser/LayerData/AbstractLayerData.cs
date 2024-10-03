using System;

namespace net.rs64.MultiLayerImage.LayerData
{

    [Serializable]
    internal abstract class AbstractLayerData
    {
        public string LayerName;
        public bool TransparencyProtected;
        public bool Visible;
        public float Opacity;
        public bool Clipping;
        public string BlendTypeKey;
        public LayerMaskData LayerMask;

    }
    [Serializable]
    internal class LayerMaskData
    {
        public bool LayerMaskDisabled;
        public ImportRasterImageData MaskTexture;
    }

}
