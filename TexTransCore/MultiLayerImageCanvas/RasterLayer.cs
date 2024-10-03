#nullable enable
using System;

namespace net.rs64.TexTransCore.MultiLayerImageCanvas
{
    public class RasterLayer : ImageLayer
    {
        public ITTTexture RasterTexture;

        public RasterLayer(bool visible, AlphaMask alphaModifier, AlphaOperation alphaOperation, bool preBlendToLayerBelow, ITTBlendKey blendTypeKey, ITTTexture texture) : base(visible, alphaModifier, alphaOperation, preBlendToLayerBelow, blendTypeKey)
        {
            RasterTexture = texture;
        }

        public override void GetImage(ITTEngine engine, ITTRenderTexture renderTexture)
        {
            switch (RasterTexture)
            {
                case ITTDiskTexture diskTexture:
                    {
                        engine.LoadTexture(diskTexture, renderTexture);
                        engine.LoadTextureWidthAnySize(diskTexture, renderTexture, null, null);
                        break;
                    }
                case ITTRenderTexture rt:
                    {
                        engine.CopyRenderTexture(rt, renderTexture);
                        break;
                    }
            }
        }
    }
}
