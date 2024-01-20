using UnityEngine;
namespace net.rs64.TexTransTool.MultiLayerImage
{
    [AddComponentMenu("TexTransTool/MultiLayer/TTT RasterLayer")]
    public sealed class RasterLayer : AbstractImageLayer
    {
        public Texture2D RasterTexture;

        public override void GetImage(RenderTexture renderTexture, IOriginTexture originTexture)
        {
            originTexture.WriteOriginalTexture(RasterTexture, renderTexture);
        }
    }
}