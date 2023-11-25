using System;
using net.rs64.TexTransCore.TransTextureCore;
using UnityEngine;

namespace net.rs64.MultiLayerImageParser.LayerData
{
    [Serializable]
    public class RasterLayerData : AbstractLayerData
    {
        public TwoDimensionalMap<Color32> RasterTexture;
    }
}