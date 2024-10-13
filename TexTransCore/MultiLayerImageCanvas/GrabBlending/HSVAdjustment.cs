#nullable enable
namespace net.rs64.TexTransCore.MultiLayerImageCanvas
{
    public class HSVAdjustment : TTGrabBlending
    {
        [Range(-1, 1)] public float Hue;
        [Range(-1, 1)] public float Saturation;
        [Range(-1, 1)] public float Value;

        public HSVAdjustment(ITTComputeKey computeKey, float hue, float saturation, float value) : base(computeKey)
        {
            Hue = hue;
            Saturation = saturation;
            Value = value;
        }
    }
}
