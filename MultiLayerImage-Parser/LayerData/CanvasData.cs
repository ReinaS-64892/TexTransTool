using System;
using System.Collections.Generic;
using net.rs64.TexTransCore.BlendTexture;
using UnityEngine;

namespace net.rs64.MultiLayerImage.LayerData
{
    [Serializable]
    internal class CanvasData
    {
        public Vector2Int Size;
        public List<AbstractLayerData> RootLayers;
    }
}