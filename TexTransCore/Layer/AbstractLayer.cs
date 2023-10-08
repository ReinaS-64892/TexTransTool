using System;
using net.rs64.TexTransCore.BlendTexture;
using UnityEngine;

namespace net.rs64.TexTransCore.Layer
{

    [Serializable]
    public abstract class AbstractLayer
    {
        public string LayerName;
        public bool TransparencyProtected;
        public bool Visible;
        public float Opacity;
        public bool Clipping;
        public BlendType BlendMode;
        public LayerMask LayerMask;

    }

    public class LayerMask
    {
        public bool LayerMaskDisabled;
        public Texture2D MaskTexture;
        public Vector2Int MaskPivot;
    }

}
