#nullable enable
using System;
using System.Collections.Generic;

namespace net.rs64.TexTransCore.MultiLayerImageCanvas
{
    public class LayerFolder<TTCE> : ImageLayer<TTCE>
    where TTCE : ITexTransCreateTexture
    , ITexTransLoadTexture
    , ITexTransCopyRenderTexture
    , ITexTransComputeKeyQuery
    , ITexTransGetComputeHandler
    {
        public List<LayerObject<TTCE>> Layers;

        public LayerFolder(bool visible, AlphaMask<TTCE> alphaModifier, AlphaOperation alphaOperation, bool preBlendToLayerBelow, ITTBlendKey blendTypeKey, List<LayerObject<TTCE>> layers) : base(visible, alphaModifier, alphaOperation, preBlendToLayerBelow, blendTypeKey)
        {
            Layers = layers;
        }

        public override void GetImage(TTCE engine, ITTRenderTexture writeTarget)
        {
            new CanvasContext<TTCE>(engine).EvaluateForFlattened(writeTarget, null, CanvasContext<TTCE>.ToBelowFlattened(Layers));
        }
    }
}
