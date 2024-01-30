using System;
using net.rs64.TexTransCore.BlendTexture;
using net.rs64.TexTransCore.TransTextureCore;
using UnityEngine;

namespace net.rs64.MultiLayerImage.LayerData
{
    internal class HSVAdjustmentLayerData : AbstractLayerData
    {
        //All -1 ~ 1
        public float Hue;
        public float Saturation;
        public float Lightness;
    }
}