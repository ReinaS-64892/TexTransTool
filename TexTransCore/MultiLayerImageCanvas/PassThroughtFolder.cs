#nullable enable
using System;
using System.Collections.Generic;

namespace net.rs64.TexTransCore.MultiLayerImageCanvas
{
    public class PassThoughtFolder : GrabLayer
    {
        public List<LayerObject> Layers;

        public PassThoughtFolder(bool visible, AlphaMask alphaModifier, bool preBlendToLayerBelow, List<LayerObject> layers) : base(visible, alphaModifier, preBlendToLayerBelow)
        {
            Layers = layers;
        }

        public override void GrabImage(ITTEngine engine, EvaluateContext evaluateContext, ITTRenderTexture grabTexture)
        {
            using (var nEvalCtx = EvaluateContext.NestContext(engine, grabTexture.Width, grabTexture.Hight, evaluateContext, AlphaModifier, null))
            {
                new CanvasContext(engine).EvaluateForFlattened(grabTexture, evaluateContext, CanvasContext.ToBelowFlattened(Layers));
            }
        }
    }
}
