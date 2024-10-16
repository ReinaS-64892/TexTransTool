#nullable enable
namespace net.rs64.TexTransCore.MultiLayerImageCanvas
{
    public class Colorize : TTGrabBlending
    {
        public Color Color;
        public Colorize(ITTComputeKey computeKey, Color color) : base(computeKey)
        {
            Color = color;
        }
    }
}
