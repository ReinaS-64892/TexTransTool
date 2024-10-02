using System;
using System.Collections.Generic;

namespace net.rs64.MultiLayerImage.LayerData
{
    [Serializable]
    internal class CanvasData
    {
        public int Width;
        public int Height;
        public List<AbstractLayerData> RootLayers;
    }
}
