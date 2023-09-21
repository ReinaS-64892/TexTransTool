
using System;
using UnityEngine;

namespace net.rs64.PSD.parser
{
    [Serializable]
    public class RasterLayer : AbstractLayer
    {
        public Texture2D RasterTexture;
        public Vector2Int TexturePivot;
    }
}