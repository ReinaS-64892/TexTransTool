#if UNITY_EDITOR
using UnityEngine;
namespace net.rs64.TexTransTool.MultiLayerImage
{
    [AddComponentMenu("TexTransTool/MultiLayer/TTT RasterLayer")]
    public class RasterLayer : AbstractImageLayer
    {
        public Texture2D RasterTexture;

        public override Texture GetImage()
        {
            return RasterTexture;
        }
    }
}
#endif