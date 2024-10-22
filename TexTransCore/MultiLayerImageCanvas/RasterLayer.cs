#nullable enable
using System;

namespace net.rs64.TexTransCore.MultiLayerImageCanvas
{
    public class RasterLayer<TTCE> : ImageLayer<TTCE>
    where TTCE : ITexTransGetTexture
    , ITexTransLoadTexture
    , ITexTransRenderTextureOperator
    , ITexTransRenderTextureReScaler
    , ITexTranBlending
    {
        public ITTTexture RasterTexture;

        public RasterLayer(bool visible, AlphaMask<TTCE> alphaModifier, AlphaOperation alphaOperation, bool preBlendToLayerBelow, ITTBlendKey blendTypeKey, ITTTexture texture) : base(visible, alphaModifier, alphaOperation, preBlendToLayerBelow, blendTypeKey)
        {
            RasterTexture = texture;
        }

        public override void GetImage(TTCE engine, ITTRenderTexture renderTexture)
        {
            switch (RasterTexture)
            {
                case ITTDiskTexture diskTexture: { engine.LoadTextureWidthAnySize(diskTexture, renderTexture); break; }
                case ITTRenderTexture rt: { engine.CopyRenderTextureMaybeReScale(rt, renderTexture); break; }
            }
        }
    }
}
