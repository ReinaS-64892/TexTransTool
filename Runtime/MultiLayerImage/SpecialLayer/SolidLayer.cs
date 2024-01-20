using net.rs64.TexTransCore.BlendTexture;
using UnityEngine;
using static net.rs64.TexTransCore.TransTextureCore.Utils.TextureUtility;
namespace net.rs64.TexTransTool.MultiLayerImage
{
    [AddComponentMenu("TexTransTool/MultiLayer/TTT SolidLayer")]
    public sealed class SolidLayer : AbstractImageLayer
    {
        public Color Color;
        public override void GetImage(RenderTexture renderTexture, IOriginTexture originTexture)
        {
            TextureBlend.ColorBlit(renderTexture, Color);
        }
    }
}