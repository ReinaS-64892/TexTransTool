#nullable enable
using System;

namespace net.rs64.TexTransCore.MultiLayerImageCanvas
{
    public class EmptyLayer<TTCE> : ImageLayer<TTCE>
    where TTCE : ITexTransCreateTexture
    , ITexTransLoadTexture
    , ITexTransCopyRenderTexture
    , ITexTransComputeKeyQuery
    , ITexTransGetComputeHandler
    , ITexTransDriveStorageBufferHolder
    {

        public EmptyLayer(bool visible, AlphaMask<TTCE> alphaModifier, AlphaOperation alphaOperation, bool preBlendToLayerBelow, ITTBlendKey blendTypeKey) : base(visible, alphaModifier, alphaOperation, preBlendToLayerBelow, blendTypeKey) { }
        public override void GetImage(TTCE engine, ITTRenderTexture renderTexture) { engine.ColorFill(renderTexture, Color.Zero); }
        public override void Dispose() { base.Dispose(); }
    }
}
