#nullable enable
using System;

namespace net.rs64.TexTransCore.MultiLayerImageCanvas
{
    public class Colorize : ITTGrabBlending
    {
        public Color Color;
        public Colorize(Color color)
        {
            Color = color;
        }

        public void GrabBlending<TTCE>(TTCE engine, ITTRenderTexture grabTexture)
        where TTCE : ITexTransCreateTexture
        , ITexTransComputeKeyQuery
        , ITexTransGetComputeHandler
        , ITexTransDriveStorageBufferHolder
        {
            using var computeHandler = engine.GetComputeHandler(engine.GrabBlend[nameof(Colorize)]);

            var texID = computeHandler.NameToID("Tex");
            var gvBufId = computeHandler.NameToID("gv");

            Span<Color> gvBuf = stackalloc Color[1];
            gvBuf[0] = Color;
            computeHandler.UploadConstantsBuffer<Color>(gvBufId, gvBuf);

            computeHandler.SetTexture(texID, grabTexture);

            computeHandler.DispatchWithTextureSize(grabTexture);

        }
    }
}
