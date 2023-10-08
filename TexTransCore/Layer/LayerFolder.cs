using System;
using System.Collections.Generic;
using UnityEngine;

namespace net.rs64.TexTransCore.Layer
{
    [Serializable]
    public class LayerFolder : AbstractLayer
    {
        public List<AbstractLayer> Layers;
    }
}