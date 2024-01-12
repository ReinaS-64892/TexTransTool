using UnityEngine;
namespace net.rs64.TexTransTool.MultiLayerImage
{
    [AddComponentMenu("TexTransTool/MultiLayer/TTT RasterLayer")]
    internal class RasterLayer : AbstractImageLayer
    {
        public Texture2D RasterTexture;

        public override Texture GetImage(int width, int height)
        {
            return RasterTexture;
        }
    }
}