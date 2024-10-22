#nullable enable
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
    }
}
