#nullable enable
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
    }
}
