using System;
using net.rs64.TexTransCore.TransTextureCore;
using UnityEngine;

namespace net.rs64.MultiLayerImage.LayerData
{
    [Serializable]
    internal class RasterLayerData : AbstractLayerData
    {
        public ImportRasterImageData RasterTexture;
    }
}