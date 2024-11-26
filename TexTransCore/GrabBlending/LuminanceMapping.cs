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
        where TTCE : ITexTransCreateTexture, ITexTransComputeKeyQuery, ITexTransGetComputeHandler
        {
            throw new NotImplementedException();
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
