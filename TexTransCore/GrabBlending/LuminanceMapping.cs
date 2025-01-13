#nullable enable
using System;

namespace net.rs64.TexTransCore.MultiLayerImageCanvas
{
    public class LuminanceMapping : ITTGrabBlending
    {
        public ILuminanceMappingGradient Gradient;
        public LuminanceMapping(ILuminanceMappingGradient gradient)
        {
            Gradient = gradient;
        }

        public void GrabBlending<TTCE>(TTCE engine, ITTRenderTexture grabTexture)
        where TTCE : ITexTransCreateTexture
        , ITexTransComputeKeyQuery
        , ITexTransGetComputeHandler
        , ITexTransDriveStorageBufferHolder
        {
            var gradient = new Color[Gradient.RecommendedResolution];
            using var gradTex = engine.CreateRenderTexture(gradient.Length, 1);
            Gradient.WriteGradient(gradient);

            using var computeHandler = engine.GetComputeHandler(engine.GrabBlend[nameof(LuminanceMapping)]);

            var gvBufId = computeHandler.NameToID("gv");
            var gradientTextureID = computeHandler.NameToID("GradientTexture");
            var texID = computeHandler.NameToID("Tex");


            Span<uint> gvBuf = stackalloc uint[4];
            gvBuf[0] = (uint)gradient.Length;

            computeHandler.UploadConstantsBuffer<uint>(gvBufId, gvBuf);

            computeHandler.SetTexture(texID, grabTexture);
            using var storageBuf = engine.SetStorageBufferFromUpload<TTCE, Color>(computeHandler, gradientTextureID, gradient.AsSpan());

            computeHandler.DispatchWithTextureSize(grabTexture);
        }
    }
    public interface ILuminanceMappingGradient
    {
        /// <summary>
        /// どれだけの長さ(解像度)があった方がよいかを示せるが、その長さで WriteSpan が呼ばれる保証はない。
        /// </summary>
        int RecommendedResolution { get; }
        void WriteGradient(Span<Color> writeSpan);
    }
}
