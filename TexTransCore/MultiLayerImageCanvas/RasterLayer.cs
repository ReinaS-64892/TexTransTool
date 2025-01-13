#nullable enable
using System;

namespace net.rs64.TexTransCore.MultiLayerImageCanvas
{
    public class RasterLayer<TTCE> : ImageLayer<TTCE>
    where TTCE : ITexTransCreateTexture
    , ITexTransLoadTexture
    , ITexTransCopyRenderTexture
    , ITexTransComputeKeyQuery
    , ITexTransGetComputeHandler
    , ITexTransDriveStorageBufferHolder
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
                case ITTDiskTexture diskTexture: { engine.LoadTextureWidthAnySize(renderTexture, diskTexture); break; }
                case ITTRenderTexture rt:
                    {
                        if (rt.EqualSize(renderTexture)) engine.CopyRenderTexture(rt, renderTexture);
                        else engine.DefaultResizing(rt, renderTexture);
                        break;
                    }
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            RasterTexture.Dispose();
        }
    }
}
