#nullable enable
using System;

namespace net.rs64.TexTransCore.MultiLayerImageCanvas
{
    public class LuminanceMapping : TTGrabBlending
    {
        public ILuminanceMappingGradient Gradient;
        public LuminanceMapping(ITTComputeKey computeKey, ILuminanceMappingGradient gradient) : base(computeKey)
        {
            Gradient = gradient;
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
