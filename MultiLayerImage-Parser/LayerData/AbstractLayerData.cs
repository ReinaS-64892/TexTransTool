using System;
using net.rs64.TexTransCore.BlendTexture;
using net.rs64.TexTransCore.TransTextureCore;
using UnityEngine;

namespace net.rs64.MultiLayerImageParser.LayerData
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
        public LowMap<Color32> MaskTexture;
    }

}
