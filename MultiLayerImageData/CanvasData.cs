using System;
using System.Collections.Generic;

namespace net.rs64.TexTransTool.MultiLayerImage.LayerData
{
    [Serializable]
    public class CanvasData
    {
        public int Width;
        public int Height;
        public List<AbstractLayerData> RootLayers;
    }
}
