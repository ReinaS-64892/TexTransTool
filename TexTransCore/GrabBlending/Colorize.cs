#nullable enable
namespace net.rs64.TexTransCore.MultiLayerImageCanvas
{
    public class Colorize : ITTGrabBlending
    {
        public Color Color;
        public Colorize(Color color)
        {
            Color = color;
        }
    }
}
