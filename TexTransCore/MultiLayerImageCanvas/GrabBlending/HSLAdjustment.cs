#nullable enable
namespace net.rs64.TexTransCore.MultiLayerImageCanvas
{
    public class HSLAdjustment : TTGrabBlending
    {
        [Range(-1, 1)] public float Hue;
        [Range(-1, 1)] public float Saturation;
        [Range(-1, 1)] public float Lightness;

        public HSLAdjustment(ITTComputeKey computeKey, float hue, float saturation, float lightness) : base(computeKey)
        {
            Hue = hue;
            Saturation = saturation;
            Lightness = lightness;
        }
    }
}
