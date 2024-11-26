#nullable enable
using System;

namespace net.rs64.TexTransCore.MultiLayerImageCanvas
{
    public class HSVAdjustment : ITTGrabBlending
    {
        [Range(-1, 1)] public float Hue;
        [Range(-1, 1)] public float Saturation;
        [Range(-1, 1)] public float Value;

        public HSVAdjustment(float hue, float saturation, float value)
        {
            Hue = hue;
            Saturation = saturation;
            Value = value;
        }


        public void GrabBlending<TTCE>(TTCE engine, ITTRenderTexture grabTexture) where TTCE : ITexTransCreateTexture, ITexTransComputeKeyQuery, ITexTransGetComputeHandler
        {
            using var computeHandler = engine.GetComputeHandler(engine.GrabBlend[nameof(HSVAdjustment)]);

            var texID = computeHandler.NameToID("Tex");
            var gvBufId = computeHandler.NameToID("gv");

            Span<float> gvBuf = stackalloc float[3];
            gvBuf[0] = Hue;
            gvBuf[1] = Saturation;
            gvBuf[2] = Value;
            computeHandler.UploadConstantsBuffer<float>(gvBufId, gvBuf);

            computeHandler.SetTexture(texID, grabTexture);

            computeHandler.DispatchWithTextureSize(grabTexture);

        }
    }
}
