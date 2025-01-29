#nullable enable
using System;

namespace net.rs64.TexTransCore.MultiLayerImageCanvas
{
    public class SolidColorLayer<TTCE> : ImageLayer<TTCE>
    where TTCE : ITexTransCreateTexture
    , ITexTransLoadTexture
    , ITexTransCopyRenderTexture
    , ITexTransComputeKeyQuery
    , ITexTransGetComputeHandler
    , ITexTransDriveStorageBufferHolder
    {
        public Color Color;
        public SolidColorLayer(bool visible, AlphaMask<TTCE> alphaModifier, AlphaOperation alphaOperation, bool preBlendToLayerBelow, ITTBlendKey blendTypeKey, Color color)
        : base(visible, alphaModifier, alphaOperation, preBlendToLayerBelow, blendTypeKey)
        { Color = color; }
        public override void GetImage(TTCE engine, ITTRenderTexture renderTexture) { engine.ColorFill(renderTexture, Color); }
    }
}
