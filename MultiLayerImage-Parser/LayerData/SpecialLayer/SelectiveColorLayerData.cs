using net.rs64.TexTransCore;

namespace net.rs64.MultiLayerImage.LayerData
{
    internal class SelectiveColorLayerData : AbstractLayerData , IGrabTag
    {
        public Vector4 RedsCMYK;
        public Vector4 YellowsCMYK;
        public Vector4 GreensCMYK;
        public Vector4 CyansCMYK;
        public Vector4 BluesCMYK;
        public Vector4 MagentasCMYK;
        public Vector4 WhitesCMYK;
        public Vector4 NeutralsCMYK;
        public Vector4 BlacksCMYK;
        public bool IsAbsolute;
    }
}
