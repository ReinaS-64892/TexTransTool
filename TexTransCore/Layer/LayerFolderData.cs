using System;
using System.Collections.Generic;
using UnityEngine;

namespace net.rs64.TexTransCore.Layer
{
    [Serializable]
    public class LayerFolderData : AbstractLayerData
    {
        public bool PassThrough;
        public List<AbstractLayerData> Layers;
    }
}