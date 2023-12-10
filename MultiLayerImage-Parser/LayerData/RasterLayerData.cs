using System;
using net.rs64.TexTransCore.TransTextureCore;
using UnityEngine;

namespace net.rs64.MultiLayerImageParser.LayerData
{
    [Serializable]
    internal class RasterLayerData : AbstractLayerData
    {
        public LowMap<Color32> RasterTexture;
    }
}