using System;
using net.rs64.TexTransCore.BlendTexture;
using net.rs64.TexTransCore.TransTextureCore.TransCompute;
using UnityEngine;

namespace net.rs64.MultiLayerImageParser.LayerData
{

    [Serializable]
    public abstract class AbstractLayerData
    {
        public string LayerName;
        public bool TransparencyProtected;
        public bool Visible;
        public float Opacity;
        public bool Clipping;
        public BlendType BlendMode;
        public LayerMaskData LayerMask;

    }
    [Serializable]
    public class LayerMaskData
    {
        public bool LayerMaskDisabled;
        public TwoDimensionalMap<Color32> MaskTexture;
    }

}
