
namespace net.rs64.MultiLayerImage.LayerData
{
    internal class HSLAdjustmentLayerData : AbstractLayerData , IGrabTag
    {
        //All -1 ~ 1
        public float Hue;
        public float Saturation;
        public float Lightness;
    }
}
