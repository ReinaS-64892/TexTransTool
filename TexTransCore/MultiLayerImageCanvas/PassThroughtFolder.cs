#nullable enable
using System;
using System.Collections.Generic;

namespace net.rs64.TexTransCore.MultiLayerImageCanvas
{
    public class PassThoughtFolder<TTCE> : GrabLayer<TTCE>
    where TTCE : ITexTransCreateTexture
    , ITexTransLoadTexture
    , ITexTransCopyRenderTexture
    , ITexTransComputeKeyQuery
    , ITexTransGetComputeHandler
    {
        public List<LayerObject<TTCE>> Layers;

        public PassThoughtFolder(bool visible, AlphaMask<TTCE> alphaModifier, bool preBlendToLayerBelow, List<LayerObject<TTCE>> layers) : base(visible, alphaModifier, preBlendToLayerBelow)
        {
            Layers = layers;
        }

        public override void GrabImage(TTCE engine, EvaluateContext<TTCE> evaluateContext, ITTRenderTexture grabTexture)
        {
            using (var nEvalCtx = EvaluateContext<TTCE>.NestContext(engine, grabTexture.Width, grabTexture.Hight, evaluateContext, AlphaMask, null))
            {
                new CanvasContext<TTCE>(engine).EvaluateForFlattened(grabTexture, evaluateContext, CanvasContext<TTCE>.ToBelowFlattened(Layers));
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            foreach (var l in Layers) l.Dispose();
        }
    }
}
