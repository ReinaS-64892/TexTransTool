#nullable enable
using System;
using System.Collections.Generic;

namespace net.rs64.TexTransCore.MultiLayerImageCanvas
{
    public class LayerFolder<TexTransCoreEngine> : ImageLayer<TexTransCoreEngine>
    where TexTransCoreEngine : ITexTransGetTexture
    , ITexTransLoadTexture
    , ITexTransRenderTextureOperator
    , ITexTransRenderTextureReScaler
    , ITexTranBlending
    {
        public List<LayerObject<TexTransCoreEngine>> Layers;

        public LayerFolder(bool visible, AlphaMask<TexTransCoreEngine> alphaModifier, AlphaOperation alphaOperation, bool preBlendToLayerBelow, ITTBlendKey blendTypeKey, List<LayerObject<TexTransCoreEngine>> layers) : base(visible, alphaModifier, alphaOperation, preBlendToLayerBelow, blendTypeKey)
        {
            Layers = layers;
        }

        public override void GetImage(TexTransCoreEngine engine, ITTRenderTexture writeTarget)
        {
            new CanvasContext<TexTransCoreEngine>(engine).EvaluateForFlattened(writeTarget, null, CanvasContext<TexTransCoreEngine>.ToBelowFlattened(Layers));
        }
    }
}
