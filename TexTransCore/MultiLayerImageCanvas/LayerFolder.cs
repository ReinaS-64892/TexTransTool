#nullable enable
using System;
using System.Collections.Generic;

namespace net.rs64.TexTransCore.MultiLayerImageCanvas
{
    public class LayerFolder : ImageLayer
    {
        public List<LayerObject> Layers;

        public LayerFolder(bool visible, AlphaMask alphaModifier, AlphaOperation alphaOperation, bool preBlendToLayerBelow, ITTBlendKey blendTypeKey, List<LayerObject> layers) : base(visible, alphaModifier, alphaOperation, preBlendToLayerBelow, blendTypeKey)
        {
            Layers = layers;
        }

        public override void GetImage(ITexTransCoreEngine engine, ITTRenderTexture writeTarget)
        {
            new CanvasContext(engine).EvaluateForFlattened(writeTarget, null, CanvasContext.ToBelowFlattened(Layers));
        }
    }
}
