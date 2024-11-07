#nullable enable
using System;

namespace net.rs64.TexTransCore.MultiLayerImageCanvas
{
    public class HSLAdjustment : ITTGrabBlending
    {
        [Range(-1, 1)] public float Hue;
        [Range(-1, 1)] public float Saturation;
        [Range(-1, 1)] public float Lightness;

        public HSLAdjustment(float hue, float saturation, float lightness)
        {
            Hue = hue;
            Saturation = saturation;
            Lightness = lightness;
        }

        public void GrabBlending<TTCE>(TTCE engine, ITTRenderTexture grabTexture) where TTCE : ITexTransCreateTexture, ITexTransComputeKeyQuery, ITexTransGetComputeHandler
        {
            using (var computeHandler = engine.GetComputeHandler(engine.GrabBlend[nameof(HSLAdjustment)]))
            {
                var texID = computeHandler.NameToID("Tex");
                var gvBufId = computeHandler.NameToID("gv");

                Span<float> gvBuf = stackalloc float[3];
                gvBuf[0] = Hue;
                gvBuf[1] = Saturation;
                gvBuf[2] = Lightness;
                computeHandler.UploadCBuffer<float>(gvBufId, gvBuf);

                computeHandler.SetTexture(texID, grabTexture);

                computeHandler.DispatchWithTextureSize(grabTexture);
            }
        }

    }
}
